using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Payments;

public class GetAllPaymentBatchesResponse
{
    public Guid Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreatedAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsActive { get; set; }
    public string Name { get; set; }
    public int TotalValue { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsPaid { get; set; }
    public string? Status { get; set; } = default;
}