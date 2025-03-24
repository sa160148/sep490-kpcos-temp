using System;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;

namespace KPCOS.BusinessLayer.DTOs.Response.Maintenances;

public class GetAllMaintenanceRequestResponse
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

    public GetAllMaintenancePackageResponse MaintenancePackage { get; set; }
}
