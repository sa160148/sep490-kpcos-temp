using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Maintenances;

public class CommandMaintenanceRequestTaskRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? StaffId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Reason { get; set; }
    public List<Guid>? StaffIds { get; set; }
}
