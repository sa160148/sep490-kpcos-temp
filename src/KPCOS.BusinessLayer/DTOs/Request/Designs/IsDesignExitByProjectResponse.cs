using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Request.Designs;

public class IsDesignExitByProjectResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsExit3DConfirmed { get; set; }
}