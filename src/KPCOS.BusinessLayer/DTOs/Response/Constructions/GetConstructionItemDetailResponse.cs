using System;
using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Constructions;

public class GetConstructionItemDetailResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Category { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsPayment { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsActive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateOnly? EstimateAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateOnly? ActualAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreatedAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ProjectId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<GetAllConstructionItemChildResponse>? Childs { get; set; } = new List<GetAllConstructionItemChildResponse>();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<GetAllConstructionTaskResponse>? ConstructionTasks { get; set; } = new List<GetAllConstructionTaskResponse>();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GetConstructionItemParentDetailResponse? Parent { get; set; }
}

public class GetConstructionItemParentDetailResponse : GetConstructionItemDetailResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GetAllConstructionItemChildResponse>? Childs { get; set; } = null;
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public GetConstructionItemParentDetailResponse? Parent { get; set; } = null;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GetAllConstructionTaskResponse>? ConstructionTasks { get; set; } = null;
}
