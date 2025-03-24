using System;

namespace KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;

public class CommandMaintenancePackageRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Price { get; set; }
    public List<Guid> MaintenanceItems { get; set; }
}
