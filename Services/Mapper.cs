using EmployeeAPI.Models;

namespace EmployeeAPI.Services
{
    public static class Mapper
    {
        public static EmployeeDto MapToDto(this Employee employee)
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
}
