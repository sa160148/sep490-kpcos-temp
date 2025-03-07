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
    /// - All items are created with status OPENING
    /// - Only parent (level 1) items can have isPayment=true
    /// - Child (level 2) items always have isPayment=false
    /// - Existing construction items for the project will be removed
    /// - Items can be based on templates or custom-defined
    /// - When templateItemId is provided, name and description are taken from the template
    /// - When templateItemId is null, custom name and description are used
    /// 
    /// Sample request:
    /// 
    ///     {
    ///         "items": [
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Lợp ngói mái nhà",
    ///                         "estimateAt": "2024-02-20",
    ///                         "isPayment": false,
    ///                         "name": "Lợp ngói",
    ///                         "templateItemId": "5cd50ec1-a85c-4be5-9dd6-2d7440b83394"
    ///                     },
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Lợp ngói mái nhà",
    ///                         "estimateAt": "2024-02-20",
    ///                         "isPayment": false,
    ///                         "name": "Lợp ngói",
    ///                         "templateItemId": null
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng mái nhà",
    ///                 "estimateAt": "2024-02-10",
    ///                 "isPayment": false,
    ///                 "name": null,
    ///                 "templateItemId": "7b2c1c48-776f-49a0-86c5-25e9ec628f17"
    ///             },
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Lợp ngói mái nhà",
    ///                         "estimateAt": "2024-02-20",
    ///                         "isPayment": false,
    ///                         "name": "Lợp ngói",
    ///                         "templateItemId": "d2a7cdeb-857c-45ed-bb45-ad185c356f93"
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng mái nhà",
    ///                 "estimateAt": "2024-02-10",
    ///                 "isPayment": true,
    ///                 "name": null,
    ///                 "templateItemId": "6d664fdd-20b5-473a-bb3c-b8395884452b"
    ///             },
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Trát tường nhà",
    ///                         "estimateAt": "2024-01-20",
    ///                         "isPayment": false,
    ///                         "name": "Trát tường",
    ///                         "templateItemId": "7e7a3d26-2c0b-4ad2-a95b-bf62838d5e32"
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng tường nhà",
    ///                 "estimateAt": "2024-01-10",
    ///                 "isPayment": true,
    ///                 "name": null,
    ///                 "templateItemId": "6d664fdd-20b5-473a-bb3c-b8395884452b"
    ///             },
    ///             {
    ///                 "childs": [
    ///                     {
    ///                         "childs": null,
    ///                         "description": "Lợp ngói mái nhà",
    ///                         "estimateAt": "2024-02-20",
    ///                         "isPayment": false,
    ///                         "name": "Lợp ngói",
    ///                         "templateItemId": "d2a7cdeb-857c-45ed-bb45-ad185c356f93"
    ///                     }
    ///                 ],
    ///                 "description": "Xây dựng mái nhà",
    ///                 "estimateAt": "2024-02-10",
    ///                 "isPayment": true,
    ///                 "name": null,
    ///                 "templateItemId": "6d664fdd-20b5-473a-bb3c-b8395884452b"
    ///             }
    ///         ],
    ///         "projectId": "6e843e92-e55f-414c-a2e7-d52afe8251ce"
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