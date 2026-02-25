using EmployeeAPI.Data;
using EmployeeAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework Core
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed the database with sample data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
        // Ensure database and tables are created
        context.Database.EnsureCreated();
        
        var seeder = new DatabaseSeeder(context);
        // Generate 1000 random employees. Adjust the number as needed.
        // For thousands of employees, this may take a minute or two on first run.
        await seeder.SeedAsync(1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Seeding failed: {ex.Message}");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
