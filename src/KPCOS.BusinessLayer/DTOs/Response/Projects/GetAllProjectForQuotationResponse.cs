using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Projects;

public class GetAllProjectForQuotationResponse : ProjectForListResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? StandOut { get; set; }
}