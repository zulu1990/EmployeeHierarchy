using EmployeeAPI.Models;

namespace EmployeeAPI.Data;

public class DatabaseSeeder
{
    private readonly EmployeeContext _context;
    private readonly Random _random = new();
    private static readonly string[] FirstNames = new[]
    {
        "John", "Jane", "Michael", "Emily", "David", "Sophia", "James", "Olivia",
        "Robert", "Ava", "William", "Isabella", "Richard", "Mia", "Joseph", "Charlotte",
        "Thomas", "Amelia", "Charles", "Harper", "Christopher", "Evelyn", "Daniel", "Abigail",
        "Matthew", "Elizabeth", "Anthony", "Emma", "Steven", "Ella", "Paul", "Scarlett",
        "Andrew", "Madison", "Joshua", "Zoe", "Kenneth", "Lily", "Kevin", "Kate",
        "Brian", "Victoria", "Edward", "Grace", "Ronald", "Nora", "Timothy", "Chloe"
    };

    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Perez", "Martin", "Lee", "White", "Harris",
        "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Young", "Walker", "Hall",
        "Allen", "King", "Scott", "Green", "Baker", "Adams", "Nelson", "Carter",
        "Roberts", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Reeves"
    };

    public DatabaseSeeder(EmployeeContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(int employeeCount = 1000)
    {
        if (_context.Employees.Any())
        {
            Console.WriteLine("Database already seeded with data.");
            return;
        }

        try
        {
            Console.WriteLine($"Starting to seed database with {employeeCount} employees...");
            var employees = GenerateEmployees(employeeCount);

            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            var maxDepth = CalculateHierarchyDepth();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error seeding database: {ex.Message}");
            throw;
        }
    }
    private List<Employee> GenerateEmployees(int count)
    {
        var employees = new List<Employee>();
        var rng = new Random(42);

        // Create CEO (ID 1, no manager)
        employees.Add(new Employee
        {
            Id = 1,
            ManagerId = null,
            Name = GenerateRandomName()
        });

        // Create middle managers and their subordinates
        int currentId = 2;
        var managersPool = new List<int> { 1 }; // Start with CEO

        while (currentId <= count)
        {
            bool isNewManager = rng.NextDouble() < 0.2 && currentId <= count;

            int managerId = managersPool[rng.Next(managersPool.Count)];

            var employee = new Employee
            {
                Id = currentId,
                ManagerId = managerId,
                Name = GenerateRandomName()
            };

            employees.Add(employee);

            if (isNewManager)
            {
                managersPool.Add(currentId);
            }

            currentId++;
        }

        return employees;
    }
    private string GenerateRandomName()
    {
        string firstName = FirstNames[_random.Next(FirstNames.Length)];
        string lastName = LastNames[_random.Next(LastNames.Length)];
        return $"{firstName} {lastName}";
    }

    private int CalculateHierarchyDepth()
    {
        var allEmployees = _context.Employees.ToList();
        int maxDepth = 0;

        foreach (var employee in allEmployees.Where(e => e.ManagerId == null))
        {
            int depth = GetDepth(employee.Id, allEmployees, 1);
            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    private int GetDepth(int employeeId, List<Employee> allEmployees, int currentDepth)
    {
        var subordinates = allEmployees.Where(e => e.ManagerId == employeeId).ToList();

        if (subordinates.Count == 0)
            return currentDepth;

        int maxSubordinateDepth = 0;
        foreach (var subordinate in subordinates)
        {
            int depth = GetDepth(subordinate.Id, allEmployees, currentDepth + 1);
            maxSubordinateDepth = Math.Max(maxSubordinateDepth, depth);
        }

        return maxSubordinateDepth;
    }
}
