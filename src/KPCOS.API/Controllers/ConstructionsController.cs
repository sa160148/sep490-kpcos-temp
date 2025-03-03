using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ConstructionsController  : BaseController
{
    private readonly IConstructionServices _constructionService;

    public ConstructionsController(IConstructionServices constructionService)
    {
        _constructionService = constructionService;
    }

    
    [HttpPost("")]
    public async Task<ApiResult> CreateConstructionAsync(ConstructionRequest request)
    {
        await _constructionService.CreateConstructionAsync(request);
        return Ok();
    }
    
}