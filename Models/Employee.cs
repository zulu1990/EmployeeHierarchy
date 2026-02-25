namespace EmployeeAPI.Models;

public class Employee
{
    public int Id { get; set; }
    public int? ManagerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Employee> Subordinates { get; set; } = new();
}
