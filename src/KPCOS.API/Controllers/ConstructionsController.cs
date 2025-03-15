using System.Security.Claims;
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
    /// - Status: Filters by task status (OPENING, PROCESSING, PREVIEWING, DONE)
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
    /// - PREVIEWING: Tasks that have been submitted for review
    /// - DONE: Completed tasks
    /// 
    /// IsOverdue filter behavior:
    /// - When IsOverdue=true: Returns only tasks with deadlines in the past that are not marked as DONE
    /// - When IsOverdue=false: Returns only tasks that are not overdue or are marked as DONE
    /// - When IsOverdue is not specified: Returns all tasks (default behavior)
    /// - All deadline comparisons use Southeast Asia time zone
    /// 
    /// User-specific filtering:
    /// - For authenticated users with the "constructor" position, only tasks assigned to them will be returned
    /// - For other users or unauthenticated requests, all tasks matching the filter criteria will be returned
    /// </remarks>
    /// <returns>A paginated list of construction tasks</returns>
    /// <response code="200">Returns the paginated list of construction tasks</response>
    [HttpGet("task")]
    [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionTaskResponse>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Gets a paginated list of construction tasks",
        Description = "Retrieves construction tasks based on the provided filter criteria including search term, active status, task status, overdue status, and construction item ID. For authenticated users with the 'constructor' position, only tasks assigned to them will be returned. Deadline comparisons use Southeast Asia time zone.",
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        (IEnumerable<GetAllConstructionTaskResponse> data, int total) result;
        if (userIdClaim != null)
        {
            var userId = Guid.Parse(userIdClaim);
            result = await _constructionService.GetAllConstructionTaskAsync(filter, userId);
            return new PagedApiResponse<GetAllConstructionTaskResponse>(result.data, filter.PageNumber, filter.PageSize, result.total);
        }
        result = await _constructionService.GetAllConstructionTaskAsync(filter);
        return new PagedApiResponse<GetAllConstructionTaskResponse>(result.data, filter.PageNumber, filter.PageSize, result.total);
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
    
    [HttpPost("task/{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Tạo các công việc mới cho hạng mục xây dựng(construction item) cấp 2 (con)",
        Description = 
            "API này cho phép tạo nhiều công việc cho một hạng mục xây dựng(construction item) cấp 2 (con) cụ thể. " +
            "Các quy tắc và hành vi:\n\n" +
            "- Chỉ có thể tạo công việc cho hạng mục xây dựng(construction item) cấp 2 (con), không thể tạo cho hạng mục xây dựng(construction item) cấp 1 (cha)\n" +
            "- Tất cả công việc được tạo với trạng thái OPENING\n" +
            "- Tên công việc phải là duy nhất trong hạng mục xây dựng\n" +
            "- Khi tạo công việc, trạng thái của hạng mục xây dựng(construction item) cấp 2 sẽ được chuyển từ OPENING sang PROCESSING\n" +
            "- Nếu hạng mục xây dựng(construction item) cấp 1 (cha) có trạng thái OPENING, nó cũng sẽ được chuyển sang PROCESSING\n" +
            "- Thời hạn (deadline) được tự động chuyển đổi sang múi giờ Việt Nam và định dạng phù hợp với PostgreSQL\n\n" +
            "Lỗi có thể xảy ra:\n" +
            "- 400 Bad Request: ID hạng mục không hợp lệ, hạng mục không phải cấp 2, tên công việc trống, " +
            "tên công việc trùng lặp trong yêu cầu, hoặc tên công việc đã tồn tại trong hạng mục\n" +
            "- 404 Not Found: Không tìm thấy hạng mục xây dựng(construction item) với ID được cung cấp",
        OperationId = "CreateConstructionTask",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> CreateConstructionTaskAsync(
        [SwaggerParameter(
            Description = 
                "Danh sách các công việc cần tạo. Mỗi công việc yêu cầu có tên (bắt buộc) và có thể bao gồm thời hạn (tùy chọn).\n\n" +
                "Ví dụ:\n" +
                "```json\n" +
                "[\n" +
                "  {\n" +
                "    \"name\": \"Công việc 1\",\n" +
                "    \"deadlineAt\": \"2023-12-31T17:00:00\"\n" +
                "  },\n" +
                "  {\n" +
                "    \"name\": \"Công việc 2\"\n" +
                "  }\n" +
                "]\n" +
                "```",
            Required = true
        )]
        List<CreateConstructionTaskRequest> request,
        [SwaggerParameter(
            Description = "ID của hạng mục xây dựng(construction item) cấp 2 (con)",
            Required = true
        )]
        Guid id
        )
    {
        await _constructionService.CreateConstructionTaskAsync(request, id);
        return Ok();
    }

    /// <summary>
    /// Cập nhật hạng mục xây dựng cấp 1 (cha) và thêm hạng mục con mới
    /// </summary>
    /// <remarks>
    /// API này cho phép cập nhật thông tin của hạng mục xây dựng cấp 1 (cha) và thêm các hạng mục con mới.
    /// "Đối với hạng mục cấp 1, chỉ cập nhật name và description.
    /// "Đối với hạng mục con, sử dụng name, description và estimateAt.
    /// 
    /// Ví dụ yêu cầu:
    /// 
    ///     {
    ///       "name": "Xây dựng mái che",
    ///       "description": "Mô tả cập nhật cho hạng mục mái che",
    ///       "childs": [
    ///         {
    ///           "name": "Lợp tôn mái che phía Bắc",
    ///           "description": "Lợp tôn cho phần mái phía Bắc của công trình",
    ///           "estimateAt": "2024-07-15"
    ///         },
    ///         {
    ///           "name": "Lợp tôn mái che phía Nam",
    ///           "description": "Lợp tôn cho phần mái phía Nam của công trình",
    ///           "estimateAt": "2024-07-30"
    ///         }
    ///       ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">ID của hạng mục xây dựng cấp 1 (cha) cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật cho hạng mục xây dựng cấp 1 (cha) và danh sách hạng mục con mới (nếu có)</param>
    /// <returns>Kết quả cập nhật hạng mục xây dựng</returns>
    /// <response code="200">Cập nhật hạng mục xây dựng thành công</response>
    /// <response code="400">ID hạng mục không hợp lệ, hạng mục không phải cấp 1 (cha), tên hạng mục cha đã tồn tại, tên hạng mục con trống, tên hạng mục con trùng lặp trong yêu cầu, hoặc tên hạng mục con đã tồn tại</response>
    /// <response code="404">Không tìm thấy hạng mục xây dựng với ID được cung cấp</response>
    [HttpPut("item/{id}/lv1")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Cập nhật hạng mục xây dựng cấp 1 (cha) và thêm hạng mục con mới",
        Description = 
            "API này cho phép cập nhật thông tin của hạng mục xây dựng cấp 1 (cha) và thêm các hạng mục con mới. " +
            "Đối với hạng mục cấp 1, chỉ cập nhật name và description. " +
            "Đối với hạng mục con, sử dụng name, description và estimateAt.",
        OperationId = "UpdateConstructionItemLv1",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> UpdateConstructionItemLv1Async(
        [SwaggerParameter(
            Description = "ID của hạng mục xây dựng cấp 1 (cha) cần cập nhật",
            Required = true
        )]
        Guid id,
        [FromBody]
        [SwaggerParameter(
            Description = "Thông tin cập nhật cho hạng mục xây dựng cấp 1 (cha) và danh sách hạng mục con mới (nếu có)",
            Required = true
        )]
        CreateConstructionItemRequest request
    )
    {
        await _constructionService.UpdateConstructionItemLv1Async(request, id);
        return Ok();
    }

    /// <summary>
    /// Cập nhật hạng mục xây dựng cấp 2 (con) và thêm công việc mới
    /// </summary>
    /// <remarks>
    /// API này cho phép cập nhật thông tin của hạng mục xây dựng cấp 2 (con) và thêm các công việc mới.
    /// 
    /// **Quy tắc và hành vi:**
    /// - Đối với hạng mục cấp 2, chỉ cập nhật name và description
    /// - Đối với công việc mới, sử dụng name và deadlineAt
    /// - Tất cả công việc mới được tạo với trạng thái OPENING
    /// - Thời hạn (deadline) được tự động chuyển đổi sang múi giờ Việt Nam và định dạng phù hợp với PostgreSQL
    /// - Nếu hạng mục xây dựng có trạng thái OPENING hoặc DONE, nó sẽ được chuyển sang PROCESSING khi thêm công việc mới
    /// - Tương tự, nếu hạng mục cha có trạng thái OPENING hoặc DONE, nó cũng sẽ được chuyển sang PROCESSING
    /// 
    /// **Lỗi có thể xảy ra:**
    /// - 400 Bad Request: ID hạng mục không hợp lệ, hạng mục không phải cấp 2, tên công việc trống, 
    ///   tên công việc trùng lặp trong yêu cầu, hoặc tên công việc đã tồn tại trong hạng mục
    /// - 404 Not Found: Không tìm thấy hạng mục xây dựng với ID được cung cấp
    /// 
    /// **Ví dụ yêu cầu:**
    /// 
    ///     {
    ///       "name": "Lợp ngói mái nhà phía Đông",
    ///       "description": "Cập nhật mô tả cho hạng mục lợp ngói mái nhà phía Đông",
    ///       "constructionTasks": [
    ///         {
    ///           "name": "Chuẩn bị ngói",
    ///           "deadlineAt": "2024-07-15T17:00:00"
    ///         },
    ///         {
    ///           "name": "Lắp đặt ngói",
    ///           "deadlineAt": "2024-07-20T17:00:00"
    ///         },
    ///         {
    ///           "name": "Kiểm tra chất lượng",
    ///           "deadlineAt": "2024-07-25T17:00:00"
    ///         }
    ///       ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">ID của hạng mục xây dựng cấp 2 (con) cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật cho hạng mục xây dựng cấp 2 (con) và danh sách công việc mới (nếu có)</param>
    /// <returns>Kết quả cập nhật hạng mục xây dựng</returns>
    /// <response code="200">Cập nhật hạng mục xây dựng thành công</response>
    /// <response code="400">ID hạng mục không hợp lệ, hạng mục không phải cấp 2, tên công việc trống, tên công việc trùng lặp trong yêu cầu, hoặc tên công việc đã tồn tại trong hạng mục</response>
    /// <response code="404">Không tìm thấy hạng mục xây dựng với ID được cung cấp</response>
    [HttpPut("item/{id}/lv2")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Cập nhật hạng mục xây dựng(construction item) cấp 2 (con) và thêm công việc mới",
        Description = 
            "API này cho phép cập nhật thông tin của hạng mục xây dựng(construction item) cấp 2 (con) và thêm các công việc mới.\n\n" +
            "**Quy tắc và hành vi:**\n" +
            "- Đối với hạng mục cấp 2, chỉ cập nhật name và description\n" +
            "- Đối với công việc mới, sử dụng name và deadlineAt\n" +
            "- Tất cả công việc mới được tạo với trạng thái OPENING\n" +
            "- Thời hạn (deadline) được tự động chuyển đổi sang múi giờ Việt Nam và định dạng phù hợp với PostgreSQL\n" +
            "- Nếu hạng mục xây dựng có trạng thái OPENING hoặc DONE, nó sẽ được chuyển sang PROCESSING khi thêm công việc mới\n" +
            "- Tương tự, nếu hạng mục cha có trạng thái OPENING hoặc DONE, nó cũng sẽ được chuyển sang PROCESSING\n\n" +
            "**Lỗi có thể xảy ra:**\n" +
            "- 400 Bad Request: ID hạng mục không hợp lệ, hạng mục(construction item) không phải cấp 2, tên công việc(construction task) trống, " +
            "tên công việc(construction task) trùng lặp trong yêu cầu, hoặc tên công việc(construction task) đã tồn tại trong hạng mục\n" +
            "- 404 Not Found: Không tìm thấy hạng mục xây dựng(construction item) với ID được cung cấp\n\n" +
            "**Ví dụ yêu cầu:**\n\n" +
            "```json\n" +
            "{\n" +
            "  \"name\": \"Lợp ngói mái nhà phía Đông\",\n" +
            "  \"description\": \"Cập nhật mô tả cho hạng mục lợp ngói mái nhà phía Đông\",\n" +
            "  \"constructionTasks\": [\n" +
            "    {\n" +
            "      \"name\": \"Chuẩn bị ngói\",\n" +
            "      \"deadlineAt\": \"2024-07-15T17:00:00\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"name\": \"Lắp đặt ngói\",\n" +
            "      \"deadlineAt\": \"2024-07-20T17:00:00\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"name\": \"Kiểm tra chất lượng\",\n" +
            "      \"deadlineAt\": \"2024-07-25T17:00:00\"\n" +
            "    }\n" +
            "  ]\n" +
            "}\n" +
            "```",
        OperationId = "UpdateConstructionItemLv2",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> UpdateConstructionItemLv2Async(
        [SwaggerParameter(
            Description = "ID của hạng mục xây dựng(construction item) cấp 2 (con) cần cập nhật",
            Required = true
        )]
        Guid id,
        [FromBody]
        [SwaggerParameter(
            Description = "Thông tin cập nhật cho hạng mục xây dựng(construction item) cấp 2 (con) và danh sách công việc(construction task) mới (nếu có)",
            Required = true
        )]
        UpdateConstructionItemLv2Request request
    )
    {   
        await _constructionService.UpdateConstructionItemLv2Async(request, id);
        return Ok();
    }

    [HttpPut("task/{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Cập nhật công việc xây dựng(construction task)",
        Description = 
            "API này cho phép cập nhật thông tin của công việc xây dựng(construction task).\n\n" +
            "**Quy tắc và hành vi:**\n" +
            "- Tên công việc phải là duy nhất trong hạng mục xây dựng(construction item lv2).\n" +
            "- Khi gán nhân viên cho công việc, trạng thái sẽ chuyển từ OPENING sang PROCESSING.\n" +
            "- Khi cập nhật URL hình ảnh(imageUrl), trạng thái sẽ chuyển từ PROCESSING sang PREVIEWING.\n" +
            "- Khi cập nhật lý do(reason), trạng thái sẽ chuyển từ PREVIEWING sang PROCESSING.\n" +
            "- Nhân viên được gán phải thuộc dự án(project staff).\n" +
            "- Nhân viên được gán phải có vị trí là 'constructor'.\n" +
            "- Không thể cập nhật lý do(reason) khi URL hình ảnh(imageUrl) chưa được cung cấp.\n" +
            "- Các field khi update có thể null hoặc không khai báo sẽ không cập nhật.\n\n" +
            "**Lưu ý quan trọng:**\n" +
            "- staffId trong request là UserId của nhân viên (từ bảng User), không phải Id của nhân viên (từ bảng Staff).\n\n" +
            "**Lỗi có thể xảy ra:**\n" +
            "- 400 Bad Request: Tên công việc đã tồn tại trong hạng mục, nhân viên không thuộc dự án, nhân viên không có vị trí 'constructor', hoặc cố gắng cập nhật lý do khi chưa có URL hình ảnh.\n" +
            "- 404 Not Found: Không tìm thấy công việc hoặc nhân viên với ID được cung cấp.\n\n" +
            "**Ví dụ yêu cầu:**\n\n" +
            "```json\n" +
            "{\n" +
            "  \"name\": \"Kiểm tra chất lượng ngói (tùy chọn)\",\n" +
            "  \"deadlineAt\": \"2024-08-15T17:00:00 (tùy chọn)\",\n" +
            "  \"staffId\": \"7e7a3d26-2c0b-4ad2-a95b-bf62838d5e32 (tùy chọn, đây là UserId từ bảng User)\",\n" +
            "  \"reason\": \"Cần kiểm tra kỹ lưỡng chất lượng ngói trước khi lắp đặt (tùy chọn, yêu cầu imageUrl đã tồn tại)\",\n" +
            "  \"imageUrl\": \"https://example.com/images/roof-inspection.jpg (tùy chọn)\"\n" +
            "}\n" +
            "```",
        OperationId = "UpdateConstructionTask",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> UpdateConstructionTaskAsync(
        [SwaggerParameter(
            Description = "ID của công việc xây dựng(construction task) cần cập nhật",
            Required = true
        )]
        Guid id,
        [FromBody]
        [SwaggerParameter(
            Description = 
                "Thông tin cập nhật cho công việc(construction task) xây dựng(construction). Các trường có thể bao gồm:\n" +
                "- name: Tên mới của công việc (tùy chọn)\n" +
                "- deadlineAt: Thời hạn mới của công việc (tùy chọn)\n" +
                "- staffId: ID của người dùng (User.Id) được gán cho công việc, không phải ID của nhân viên (Staff.Id) (tùy chọn, nhân viên phải có vị trí 'constructor')\n" +
                "- reason: Lý do hoặc ghi chú cho công việc (tùy chọn, yêu cầu imageUrl đã tồn tại)\n" +
                "- imageUrl: URL hình ảnh cho công việc (tùy chọn)",
            Required = true
        )]
        UpdateConstructionTaskRequest request
    )
    {   
        await _constructionService.UpdateConstructionTaskAsync(request, id);
        return Ok();
    }

    [HttpDelete("task/{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Xóa công việc xây dựng(construction task)",
        Description = "Xóa công việc xây dựng(construction task) dựa trên ID của công việc, **xóa khỏi hệ thống luôn**.\n"
        + "**Lưu ý:**\n"
        + "- Không thể xóa công việc đang được gán cho nhân viên.\n"
        + "- Chỉ có thể xóa công việc đang ở trạng thái OPENING.\n"
        ,
        OperationId = "DeleteConstructionTask",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> DeleteConstructionTaskAsync(
        [SwaggerParameter(
            Description = "ID của công việc xây dựng(construction task) cần xóa",
            Required = true
        )]
        Guid id
    )
    {
        await _constructionService.DeleteConstructionTaskAsync(id);
        return Ok();    
    }

    [HttpDelete("item/{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Xóa hạng mục xây dựng(construction item)",
        Description = "Xóa hạng mục xây dựng(construction item) dựa trên ID của hạng mục, **xóa khỏi hệ thống luôn**.\n"
        + "**Lưu ý:**\n"
        + "- Chỉ có thể xóa hạng mục đang ở trạng thái OPENING.\n"
        + "- Không thể xóa hạng mục(construction item) đang được gán cho công việc(construction task).\n"
        + "- Không thể xóa hạng mục(construction item) đang được gán cho hạng mục cha(construction item lv1) hoặc ngược lại.\n"
        ,
        OperationId = "DeleteConstructionItem",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> DeleteConstructionItemAsync(
        [SwaggerParameter(
            Description = "ID của hạng mục xây dựng(construction item) cần xóa",
            Required = true
        )]
        Guid id
    )
    {
        await _constructionService.DeleteConstructionItemAsync(id);
        return Ok();
    }

    [HttpPut("task/{id}/confirm")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Xác nhận công việc xây dựng(construction task)",
        Description = "Xác nhận công việc xây dựng(construction task) dựa trên ID của công việc.\n"
        + "**Lưu ý:**\n"
        + "- Chỉ có thể xác nhận công việc đang ở trạng thái PREVIEWING.\n"
        + "- Công việc sẽ được chuyển sang trạng thái DONE.\n"
        + "- Không thể xác nhận công việc(construction task) khi không có URL hình ảnh(imageUrl).\n"
        + "- Khi confirm 1 công việc(construction task), hạng mục xây dựng(construction item lv2)" 
        + " sẽ được tự động confirm khi tất cả công việc(construction task) của nó đã DONE," 
        + " cũng như kiểm tra các hạng mục xây dựng(construction item lv2) trong hạng mục công việc cha(construction item lv1)"
        + " đã được DONE hay chưa và tự động cập nhật trạng thái của hạng mục xây dựng(construction item lv1)"
        + " sang trạng thái DONE.\n",
        OperationId = "ConfirmConstructionTask",
        Tags = new[] { "Constructions" }
    )]
    public async Task<ApiResult> ConfirmConstructionTaskAsync(
        [SwaggerParameter(
            Description = "ID của công việc xây dựng(construction task) cần xác nhận",
            Required = true
        )]
        Guid id
    )
    {
        await _constructionService.ConfirmConstructionTaskAsync(id);
        return Ok();
    }
}