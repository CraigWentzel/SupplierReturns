using Microsoft.EntityFrameworkCore;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
}
