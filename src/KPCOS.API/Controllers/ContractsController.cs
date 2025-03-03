using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ContractsController : BaseController
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpPost("")]
    public async Task<ApiResult> CreateContractAsync(ContractCreateRequest request)
    {
        await _contractService.CreateContractAsync(request);
        return Ok();
    }

    // [HttpGet("{id}")]
    // public async Task<ApiResult<ContractResponse>> GetContractByIdAsync(Guid id)
    // {
    //     var result = await _contractService.GetContractByIdAsync(id);
    //     return result;
    // }
    
}