using System;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IMaintenanceService
{
    Task CreateMaintenanceRequestAsync(CommandMaintenanceRequest request, Guid customerId);
    
    Task<(IEnumerable<GetAllMaintenanceRequestResponse> data, int total)> GetMaintenanceRequestsAsync(GetAllMaintenanceRequestFilterRequest request);
    
    Task CreateMaintenancePackageItemAsync(CommandMaintenanceItemRequest request);
    
    Task<(IEnumerable<GetAllMaintenanceItemResponse> data, int total)> GetAllMaintenanceItemAsync(GetAllMaintenanceItemFilterRequest request);
    
    Task CreateMaintenancePackageAsync(CommandMaintenancePackageRequest request);
    
    Task<(IEnumerable<GetAllMaintenancePackageResponse> data, int total)> GetAllMaintenancePackageAsync(GetAllMaintenancePackageFilterRequest request);
    
    Task UpdateMaintenanceTaskStatusAsync(Guid id, CommandMaintenanceRequestTaskRequest request);
    
    Task ConfirmMaintenanceTaskAsync(Guid id);
    
    Task<GetAllMaintenanceRequestTaskResponse> GetMaintenanceTaskAsync(Guid id);
    
    Task<(IEnumerable<GetAllMaintenanceRequestTaskResponse> data, int total)> GetAllMaintenanceRequestTasksAsync(GetAllMaintenanceRequestTaskFilterRequest request, Guid? userId = null);
    
    Task<(IEnumerable<GetAllStaffResponse> data, int total)> GetStaffsAsync(GetAllStaffRequest request, Guid maintenanceRequestId);
    
    Task AssignStaffsAsync(Guid maintenanceRequestId, CommandMaintenanceRequestTaskRequest request);

    Task<GetAllMaintenanceRequestResponse> GetDetailMaintenanceRequestAsync(Guid id);
}
