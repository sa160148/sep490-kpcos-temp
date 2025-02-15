using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;

    public PackagesController(IPackageService packageService)
    {
        _packageService = packageService;
    }
    
    [HttpPost("")]
    public async Task<ApiResult> CreatePackageAsync(PackageCreateRequest request)
    {
        await _packageService.CreatePackageAsync(request);
        return new ApiResult(true, ApiResultStatusCode.Success);
    }

    
}