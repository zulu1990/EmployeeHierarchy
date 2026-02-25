using EmployeeAPI.Data;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Services;

public interface IEmployeeService
{
    Task<Employee?> GetEmployeeWithSubordinatesAsync(int employeeId);
}

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeContext _context;

    public EmployeeService(EmployeeContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves an employee and all their subordinates (recursive) using a recursive CTE query.
    /// This approach is efficient for large datasets with thousands of rows.
    /// </summary>
    public async Task<Employee?> GetEmployeeWithSubordinatesAsync(int employeeId)
    {
        // Use raw SQL with recursive CTE to efficiently fetch the hierarchy
        var employees = await _context.Employees
            .FromSqlInterpolated($@"
                WITH EmployeeHierarchy AS (
                    SELECT Id, ManagerId, Name
                    FROM Employees
                    WHERE Id = {employeeId}
                    UNION ALL
                    SELECT e.Id, e.ManagerId, e.Name
                    FROM Employees e
                    INNER JOIN EmployeeHierarchy eh ON e.ManagerId = eh.Id
                )
                SELECT * FROM EmployeeHierarchy
            ")
            .ToListAsync();

        if (employees.Count == 0)
            return null;

        // Build the hierarchy structure
        var employeeDict = employees.ToDictionary(e => e.Id);
        foreach (var employee in employees)
        {
            if (employee.Id != employeeId && employee.ManagerId.HasValue)
            {
                if (employeeDict.TryGetValue(employee.ManagerId.Value, out var manager))
                {
                    manager.Subordinates.Add(employee);
                }
            }
        }

        return employeeDict[employeeId];
    }
}
