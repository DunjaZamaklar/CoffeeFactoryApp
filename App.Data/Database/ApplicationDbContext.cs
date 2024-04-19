using App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Database
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<SupplyOrderStatus> SupplyOrderStatus { get; set; }

        public DbSet<EmployeePosition> EmployeePositions { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<SupplyCategory> SupplyCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options) { }
    }
}
