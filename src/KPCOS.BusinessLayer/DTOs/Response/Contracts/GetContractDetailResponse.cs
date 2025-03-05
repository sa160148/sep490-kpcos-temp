using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.Payments;

namespace KPCOS.BusinessLayer.DTOs.Response.Contracts;

public class GetContractDetailResponse
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsActive { get; set; } = default;
    public string Name { get; set; }
    public string CustomerName { get; set; }
    public int ContractValue { get; set; }
    public string Url { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
    public Guid QuotationId { get; set; }
    public Guid ProjectId { get; set; }
    public string? Status { get; set; } = default;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GetAllPaymentBatchesResponse>? PaymentBatches { get; set; } = new List<GetAllPaymentBatchesResponse>();
}