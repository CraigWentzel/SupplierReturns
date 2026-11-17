using Microsoft.EntityFrameworkCore;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierReturn> SupplierReturns => Set<SupplierReturn>();
    public DbSet<SupplierReturnLine> SupplierReturnLines => Set<SupplierReturnLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSupplierReturn(modelBuilder);
        ConfigureSupplierReturnLine(modelBuilder);
    }

    private static void ConfigureSupplierReturn(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupplierReturn>(entity =>
        {
            entity.Property(r => r.ReturnNumber).HasMaxLength(20).IsRequired();
            entity.Property(r => r.CreditNoteNumber).HasMaxLength(50);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasMany(r => r.Lines)
                .WithOne()
                .HasForeignKey(l => l.SupplierReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Supplier)
                .WithMany()
                .HasForeignKey(r => r.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSupplierReturnLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupplierReturnLine>(entity =>
        {
            entity.Property(l => l.StockCode).HasMaxLength(50).IsRequired();
            entity.Property(l => l.Description).HasMaxLength(200);
            entity.Property(l => l.Reason).HasMaxLength(200).IsRequired();
            entity.Property(l => l.Quantity).HasPrecision(18, 2);
            entity.Property(l => l.UnitCost).HasPrecision(18, 2);
        });
    }
}
