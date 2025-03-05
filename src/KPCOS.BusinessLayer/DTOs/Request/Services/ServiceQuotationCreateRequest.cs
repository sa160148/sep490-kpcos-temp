namespace KPCOS.BusinessLayer.DTOs.Request.Services;

public class ServiceQuotationCreateRequest
{
    public Guid Id { get; set; }
    public string Note { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; }
}