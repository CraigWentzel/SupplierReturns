namespace SupplierReturns.Api.Models;

public enum ReturnStatus
{
    Draft,
    AwaitingCredit,
    Credited
}

public class SupplierReturn
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ReturnStatus Status { get; set; } = ReturnStatus.Draft;
    public string? CreditNoteNumber { get; set; }
    public List<SupplierReturnLine> Lines { get; set; } = new List<SupplierReturnLine>();
}