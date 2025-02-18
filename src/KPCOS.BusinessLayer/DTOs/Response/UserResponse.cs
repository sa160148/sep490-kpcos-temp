using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response;

public class UserResponse
{
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Password { get; set; }
}