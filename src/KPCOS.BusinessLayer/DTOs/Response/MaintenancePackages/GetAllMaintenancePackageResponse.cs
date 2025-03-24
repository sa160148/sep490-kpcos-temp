using System;

namespace KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;

public class GetAllMaintenancePackageResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    public IEnumerable<int>? PriceList { get; set; } = new List<int>();
    
    public IEnumerable<GetAllMaintenanceItemResponse>? MaintenanceItems { get; set; } = new List<GetAllMaintenanceItemResponse>();
    
    public string? Status { get; set; }
    
    public bool? IsActive { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
