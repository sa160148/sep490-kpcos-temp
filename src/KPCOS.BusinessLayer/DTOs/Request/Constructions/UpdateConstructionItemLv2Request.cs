using System;
using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class UpdateConstructionItemLv2Request
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<UpdateConstructionTaskRequest>? ConstructionTasks { get; set; }
}
