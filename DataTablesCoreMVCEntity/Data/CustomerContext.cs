using DataTablesCoreMVCEntity.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTablesCoreMVCEntity.Data
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options)
        : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customer");
        }

    }
}
