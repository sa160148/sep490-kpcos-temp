using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

    /// <summary>
    /// Gets a paginated list of construction items with their children
    /// </summary>
    /// <param name="filter">Filter criteria for construction items including:
    /// - Search: Filters by name or description containing the search term
    /// - IsActive: Filters by active status (true/false)
    /// - Status: Filters by construction item status (OPENING, PROCESSING, DONE)
    /// - IsPayment: Filters by payment status (true/false)
    /// - IsChild: If true, returns only child items; if false, returns only parent items
    /// - PageNumber: Page number for pagination (1-based)
    /// - PageSize: Number of items per page
    /// - SortColumn: Column to sort by (default: CreatedAt)
    /// - SortDir: Sort direction (Asc or Desc, default: Desc)
    /// </param>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/constructions/items?Search=foundation&amp;IsActive=true&amp;Status=OPENING&amp;IsPayment=true&amp;PageNumber=1&amp;PageSize=10
    /// 
    /// Available status values:
    /// - OPENING: Initial status for new construction items
    /// - PROCESSING: Construction items that are currently in progress
    /// - DONE: Completed construction items
    /// 
    /// IsChild filter behavior:
    /// - When IsChild=true: Returns only child items (items with a parent)
    /// - When IsChild=false: Returns only parent items (items without a parent) with their children
    /// - When IsChild is not specified: Returns parent items with their children (default behavior)
    /// </remarks>
    /// <returns>A paginated list of construction items with their children</returns>
    /// <response code="200">Returns the paginated list of construction items</response>
    [HttpGet("items")]
    [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionItemResponse>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Gets a paginated list of construction items with their children",
        Description = "Retrieves construction items based on the provided filter criteria. Parent items are returned with their child items populated in the Childs property.",
        OperationId = "GetAllConstructionItems",
        Tags = new[] { "Constructions" }
    )]
    public async Task<PagedApiResponse<GetAllConstructionItemResponse>> GetAllConstructionItemsAsync(
        [FromQuery] 
        [SwaggerParameter(
            Description = "Filter criteria for construction items including Search, IsActive, Status (OPENING, PROCESSING, DONE), IsPayment, IsChild, PageNumber, PageSize, SortColumn, and SortDir",
            Required = false
        )]
        GetAllConstructionItemFilterRequest filter)
    {
        var (data, total) = await _constructionService.GetAllConstructionItemsAsync(filter);
        return new PagedApiResponse<GetAllConstructionItemResponse>(data, filter.PageNumber, filter.PageSize, total);
    }

    /// <summary>
    /// Gets a paginated list of construction tasks
    /// </summary>
    /// <param name="filter">Filter criteria for construction tasks including:
    /// - Search: Filters by name containing the search term
    /// - IsActive: Filters by active status (true/false)
    /// - Status: Filters by task status (OPENING, PROCESSING, DONE)
    /// - IsOverdue: Filters by overdue status (true/false)
    /// - ConstructionItemId: Filters by construction item ID
    /// - PageNumber: Page number for pagination (1-based)
    /// - PageSize: Number of items per page
    /// - SortColumn: Column to sort by (default: CreatedAt)
    /// - SortDir: Sort direction (Asc or Desc, default: Desc)
    /// </param>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/constructions/task?Search=foundation&amp;IsActive=true&amp;Status=OPENING&amp;IsOverdue=true&amp;ConstructionItemId=6e843e92-e55f-414c-a2e7-d52afe8251ce&amp;PageNumber=1&amp;PageSize=10
    /// 
    /// Available status values:
    /// - OPENING: Initial status for new tasks
    /// - PROCESSING: Tasks that are currently in progress
    /// - DONE: Completed tasks
    /// 
    /// IsOverdue filter behavior:
    /// - When IsOverdue=true: Returns only tasks with deadlines in the past that are not marked as DONE
    /// - When IsOverdue=false: Returns only tasks that are not overdue or are marked as DONE
    /// - When IsOverdue is not specified: Returns all tasks (default behavior)
    /// </remarks>
    /// <returns>A paginated list of construction tasks</returns>
    /// <response code="200">Returns the paginated list of construction tasks</response>
    [HttpGet("task")]
    [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionTaskResponse>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Gets a paginated list of construction tasks",
        Description = "Retrieves construction tasks based on the provided filter criteria including search term, active status, task status, overdue status, and construction item ID.",
        OperationId = "GetAllConstructionTasks",
        Tags = new[] { "Constructions" }
    )]
    public async Task<PagedApiResponse<GetAllConstructionTaskResponse>> GetAllConstructionTaskAsync(
        [FromQuery] 
        [SwaggerParameter(
            Description = "Filter criteria for construction tasks including Search, IsActive, Status, IsOverdue, ConstructionItemId, PageNumber, PageSize, SortColumn, and SortDir",
            Required = false
        )]
        GetAllConstructionTaskFilterRequest filter)
    {
        var (data, total) = await _constructionService.GetAllConstructionTaskAsync(filter);
        return new PagedApiResponse<GetAllConstructionTaskResponse>(data, filter.PageNumber, filter.PageSize, total);
    }

    [HttpGet("task/{id}")]
    [ProducesResponseType(typeof(ApiResult<GetConstructionTaskDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Gets detailed information about a specific construction task",
        Description = "Retrieves detailed information about a construction task by its ID, including task properties and associated staff information.",
        OperationId = "GetConstructionTaskDetailById",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult<GetConstructionTaskDetailResponse>> GetConstructionTaskDetailByIdAsync(
        [SwaggerParameter(
            Description = "The unique identifier of the construction task",
            Required = true
        )]
        Guid id)
    {
        var task = await _constructionService.GetConstructionTaskDetailByIdAsync(id);
        return Ok(task);
    }

    [HttpGet("item/{id}")]
    [ProducesResponseType(typeof(ApiResult<GetConstructionItemDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Gets detailed information about a specific construction item",
        Description = "Retrieves detailed information about a construction item by its ID, including its properties, tasks, and hierarchical relationships. For parent items, includes child items. For child items, includes parent information.",
        OperationId = "GetConstructionItemDetailById",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult<GetConstructionItemDetailResponse>> GetConstructionItemDetailByIdAsync(
        [SwaggerParameter(
            Description = "The unique identifier of the construction item",
            Required = true
        )]
        Guid id)
    {
        var result = await _constructionService.GetConstructionItemDetailByIdAsync(id);
        return Ok(result);
    }
}