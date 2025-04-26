using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Maintenances;

public class CommandMaintenanceRequest
{
    public string? Name { get; set; }
    public double? Area { get; set; }
    public double? Depth { get; set; }
    public string? Address { get; set; }
    public decimal? TotalValue { get; set; }
    public int? MinPrice { get; set; }
    public string? Type { get; set; }
    public bool? IsPaid { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public int? Duration { get; set; }
    public Guid? MaintenancePackageId { get; set; }
    public List<Guid>? StaffIds { get; set; }
}
