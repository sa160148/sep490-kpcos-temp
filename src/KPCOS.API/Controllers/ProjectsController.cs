using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.BusinessLayer.Services;

using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using Swashbuckle.AspNetCore.Annotations;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Request.Payments;

namespace KPCOS.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    /// <param name="authService"></param>
    [Route("api/[controller]")]
    public class ProjectsController(
        IProjectService service, 
        IAuthService authService, 
        IConstructionServices constructionService, 
        IPaymentService paymentService) : BaseController
    {
        /// <summary>
        /// Get all project for each role of user
        /// </summary>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        ///
        /// pageNumber: int.
        /// pageSize: int.
        /// </param>
        /// <remarks>
        /// <para>Retrieve a paginate list of project for each role user.
        /// ADMINISTRATOR can get all project, CUSTOMER can get all project that they created, staff* can get all project that they assigned.</para>
        /// <para>staff* is CONSULTANT, DESIGNER, CONSTRUCTOR.</para>
        /// Sample request:
        /// 
        ///     Get /api/projects
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedApiResponse<ProjectForListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<PagedApiResponse<ProjectForListResponse>> GetsAsync([FromQuery] PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var count = await service.CountAsync();
            if (count == 0)
            {
                return new PagedApiResponse<ProjectForListResponse>(new List<ProjectForListResponse>(),
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: count);
            }
            var projects = await service.GetsAsync(filter, userId, role);
            return new PagedApiResponse<ProjectForListResponse>(projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }
        
        /// <summary>
        /// Gets projects for consultation with quotation base on project with actor: ADMINISTRATOR, CONSULTANT, CUSTOMER
        /// </summary>
        /// <param name="filter">Pagination parameters (pageNumber and pageSize)</param>
        /// <returns>Paginated list of projects with quotation information and standout status</returns>
        /// <remarks>
        /// <para>Retrieve a paginated list of project for consultation with each Role user: Administrator, Customer, Staff Consultant.</para>
        /// <para>This endpoint returns projects with status REQUESTING and PROCESSING.</para>
        /// 
        /// Projects are marked as standout based on role:
        /// 
        /// For Administrator:
        /// - Has open quotations
        /// - Has approved quotations without active/processing contracts
        /// 
        /// For Consultant:
        /// - Has no quotations
        /// - Has quotations in UPDATING or REJECTED status
        /// 
        /// For Customer:
        /// - Has contracts in PROCESSING status
        /// - Has PREVIEW quotations without any APPROVED/UPDATING quotations
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of projects</response>
        /// <response code="401">Unauthorized. User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("consultation")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectForQuotationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllProjectForQuotationResponse>> GetsProjectForConsultationAsync(
            [FromQuery] PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            var advandcedFilter = new GetAllProjectByUserIdRequest
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Status = new List<string>()
                {
                    EnumProjectStatus.REQUESTING.ToString(),
                    EnumProjectStatus.PROCESSING.ToString()
                },
            };
            
            // Get projects and count in a single query
            var (projects, count) = await service.GetAllProjectForQuotationByUserIdAsync(advandcedFilter, userId, role);
            
            if (count == 0)
            {
                return new PagedApiResponse<GetAllProjectForQuotationResponse>(
                    new List<GetAllProjectForQuotationResponse>(),
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: 0);
            }
            
            return new PagedApiResponse<GetAllProjectForQuotationResponse>(
                projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }
        
        /// <summary>
        /// Get projects for design phase with staff information and standout flags
        /// </summary>
        /// <param name="filter">Pagination parameters (PageNumber and PageSize)</param>
        /// <returns>Paginated list of projects with design information, staff details, and standout status</returns>
        /// <remarks>
        /// This endpoint returns projects with status DESIGNING and includes:
        /// - Basic project information (name, address, area, etc.)
        /// - Staff assigned to the project (designers, managers, etc.)
        /// - Latest design image URL
        /// - Standout flag based on user role
        /// 
        /// Standout flag rules by role:
        /// 
        /// For Administrator:
        /// - Project has no manager assigned
        /// 
        /// For Manager:
        /// - Project has no designer assigned, OR
        /// - Project has designs in OPENING status
        /// 
        /// For Designer:
        /// - Project has no designs, OR
        /// - Project has designs in REJECTED or EDITING status
        /// 
        /// For Customer:
        /// - Project has any design in PREVIEWING status
        /// 
        /// Sample request:
        /// 
        ///     GET /api/projects/design?PageNumber=1&amp;PageSize=10
        /// 
        /// Sample response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Project Name",
        ///       "customerName": "Customer Name",
        ///       "address": "123 Main St",
        ///       "area": 150,
        ///       "status": "DESIGNING",
        ///       "imageUrl": "https://example.com/image.jpg",
        ///       "standOut": true,
        ///       "staffs": [
        ///         {
        ///           "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "fullName": "Staff Name",
        ///           "email": "staff@example.com",
        ///           "position": "DESIGNER",
        ///           "avatar": "https://example.com/avatar.jpg"
        ///         }
        ///       ]
        ///     }
        ///   ],
        ///   "pageNumber": 1,
        ///   "pageSize": 10,
        ///   "totalPages": 1,
        ///   "totalRecords": 1
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of projects with design information and staff details</response>
        /// <response code="401">Unauthorized. User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("design")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectForDesignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<PagedApiResponse<GetAllProjectForDesignResponse>> GetsProjectForDesignAsync(
            [FromQuery] PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            var advandcedFilter = new GetAllProjectByUserIdRequest
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Status = new List<string>()
                {
                    EnumProjectStatus.DESIGNING.ToString()
                },
            };
            
            // Get projects and count in a single query
            var (projects, count) = await service.GetAllProjectForDesignByUserIdAsync(advandcedFilter, userId, role);
            
            if (count == 0)
            {
                return new PagedApiResponse<GetAllProjectForDesignResponse>(
                    new List<GetAllProjectForDesignResponse>(),
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: 0);
            }
            
            return new PagedApiResponse<GetAllProjectForDesignResponse>(
                projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }

        /// <summary>
        /// Retrieve a project by id
        /// </summary>
        /// <param name="id">
        /// <para><see cref="Guid"/> request contains: </para>
        ///
        /// id: guid.
        /// </param>
        /// <remarks>
        /// <para>Retrieve a project by id.</para>
        /// Sample request:
        /// 
        ///     Get /api/projects{id}
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResult<ProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<ApiResult<ProjectResponse>> GetAsync(Guid id)
        {
            var project = await service.GetAsync(id);
            return project;
        }

        /// <summary>
        /// Create project
        /// </summary>
        /// <param name="request">
        /// <para><see cref="ProjectRequest"/> request object contains: </para>
        ///
        /// customerName: string.
        /// email: string.
        /// password: string.
        /// phone: string.
        /// address: string.
        /// area: float.
        /// depth: float.
        /// note: string.
        /// packageId: guid.
        /// templatedesignid: guid.
        ///</param>
        /// <returns>
        /// An Object with a JSON format.  <see cref="ApiResult"/>
        /// </returns>
        /// <remarks>
        /// <para>Customer request a new project, this mean customer create a project.</para>  
        /// Sample request:
        /// 
        ///     POST /api/projects
        ///     {
        ///         "customerName": "Customer 1",
        ///         "address": "HCM",
        ///         "phone": "0123456789",
        ///         "email": "",
        ///         "area": 100,
        ///         "depth": 100,
        ///         "packageId": "5ca78687-26db-40ed-99d0-685dff2b7e3e",
        ///         "note": "Note 1",
        ///         "templatedesignid": "5ca78687-26db-40ed-99d0-685dff2b7e3e"
        ///      }
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize("CUSTOMER")]
        public async Task<ApiResult> CreateAsync(ProjectRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await service.CreateAsync(request, Guid.Parse(userId));
            return Ok();
        }

        /// <summary>
        /// Assign staff to project based on project status
        /// </summary>
        /// <param name="request">
        /// <para><see cref="StaffAssignRequest"/> request object contains: </para>
        /// 
        /// staffId: guid (UserId from Staff table).
        ///</param>
        /// <param name="id">Project ID</param>
        /// <returns>An Object with a JSON format. <see cref="ApiResult"/></returns>
        /// <remarks>
        /// <para>Only admin can do this.</para>
        /// <para>Assigns staff to project following the status chain:</para>
        /// <para>REQUESTING -> Assign Consultant -> PROCESSING</para>
        /// <para>DESIGNING -> Assign Manager -> DESIGNING</para>
        /// <para>DESIGNING -> Assign Designer -> DESIGNING</para>
        /// <para>CONSTRUCTING -> Assign Constructor -> CONSTRUCTING</para>
        /// </remarks>
        [HttpPost("{id}/assignconsultant")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        // [CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> AssignStaffAsync(Guid id, StaffAssignRequest request)
        {
            await service.AssignStaffAsync(id, request.StaffId);
            return Ok();
        }
        
        /// <summary>
        /// Get quotation by project
        /// </summary>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        ///
        /// pageNumber: int.
        /// pageSize: int.
        /// </param>
        /// <param name="id">Project ID</param>
        /// <remarks>
        /// <para>Retrieve a paginated list of quotation by project.</para>
        /// Sample request:
        /// 
        ///     Get /api/projects/{id}/quotation
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("{id}/quotation")]
        public async Task<PagedApiResponse<QuotationForProjectResponse>> GetQuotationsByProjectAsync(Guid id, [FromQuery] GetAllQuotationFilterRequest filter)
        {
            var quotations = await service.GetQuotationsByProjectAsync(id, filter);
            return new PagedApiResponse<QuotationForProjectResponse>(quotations.data,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: quotations.total);
        }
        
        /// <summary>
        /// Get all contracts for a specific project
        /// </summary>
        /// <param name="id">The project ID to get contracts for</param>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        /// pageNumber: int - The page number to retrieve
        /// pageSize: int - The number of items per page
        /// </param>
        /// <returns>Paginated list of contracts with their associated quotation total prices</returns>
        /// <remarks>
        /// <para>Retrieves a paginated list of contracts for a specific project.</para>
        /// <para>Each contract includes:</para>
        /// <list type="bullet">
        ///     <item><description>Contract details (ID, name, status, etc.)</description></item>
        ///     <item><description>Contract value from the contract itself</description></item>
        /// </list>
        /// Sample request:
        /// 
        ///     GET /api/projects/{id}/contract?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of contracts</response>
        /// <response code="404">Project not found</response>
        /// <response code="400">Project is inactive</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/contract")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllContractResponse>> GetAllContractByProjectAsync(Guid id, [FromQuery]GetAllContractFilterRequest filter)
        {
            var contract = await service.GetContractByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllContractResponse>(contract.data, filter.PageNumber, filter.PageSize, contract.total);
        }
        
        /// <summary>
        /// Get all designs for a specific project
        /// </summary>
        /// <param name="id">The project ID to get designs for</param>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        /// pageNumber: int - The page number to retrieve
        /// pageSize: int - The number of items per page
        /// </param>
        /// <returns>Paginated list of designs with their associated images and staff information</returns>
        /// <remarks>
        /// <para>Retrieves a paginated list of designs for a specific project.</para>
        /// <para>Each design includes:</para>
        /// <list type="bullet">
        ///     <item><description>Basic design information (ID, version, status, etc.)</description></item>
        ///     <item><description>Design images associated with the design</description></item>
        ///     <item><description>Staff information who created the design</description></item>
        /// </list>
        /// Sample request:
        /// 
        ///     GET /api/projects/{id}/design?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of designs</response>
        /// <response code="404">Project not found</response>
        /// <response code="400">Project is inactive</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/design")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllDesignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllDesignResponse>> GetAllDesignByProjectAsync(Guid id, [FromQuery]GetAllDesignFilterRequest filter)
        {
            var design = await service.GetAllDesignByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllDesignResponse>(design.data, filter.PageNumber, filter.PageSize, design.total);
        }
        
        /// <summary>
        /// Gets a paginated list of construction items for a specific project
        /// </summary>
        /// <param name="id">The ID of the project</param>
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
        ///     GET /api/projects/{id}/construction?Search=foundation&amp;IsActive=true&amp;Status=OPENING&amp;IsPayment=true&amp;PageNumber=1&amp;PageSize=10
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
        [HttpGet("{id}/construction")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionItemResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of construction items for a specific project",
            Description = "Retrieves construction items for the specified project based on the provided filter criteria. Parent items are returned with their child items populated in the Childs property.",
            OperationId = "GetAllConstructionItemsByProject",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllConstructionItemResponse>> GetAllConstructionItemsByProjectAsync(Guid id,
            [FromQuery] 
            [SwaggerParameter(
                Description = "Filter criteria for construction items including Search, IsActive, Status (OPENING, PROCESSING, DONE), IsPayment, IsChild, PageNumber, PageSize, SortColumn, and SortDir",
                Required = false
            )]
            GetAllConstructionItemFilterRequest filter)
        {
            var (data, total) = await constructionService.GetAllConstructionItemsAsync(filter, id);
            return new PagedApiResponse<GetAllConstructionItemResponse>(data, filter.PageNumber, filter.PageSize, total);
        }

        [HttpGet("{id}/design/3d-confirmed")]
        [ProducesResponseType(typeof(ApiResult<IsDesignExitByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any confirmed 3D designs",
            Description = "Determines whether the specified project has any designs with type '3D' and status 'CONFIRMED'",
            OperationId = "IsDesign3DConfirmedAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsDesignExitByProjectResponse>> IsDesign3DConfirmedAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for confirmed 3D designs",
                Required = true
            )]
            Guid id)
        {
            var designs = await service.IsDesign3DConfirmedAsync(id);
            return Ok(designs);
        }

        [HttpGet("{id}/quotation/approved")]
        [ProducesResponseType(typeof(ApiResult<IsQuotationApprovedByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any approved quotations",
            Description = "Determines whether the specified project has any quotations with status 'APPROVED'",
            OperationId = "IsQuotationApprovedByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsQuotationApprovedByProjectResponse>> IsQuotationApprovedByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for approved quotations",
                Required = true
            )]
            Guid id)
        {
            var isQuotationApproved = await service.IsQuotationApprovedByProjectAsync(id);
            return Ok(isQuotationApproved);
        }

        [HttpGet("{id}/contract/active")]
        [ProducesResponseType(typeof(ApiResult<IsContractApprovedByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any active contracts",
            Description = "Determines whether the specified project has any contracts with status 'ACTIVE'",
            OperationId = "IsContractApprovedByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsContractApprovedByProjectResponse>> IsContractApprovedByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for active contracts",
                Required = true
            )]
            Guid id)
        {
            var isContractApproved = await service.IsContractApprovedByProjectAsync(id);
            return Ok(isContractApproved);
        }

        /// <summary>
        /// Get all staff assigned to a specific project with filtering options
        /// </summary>
        /// <param name="id">The project ID to get staff for</param>
        /// <param name="filter">Filter parameters including Position, IsIdle, and pagination</param>
        /// <returns>A paginated list of staff assigned to the project</returns>
        /// <remarks>
        /// <para>This endpoint returns staff assigned to a project with filtering options:</para>
        /// <list type="bullet">
        ///     <item><description>Position: Filter by staff position (CONSTRUCTOR, MANAGER, CONSULTANT, etc.)</description></item>
        ///     <item><description>IsIdle: For constructors, returns only those not assigned to any active construction tasks</description></item>
        /// </list>
        /// <para>Sample request:</para>
        /// 
        ///     GET /api/projects/{id}/staff?Position=CONSTRUCTOR&amp;IsIdle=true&amp;PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Returns the list of staff</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}/staff")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllStaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all staff assigned to a specific project with filtering options",
            Description = "Returns staff assigned to a project with filtering by position and idle status",
            OperationId = "GetAllStaffByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllStaffResponse>> GetAllStaffByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get staff for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for staff including Position, IsIdle, and pagination parameters",
                Required = false
            )]
            GetAllStaffRequest filter)
        {
            var staffs = await service.GetAllStaffByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllStaffResponse>(staffs.data, filter.PageNumber, filter.PageSize, staffs.total);
        }

        [HttpGet("{id}/construction-task")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of construction tasks for a specific project",
            Description = "Retrieves construction tasks for the specified project based on the provided filter criteria. If the logged-in user is a CONSTRUCTOR, only returns tasks assigned to them. All other roles see all tasks for the project.",
            OperationId = "GetAllConstructionTaskByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllConstructionTaskResponse>> GetAllConstructionTaskByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get construction tasks for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for construction tasks including Search, IsActive, Status, IsOverdue, and ConstructionItemId",
                Required = false
            )]
            KPCOS.BusinessLayer.DTOs.Request.Constructions.GetAllConstructionTaskFilterRequest filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllConstructionTaskResponse> data, int total) constructionTasks;
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                constructionTasks = await service.GetAllConstructionTaskByProjectAsync(id, filter, userId);
                return new PagedApiResponse<GetAllConstructionTaskResponse>(constructionTasks.data, filter.PageNumber, filter.PageSize, constructionTasks.total);
        
            }
            constructionTasks = await service.GetAllConstructionTaskByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllConstructionTaskResponse>(constructionTasks.data, filter.PageNumber, filter.PageSize, constructionTasks.total);
        }

        [HttpGet("{id}/project-issue")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectIssueResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of project issues for a specific project",
            Description = "Retrieves project issues for the specified project based on the provided filter criteria. If the logged-in user is a CONSTRUCTOR, only returns issues assigned to them. All other roles see all issues for the project.",
            OperationId = "GetAllProjectIssueByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllProjectIssueResponse>> GetAllProjectIssueByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get issues for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for project issues including Search, Status, IssueTypeId, ConstructionItemId, and UserId",
                Required = false
            )]
            GetAllProjectIssueFilterRequest filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllProjectIssueResponse> data, int total) projectIssues;
            
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                projectIssues = await service.GetAllProjectIssueByProjectAsync(id, filter, userId);
                return new PagedApiResponse<GetAllProjectIssueResponse>(projectIssues.data, filter.PageNumber, filter.PageSize, projectIssues.total);
            }
            
            projectIssues = await service.GetAllProjectIssueByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllProjectIssueResponse>(projectIssues.data, filter.PageNumber, filter.PageSize, projectIssues.total);
        }

        [HttpPut("{id}/finish")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResult> FinishProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to finish",
                Required = true
            )]
            Guid id)
        {
            await service.FinishProjectAsync(id);
            return Ok();
        }

        [HttpGet("{id}/docs")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllDocResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllDocResponse>> GetAllDocAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get docs for",
                Required = true
            )]
            Guid id,
            [FromQuery]
            GetAllDocFilterRequest filter)
        {
            var docs = await service.GetAllDocAsync(id, filter);
            return new PagedApiResponse<GetAllDocResponse>(docs.data, filter.PageNumber, filter.PageSize, docs.total);
        }

        [HttpGet("{id}/payment")]
        [ProducesResponseType(typeof(PagedApiResponse<GetTransactionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetTransactionDetailResponse>> GetAllPaymentAsync(
            Guid id, 
            [FromQuery] GetAllTransactionFilterRequest filter)
        {
            // Set Related to "batch" if not already specified, to ensure we get payment batch transactions
            if (string.IsNullOrEmpty(filter.Related))
            {
                filter.Related = "batch";
            }
            
            var payments = await paymentService.GetTransactionsAsync(filter, projectId: id);
            return new PagedApiResponse<GetTransactionDetailResponse>(
                payments.data, 
                filter.PageNumber, 
                filter.PageSize, 
                payments.total);
        }
    }
}