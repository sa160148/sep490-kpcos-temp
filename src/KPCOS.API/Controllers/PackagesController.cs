using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Pagination;
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
    
    [HttpGet("")]
    public async Task<PagedApiResponse<PackageResponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        
        var result = await _packageService.GetsAsyncPaging(filter);
        return new PagedApiResponse<PackageResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }
    
    [HttpGet("{id}")]
    public async Task<ApiResult<PackageResponse>> GetPackageByIdAsync(Guid id)
    {
        var result = await _packageService.GetPackageByIdAsync(id);
        return result;
    }
    [HttpPut("{id}")]
    public async Task<ApiResult> UpdatePackageAsync(Guid id, PackageCreateRequest request)
    {
        await _packageService.UpdatePackageAsync(id, request);
        return Ok();
    }
    [HttpDelete("{id}")]
    public async Task<ApiResult> DeletePackageAsync(Guid id)
    {
        await _packageService.DeletePackageAsync(id);
        return new ApiResult(true, ApiResultStatusCode.Success);
    }
}