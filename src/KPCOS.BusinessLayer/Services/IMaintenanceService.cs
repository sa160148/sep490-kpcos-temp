using System;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IMaintenanceService
{
    Task CreateMaintenancePackageItemAsync(CommandMaintenanceItemRequest request);
    
    Task<(IEnumerable<GetAllMaintenanceItemResponse> data, int total)> GetAllMaintenanceItemAsync(GetAllMaintenanceItemFilterRequest request);
    
    Task CreateMaintenancePackageAsync(CommandMaintenancePackageRequest request);
    
    Task<(IEnumerable<GetAllMaintenancePackageResponse> data, int total)> GetAllMaintenancePackageAsync(GetAllMaintenancePackageFilterRequest request);
}
