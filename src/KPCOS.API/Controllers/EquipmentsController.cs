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
public class EquipmentsController : BaseController
{
    private readonly IEquipmentService _equipmentService;
    public EquipmentsController (IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }
    
    [HttpPost("")]
    public async Task<ApiResult> CreateEquipmentAsync(EquipmentCreateRequest request)
    {
        await _equipmentService.CreateEquipmentAsync(request);
        return Ok();
    }
    
    [HttpGet("")]
    public async Task<PagedApiResponse<EquipmentResponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        var result = await _equipmentService.GetsAsyncPaging(filter);
        return new PagedApiResponse<EquipmentResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }
    
    [HttpGet("{id}")]
    public async Task<ApiResult<EquipmentResponse>> GetEquipmentByIdAsync(Guid id)
    {
        var result = await _equipmentService.GetEquipmentByIdAsync(id);
        return result;
    }
    
    [HttpPut("{id}")]
    public async Task<ApiResult> UpdateEquipmentAsync(Guid id, EquipmentCreateRequest request)
    {
        await _equipmentService.UpdateEquipmentAsync(id, request);
        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ApiResult> DeleteEquipmentAsync(Guid id)
    {
        await _equipmentService.DeleteEquipmentAsync(id);
        return Ok();
    }
   
}