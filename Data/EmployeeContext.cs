using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Data;

public class EmployeeContext : DbContext
{
    public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Employee entity
        modelBuilder.Entity<Employee>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Configure self-referencing relationship
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Subordinates)
            .WithOne()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
