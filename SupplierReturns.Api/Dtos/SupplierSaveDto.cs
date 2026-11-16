namespace SupplierReturns.Api.Dtos;

public class SupplierSaveDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
