using Microsoft.EntityFrameworkCore;
using SupplierReturns.Api.Dtos;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Api.Data;

public class SupplierData
{
    private readonly AppDbContext db;

    public SupplierData(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        List<Supplier> suppliers = await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        return suppliers.Select(ToDto).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        Supplier? supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return supplier == null ? null : ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(SupplierSaveDto input)
    {
        Supplier supplier = new Supplier
        {
            Name = input.Name,
            ContactEmail = input.ContactEmail,
            Phone = input.Phone
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        return ToDto(supplier);
    }

    public async Task<bool> UpdateAsync(int id, SupplierSaveDto input)
    {
        Supplier? supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null)
        {
            return false;
        }

        supplier.Name = input.Name;
        supplier.ContactEmail = input.ContactEmail;
        supplier.Phone = input.Phone;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Supplier? supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null)
        {
            return false;
        }

        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync();

        return true;
    }

    private static SupplierDto ToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactEmail = supplier.ContactEmail,
            Phone = supplier.Phone
        };
    }
}
