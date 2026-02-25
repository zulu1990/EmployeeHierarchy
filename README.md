# Employee Hierarchy API

A high-performance .NET 8 Web API that retrieves employee hierarchies from a SQL Server database. The API uses recursive CTEs for efficient querying of large datasets with thousands of rows.

## Features

- **Hierarchical Employee Retrieval**: Fetch an employee and all their subordinates (and their subordinates, recursively)
- **Efficient Querying**: Uses SQL recursive CTEs for optimal performance with large datasets
- **Comprehensive API Documentation**: Swagger/OpenAPI support included
- **Error Handling**: Proper HTTP status codes and error messages

## Prerequisites

- .NET 8 SDK or later
- SQL Server (LocalDB, Express, or full instance)
- Visual Studio 2022 or VS Code with C# extension

## Database Setup

### 1. Create the Database and Table

Run the following SQL script on your SQL Server instance:

```sql
CREATE DATABASE EmployeeDb;

USE EmployeeDb;

CREATE TABLE Employees (
    Id INT PRIMARY KEY,
    ManagerId INT NULL,
    Name NVARCHAR(100) NOT NULL,
    FOREIGN KEY (ManagerId) REFERENCES Employees(Id)
);
```

### 2. Insert Sample Data

```sql
INSERT INTO Employees (Id, ManagerId, Name) VALUES
(1, NULL, 'Employee 1'),
(2, 1, 'Employee 2'),
(3, 1, 'Employee 3'),
(4, 2, 'Employee 4'),
(5, 2, 'Employee 5'),
(6, 4, 'Employee 6');
```

## Configuration

Update the connection string in [appsettings.json](appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EmployeeDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

Common connection strings:
- **Local SQL Server**: `Server=(local);Database=EmployeeDb;Trusted_Connection=true;TrustServerCertificate=true;`
- **LocalDB**: `Server=(localdb)\\mssqllocaldb;Database=EmployeeDb;Trusted_Connection=true;TrustServerCertificate=true;`
- **Azure SQL Database**: `Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=EmployeeDb;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;`

## Building and Running

### Using .NET CLI

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the API
dotnet run
```

The API will start on `https://localhost:7147` (HTTPS) and `http://localhost:5181` (HTTP) by default.

### Using Visual Studio

1. Open the solution file
2. Press `F5` to start debugging
3. Wait for the application to launch in your browser

## API Endpoints

### Get Employee with Full Hierarchy

```
GET /employee/{id}
```

**Parameters:**
- `id` (path parameter): The employee ID

**Responses:**

**200 OK** - Returns the employee with their full subordinate hierarchy:
```json
{
  "id": 1,
  "managerId": null,
  "name": "Employee 1",
  "subordinates": [
    {
      "id": 2,
      "managerId": 1,
      "name": "Employee 2",
      "subordinates": [
        {
          "id": 4,
          "managerId": 2,
          "name": "Employee 4",
          "subordinates": [
            {
              "id": 6,
              "managerId": 4,
              "name": "Employee 6",
              "subordinates": []
            }
          ]
        },
        {
          "id": 5,
          "managerId": 2,
          "name": "Employee 5",
          "subordinates": []
        }
      ]
    },
    {
      "id": 3,
      "managerId": 1,
      "name": "Employee 3",
      "subordinates": []
    }
  ]
}
```

**404 Not Found** - If the employee does not exist:
```json
{
  "message": "Employee with ID {id} not found."
}
```

**500 Internal Server Error** - If an error occurs during processing:
```json
{
  "message": "An error occurred while retrieving the employee."
}
```

## Example Requests

### Using curl

```bash
# Get employee with ID 1 and all subordinates
curl -X GET "https://localhost:7147/employee/1" -H "accept: application/json"

# Get employee with ID 2 and their subordinates
curl -X GET "https://localhost:7147/employee/2" -H "accept: application/json"
```

### Using PowerShell

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7147/employee/1" `
    -Method Get `
    -ContentType "application/json"
Write-Host ($response | ConvertTo-Json -Depth 100)
```

### Using Python

```python
import requests
import json

response = requests.get("https://localhost:7147/employee/1", verify=False)
if response.status_code == 200:
    print(json.dumps(response.json(), indent=2))
else:
    print(f"Error: {response.status_code} - {response.text}")
```

## API Documentation

Once the application is running, access the interactive Swagger UI at:

```
https://localhost:7147/swagger/ui
```

Or view the OpenAPI specification at:

```
https://localhost:7147/swagger/v1/swagger.json
```

## Performance Considerations

### Recursive CTE Query

The API uses SQL recursive CTEs (Common Table Expressions) for efficient hierarchical queries. This approach:

- **Handles Large Datasets**: Efficiently queries hundreds of thousands of records
- **Minimizes Network Roundtrips**: Fetches all data in a single query
- **Optimized by Database**: SQL Server optimizes the CTE execution plan
- **Reduces Memory Usage**: Compared to loading entire tables and filtering in code

### Query Execution

For a hierarchy with thousands of rows:
- The CTE starts with the specified employee
- Then recursively finds all subordinates
- Returns only the relevant subset for the requested hierarchy

### Database Optimization Tips

1. **Index on ManagerId**:
   ```sql
   CREATE INDEX idx_ManagerId ON Employees(ManagerId);
   ```

2. **Index on Id** (usually created automatically as primary key)

3. **Consider Partitioning** for very large tables (100M+ rows)

## Architecture

### Components

- **Models**: `Employee` class represents the employee domain model
- **Data Layer**: `EmployeeContext` provides Entity Framework Core database access
- **Service Layer**: `EmployeeService` implements the hierarchy retrieval logic
- **Controller**: `EmployeeController` handles HTTP requests and responses

### Flow

1. HTTP GET request to `/employee/{id}`
2. `EmployeeController.GetEmployee()` handles the request
3. `IEmployeeService.GetEmployeeWithSubordinatesAsync()` executes the recursive CTE query
4. Results are mapped to `EmployeeDto` and returned as JSON

## Troubleshooting

### Connection String Issues

- Verify SQL Server is running
- Check the server name and database name
- Ensure Windows Authentication is enabled (if using Trusted_Connection=true)
- Test the connection string in SQL Server Management Studio first

### Missing Dependencies

The project includes:
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `Microsoft.EntityFrameworkCore.Tools` - EF Core tools for migrations
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI support

If you encounter package resolution issues, run:
```bash
dotnet restore --force
```

### CORS Issues

The API includes a CORS policy allowing all origins for development. For production, modify the CORS policy in [Program.cs](Program.cs) to allow only trusted origins.

## Future Enhancements

- Add pagination support for large hierarchies
- Implement caching for frequently accessed employees
- Add filtering by department or location
- Support for bulk employee updates
- Add audit logging for employee changes

## License

This project is provided as-is for educational and development purposes.
