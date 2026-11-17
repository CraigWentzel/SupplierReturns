namespace SupplierReturns.Api.Models;

public class SupplierReturnLine
{
    public int Id { get; set; }
    public int SupplierReturnId { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Reason { get; set; } = string.Empty;
}