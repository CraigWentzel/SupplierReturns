using Microsoft.EntityFrameworkCore;
using SupplierReturns.Api.Dtos;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Api.Data;

public class ReturnData
{
    public const string NotFound = "NotFound";
    private readonly AppDbContext _db;

    public ReturnData(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReturnDto>> GetAllAsync(ReturnStatus? status, int? supplierId)
    {
        IQueryable<SupplierReturn> query = _db.SupplierReturns
            .Include(r => r.Supplier)
            .Include(r => r.Lines);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }
        if (supplierId.HasValue)
        {
            query = query.Where(r => r.SupplierId == supplierId.Value);
        }

        List<SupplierReturn> returns = await query.OrderByDescending(r => r.Id).ToListAsync();
        return returns.Select(ToDto).ToList();
    }

    public async Task<ReturnDto?> GetByIdAsync(int id)
    {
        SupplierReturn? r = await LoadAsync(id);
        return r == null ? null : ToDto(r);
    }

    public async Task<ReturnDto?> CreateAsync(CreateReturnDto dto)
    {
        bool supplierExists = await _db.Suppliers.AnyAsync(s => s.Id == dto.SupplierId);
        if (!supplierExists)
        {
            return null;
        }

        SupplierReturn entity = new SupplierReturn
        {
            SupplierId = dto.SupplierId,
            ReturnNumber = "PENDING"
        };
        foreach (CreateReturnLineDto l in dto.Lines)
        {
            entity.Lines.Add(new SupplierReturnLine
            {
                StockCode = l.StockCode,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                Reason = l.Reason
            });
        }

        _db.SupplierReturns.Add(entity);
        await _db.SaveChangesAsync();

        entity.ReturnNumber = "RET-" + entity.Id.ToString("D5");
        await _db.SaveChangesAsync();

        SupplierReturn loaded = (await LoadAsync(entity.Id))!;
        return ToDto(loaded);
    }

    public async Task<(ReturnDto? Dto, string? Error)> SubmitAsync(int id)
    {
        SupplierReturn? r = await LoadAsync(id);
        if (r == null) return (null, NotFound);
        if (r.Status != ReturnStatus.Draft) return (null, "Only a Draft return can be submitted.");

        r.Status = ReturnStatus.AwaitingCredit;
        await _db.SaveChangesAsync();
        return (ToDto(r), null);
    }

    public async Task<(ReturnDto? Dto, string? Error)> CreditAsync(int id, string creditNoteNumber)
    {
        SupplierReturn? r = await LoadAsync(id);
        if (r == null) return (null, NotFound);
        if (r.Status != ReturnStatus.AwaitingCredit) return (null, "Only a return awaiting credit can be credited.");

        r.Status = ReturnStatus.Credited;
        r.CreditNoteNumber = creditNoteNumber;
        await _db.SaveChangesAsync();
        return (ToDto(r), null);
    }

    public async Task<string?> DeleteAsync(int id)
    {
        SupplierReturn? r = await LoadAsync(id);
        if (r == null) return NotFound;
        if (r.Status != ReturnStatus.Draft) return "Only a Draft return can be deleted.";

        _db.SupplierReturns.Remove(r);
        await _db.SaveChangesAsync();
        return null;
    }

    private async Task<SupplierReturn?> LoadAsync(int id)
    {
        return await _db.SupplierReturns
            .Include(r => r.Supplier)
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    private static ReturnDto ToDto(SupplierReturn r)
    {
        List<ReturnLineDto> lines = r.Lines.Select(l => new ReturnLineDto
        {
            Id = l.Id,
            StockCode = l.StockCode,
            Description = l.Description,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost,
            LineTotal = l.Quantity * l.UnitCost,
            Reason = l.Reason
        }).ToList();

        return new ReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            SupplierId = r.SupplierId,
            SupplierName = r.Supplier?.Name ?? string.Empty,
            Status = r.Status.ToString(),
            CreditNoteNumber = r.CreditNoteNumber,
            CreatedUtc = r.CreatedUtc,
            Total = lines.Sum(l => l.LineTotal),
            Lines = lines
        };
    }
}