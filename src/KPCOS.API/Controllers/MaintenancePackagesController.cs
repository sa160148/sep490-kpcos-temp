using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/maintenance-packages")]
    public class MaintenancePackagesController : BaseController
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenancePackagesController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpPost("item")]
        public async Task<ApiResult> CreateMaintenancePackageItemAsync(
            [FromBody] 
            CommandMaintenanceItemRequest request)
        {
            await _maintenanceService.CreateMaintenancePackageItemAsync(request);
            return Ok();
        }

        [HttpGet("item")]
        public async Task<PagedApiResponse<GetAllMaintenanceItemResponse>> GetAllMaintenanceItemAsync(
            [FromQuery] 
            GetAllMaintenanceItemFilterRequest request)
        {
            var maintenancePackageItems = await _maintenanceService.GetAllMaintenanceItemAsync(request);
            return new PagedApiResponse<GetAllMaintenanceItemResponse>(maintenancePackageItems.data, 
            request.PageNumber, 
            request.PageSize, 
            maintenancePackageItems.total);
        }

        [HttpPost]
        public async Task<ApiResult> CreateMaintenancePackageAsync(
            [FromBody] 
            CommandMaintenancePackageRequest request)
        {
            await _maintenanceService.CreateMaintenancePackageAsync(request);
            return Ok();
        }

        [HttpGet]
        public async Task<PagedApiResponse<GetAllMaintenancePackageResponse>> GetAllMaintenancePackageAsync(
            [FromQuery] 
            GetAllMaintenancePackageFilterRequest request)
        {
            var maintenancePackages = await _maintenanceService.GetAllMaintenancePackageAsync(request);
            return new PagedApiResponse<GetAllMaintenancePackageResponse>(maintenancePackages.data, 
            request.PageNumber, 
            request.PageSize, 
            maintenancePackages.total);
        }
    }
}
