using System.ComponentModel.DataAnnotations;

namespace SupplierReturns.Api.Dtos;

public class CreateReturnLineDto
{
    [Required, StringLength(50)]
    public string StockCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Quantity { get; set; }

    [Range(0, 100000000)]
    public decimal UnitCost { get; set; }

    [Required, StringLength(200)]
    public string Reason { get; set; } = string.Empty;
}

public class CreateReturnDto
{
    [Range(1, int.MaxValue)]
    public int SupplierId { get; set; }

    [MinLength(1)]
    public List<CreateReturnLineDto> Lines { get; set; } = new List<CreateReturnLineDto>();
}

public class CreditReturnDto
{
    [Required, StringLength(50)]
    public string CreditNoteNumber { get; set; } = string.Empty;
}

public class ReturnLineDto
{
    public int Id { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReturnDto
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CreditNoteNumber { get; set; }
    public DateTime CreatedUtc { get; set; }
    public decimal Total { get; set; }
    public List<ReturnLineDto> Lines { get; set; } = new List<ReturnLineDto>();
}