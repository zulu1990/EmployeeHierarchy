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
                SELECT DISTINCT * FROM EmployeeHierarchy
            ")
            .ToListAsync();

        if (employees.Count == 0)
            return null;

        return employees.FirstOrDefault(e => e.Id == employeeId);
    }
}
