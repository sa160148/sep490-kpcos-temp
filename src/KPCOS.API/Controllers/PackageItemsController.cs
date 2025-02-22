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
public class PackageItemsController : ControllerBase
{
    private readonly IPackageItemService _packageItemService;

    public PackageItemsController(IPackageItemService packageItemService)
    {
        _packageItemService = packageItemService;
    }
    
    [HttpPost("")]
    public async Task<ApiResult> CreatePackageItemAsync(PackageItemCreateRequest request)
    {
        await _packageItemService.CreatePackageItemAsync(request);
        return Ok();
    }
    
    [HttpGet("")]
    public async Task<PagedApiResponse<PackageItemResponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        
        var result = await _packageItemService.GetsAsyncPaging(filter);
        return new PagedApiResponse<PackageItemResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
        
    }  
    
    [HttpGet("{id}")]
    public async Task<ApiResult<PackageItemResponse>> GetPackageItemByIdAsync(Guid id)
    {
        var packageItem = await _packageItemService.GetPackageItemByIdAsync(id);
        return packageItem;
    }
    
    [HttpPut("{id}")]
    public async Task<ApiResult> UpdatePackageItemAsync(Guid id, PackageItemCreateRequest request)
    {
        await _packageItemService.UpdatePackageItemAsync(id, request);
        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ApiResult> DeletePackageItemAsync(Guid id)
    {
        await _packageItemService.DeletePackageItemAsync(id);
        return Ok();
    }
    
    
}