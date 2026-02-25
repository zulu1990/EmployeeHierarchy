using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves an employee with all their subordinates and their subordinates recursively.
    /// </summary>
    /// <param name="id">The employee ID</param>
    /// <returns>Employee object with full hierarchy</returns>
    /// <response code="200">Returns the employee with their subordinates</response>
    /// <response code="404">If employee is not found</response>
    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeWithSubordinatesAsync(id);
            
            if (employee == null)
            {
                _logger.LogInformation("Employee with ID {EmployeeId} not found.", id);
                return NotFound(new { message = $"Employee with ID {id} not found." });
            }

            var dto = MapToDto(employee);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the employee." });
        }
    }

    private EmployeeDto MapToDto(Models.Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            ManagerId = employee.ManagerId,
            Name = employee.Name,
            Subordinates = employee.Subordinates.Select(MapToDto).ToList()
        };
    }
}

public record EmployeeDto
{
    public int Id { get; set; }
    public int? ManagerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<EmployeeDto> Subordinates { get; set; } = new();
}
