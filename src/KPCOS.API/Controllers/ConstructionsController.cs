using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
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
    
    /*[HttpPost("")]
    public async Task<ApiResult> CreateConstructionAsync(ConstructionRequest request)
    {
        await _constructionService.CreateConstructionAsync(request);
        return Ok();
    }*/
    
    /// <summary>
    /// Creates or updates construction items for a project
    /// </summary>
    /// <param name="request">The construction creation request containing project ID and construction items</param>
    /// <returns>Success response if the construction items are created successfully</returns>
    /// <remarks>
    /// This endpoint creates a hierarchical structure of construction items for a project.
    /// - Supports up to 2 levels (parent and child items)
    /// - Maximum 3 parent items can have payment status (isPayment: true)
    /// - Child items always have normal status
    /// - Existing construction items for the project will be removed
    /// - The templateItemId can be null for custom items that not from templates
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "items": [
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Đào móng nhà",
    ///                         "estDate": "2023-12-20",
    ///                         "isPayment": false,
    ///                         "name": "Đào móng",
    ///                         "templateItemId": null
    ///                     },
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Đổ bê tông móng nhà",
    ///                         "estDate": "2023-12-25",
    ///                         "isPayment": false,
    ///                         "name": "Đổ bê tông móng",
    ///                         "templateItemId": null
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng móng nhà",
    ///                 "estDate": "2023-12-15",
    ///                 "isPayment": true,
    ///                 "name": "Móng nhà",
    ///                 "templateItemId": null
    ///             },
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Xây tường gạch nhà",
    ///                         "estDate": "2024-01-15",
    ///                         "isPayment": false,
    ///                         "name": "Xây tường gạch",
    ///                         "templateItemId": null
    ///                     },
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Trát tường nhà",
    ///                         "estDate": "2024-01-20",
    ///                         "isPayment": false,
    ///                         "name": "Trát tường",
    ///                         "templateItemId": null
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng tường nhà",
    ///                 "estDate": "2024-01-10",
    ///                 "isPayment": true,
    ///                 "name": "Tường nhà",
    ///                 "templateItemId": null
    ///             },
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Đổ bê tông mái nhà",
    ///                         "estDate": "2024-02-15",
    ///                         "isPayment": false,
    ///                         "name": "Đổ bê tông mái",
    ///                         "templateItemId": null
    ///                     },
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Lợp ngói mái nhà",
    ///                         "estDate": "2024-02-20",
    ///                         "isPayment": false,
    ///                         "name": "Lợp ngói",
    ///                         "templateItemId": null
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng mái nhà",
    ///                 "estDate": "2024-02-10",
    ///                 "isPayment": true,
    ///                 "name": "Mái nhà",
    ///                 "templateItemId": null
    ///             }
    ///         ],
    ///         "projectId": "44124f32-7de7-4feb-a8cd-51f19cfa83ab"
    ///     }
    /// </remarks>
    /// <response code="200">Construction items created successfully</response>
    /// <response code="400">If more than 3 parent items have payment status</response>
    /// <response code="404">If the project or template item is not found</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [HttpPost("")]
    public async Task<ApiResult> CreateConstructionV2Async(CreateConstructionRequest request)
    {
        await _constructionService.CreateConstructionV2Async(request);
        return Ok();
    }
}