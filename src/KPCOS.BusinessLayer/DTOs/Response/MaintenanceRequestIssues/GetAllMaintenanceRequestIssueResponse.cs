using System;
using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.MaintenanceRequestIssues;

public class GetAllMaintenanceRequestIssueResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Solution { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public DateOnly? ActualAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsActive { get; set; }
    
    // Navigation properties for related entities
    /*[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]*/
    public GetAllStaffResponse? Staff { get; set; }
    
    /*[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]*/
    public GetMaintenanceRequestResponse? MaintenanceRequest { get; set; }
}
