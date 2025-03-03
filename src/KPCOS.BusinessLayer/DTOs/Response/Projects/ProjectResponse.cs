using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Projects;

/*
 public record ProjectResponse(
    Guid? Id,
    string? Name, 
    string? CustomerName,
    string? Address,
    string? Phone,
    string? Email,
    float? Area,
    float? Depth,
    string? Note,
    string? Status,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    bool? IsActive
    );
*/

public class ProjectResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomerName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Area { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Depth { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreatedAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PackageResponse? Package { get; set; }

    public IEnumerable<StaffResponse?> Staff { get; set; } = new List<StaffResponse?>();
    /*[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QuotationResponse? Type { get; set; }*/
}