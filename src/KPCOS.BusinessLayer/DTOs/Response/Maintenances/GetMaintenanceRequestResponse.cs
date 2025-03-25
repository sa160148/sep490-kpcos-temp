using System;
using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.Maintenances;

public class GetMaintenanceRequestResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Area { get; set; }
    public double? Depth { get; set; }
    public string? Address { get; set; }
    public int? TotalValue { get; set; }
    public string? Type { get; set; }
    public bool? IsPaid { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GetAllMaintenancePackageResponse? MaintenancePackage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GetAllStaffResponse? Customer { get; set; }
}
