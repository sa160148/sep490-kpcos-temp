using System;
using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Constructions;

public class GetAllConstructionTaskResponse : ConstructionTaskResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? ImageUrl { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Reason { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public Guid? ConstructionItemId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public DateTime? Deadline { get; set; }
}
