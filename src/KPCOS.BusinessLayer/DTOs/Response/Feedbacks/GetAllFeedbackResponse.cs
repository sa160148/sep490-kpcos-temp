using System;
using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.Feedbacks;

public class GetAllFeedbackResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? Rating { get; set; }
    public string? Type { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsActive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GetAllStaffResponse? Customer { get; set; }
}
