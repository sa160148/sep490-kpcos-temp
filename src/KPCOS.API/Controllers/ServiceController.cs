using KPCOS.BusinessLayer;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ServiceController
{
    private readonly IServiceService _serviceService;
    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }
    [HttpPost("")]
    public async Task<ApiResult> CreateServiceAsync(ServiceCreateRequest request)
    {
        await _serviceService.CreateService(request);
        return new ApiResult(true, ApiResultStatusCode.Success);
    }
}