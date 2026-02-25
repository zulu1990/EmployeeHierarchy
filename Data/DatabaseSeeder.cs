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

    /// <summary>
    /// Seeds the database with a large number of randomly generated employees.
    /// Creates a realistic organizational hierarchy with multiple levels.
    /// </summary>
    /// <param name="employeeCount">Number of employees to generate (default: 1000)</param>
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

            Console.WriteLine($"✓ Database seeded successfully with {employeeCount} employees.");
            Console.WriteLine($"  - Total records: {_context.Employees.Count()}");
            Console.WriteLine($"  - CEO (no manager): {_context.Employees.Count(e => e.ManagerId == null)}");
            Console.WriteLine($"  - Managers: {_context.Employees.Where(e => e.ManagerId != null).Distinct().Count()}");

            // Calculate hierarchy depth
            var maxDepth = CalculateHierarchyDepth();
            Console.WriteLine($"  - Hierarchy depth: {maxDepth} levels");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error seeding database: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generates a list of random employees with a realistic hierarchical structure.
    /// Creates multiple management chains and ensures the hierarchy is connected.
    /// </summary>
    private List<Employee> GenerateEmployees(int count)
    {
        var employees = new List<Employee>();
        var rng = new Random(42); // Use seed for reproducibility, change if you want different data each time

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
            // Randomly decide whether to create a manager or a regular employee
            // 20% chance to create a new manager (creates deeper hierarchy)
            bool isNewManager = rng.NextDouble() < 0.2 && currentId <= count;

            // Select a random manager from the pool
            int managerId = managersPool[rng.Next(managersPool.Count)];

            var employee = new Employee
            {
                Id = currentId,
                ManagerId = managerId,
                Name = GenerateRandomName()
            };

            employees.Add(employee);

            // If this new employee is a manager, add them to the pool
            if (isNewManager)
            {
                managersPool.Add(currentId);
            }

            currentId++;
        }

        return employees;
    }

    /// <summary>
    /// Generates a random employee name by combining first and last names.
    /// </summary>
    private string GenerateRandomName()
    {
        string firstName = FirstNames[_random.Next(FirstNames.Length)];
        string lastName = LastNames[_random.Next(LastNames.Length)];
        return $"{firstName} {lastName}";
    }

    /// <summary>
    /// Calculates the maximum depth of the organizational hierarchy.
    /// </summary>
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
