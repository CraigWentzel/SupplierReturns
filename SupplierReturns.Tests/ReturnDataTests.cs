using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SupplierReturns.Api.Data;
using SupplierReturns.Api.Dtos;
using SupplierReturns.Api.Models;

namespace SupplierReturns.Tests;

public class ReturnDataTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly ReturnData _data;

    public ReturnDataTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _db.Suppliers.Add(new Supplier { Name = "Acme Supplies" });
        _db.SaveChanges();

        _data = new ReturnData(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static CreateReturnDto NewReturn(int supplierId)
    {
        return new CreateReturnDto
        {
            SupplierId = supplierId,
            Lines = new List<CreateReturnLineDto>
            {
                new CreateReturnLineDto
                {
                    StockCode = "A100",
                    Description = "Widget",
                    Quantity = 5,
                    UnitCost = 12.50m,
                    Reason = "Damaged on delivery"
                }
            }
        };
    }

    [Fact]
    public async Task CreateAsync_ValidReturn_IsDraftWithNumberAndTotal()
    {
        ReturnDto? result = await _data.CreateAsync(NewReturn(1));

        Assert.NotNull(result);
        Assert.Equal("RET-00001", result!.ReturnNumber);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(62.5m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_UnknownSupplier_ReturnsNull()
    {
        ReturnDto? result = await _data.CreateAsync(NewReturn(999));

        Assert.Null(result);
    }

    [Fact]
    public async Task CreditAsync_BeforeSubmit_ReturnsError()
    {
        ReturnDto? created = await _data.CreateAsync(NewReturn(1));

        (ReturnDto? Dto, string? Error) result = await _data.CreditAsync(created!.Id, "CN-1001");

        Assert.Null(result.Dto);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task SubmitThenCredit_MovesToCredited()
    {
        ReturnDto? created = await _data.CreateAsync(NewReturn(1));

        await _data.SubmitAsync(created!.Id);
        (ReturnDto? Dto, string? Error) result = await _data.CreditAsync(created.Id, "CN-1001");

        Assert.Null(result.Error);
        Assert.Equal("Credited", result.Dto!.Status);
        Assert.Equal("CN-1001", result.Dto.CreditNoteNumber);
    }
}