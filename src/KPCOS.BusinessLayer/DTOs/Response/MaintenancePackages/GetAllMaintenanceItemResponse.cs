using System;

namespace KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;

public class GetAllMaintenanceItemResponse
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }
}
