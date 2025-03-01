using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.Services;

using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;

namespace KPCOS.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    /// <param name="authService"></param>
    [Route("api/[controller]")]
    public class ProjectsController(IProjectService service, IAuthService authService) : BaseController
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
            var count = service.CountProjectByUserIdAsync(Guid.Parse(userId));
            
            var advandcedFilter = new GetAllProjectByUserIdRequest
            {
                page = filter.PageNumber,
                per_page = filter.PageSize,
                Status = new List<string>()
                {
                    EnumProjectStatus.REQUESTING.ToString(),
                    EnumProjectStatus.PROCESSING.ToString()
                },
            };
            
            var projects = await service.GetAllProjectForQuotationByUserIdAsync(advandcedFilter, userId, role);
            return new PagedApiResponse<GetAllProjectForQuotationResponse>(
                projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }
        
        /// <summary>
        /// Gets projects for design with standout flags based on user role: ADMINISTRATOR, MANAGER, DESIGNER, CUSTOMER
        /// </summary>
        /// <param name="filter">Pagination parameters (pageNumber and pageSize)</param>
        /// <returns>Paginated list of projects with design information and standout status</returns>
        /// <remarks>
        /// This endpoint returns projects with status DESIGNING.
        /// 
        /// Projects are marked as standout based on role:
        /// 
        /// For Administrator:
        /// - Project has no manager assigned
        /// 
        /// For Manager:
        /// - Project has no designer assigned
        /// - OR project has any designs in OPENING status
        /// 
        /// For Designer:
        /// - Project has no designs
        /// - OR project has designs in REJECTED/EDITING status
        /// 
        /// For Customer:
        /// - Project has any design in PREVIEWING status
        /// 
        /// Sample request:
        /// 
        ///     GET /api/projects/design?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of projects</response>
        /// <response code="401">Unauthorized. User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("design")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectForDesignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        /*[CustomAuthorize]*/
        public async Task<PagedApiResponse<GetAllProjectForDesignResponse>> GetsProjectForDesignAsync(
            [FromQuery] PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var count = service.CountProjectByUserIdAsync(Guid.Parse(userId));
            
            var advandcedFilter = new GetAllProjectByUserIdRequest
            {
                page = filter.PageNumber,
                per_page = filter.PageSize,
                Status = new List<string>()
                {
                    EnumProjectStatus.DESIGNING.ToString()
                },
            };
            
            var projects = await service.GetAllProjectForDesignByUserIdAsync(advandcedFilter, userId, role);
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
        /// <para>PROCESSING -> Assign Designer -> DESIGNING</para>
        /// <para>DESIGNING -> Assign Constructor -> CONSTRUCTING</para>
        /// </remarks>
        [HttpPost("{id}/assignconsultant")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize("ADMINISTRATOR")]
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
        public async Task<PagedApiResponse<QuotationForProjectResponse>> GetQuotationsByProjectAsync(Guid id, [FromQuery] PaginationFilter filter)
        {
            var count = service.CountQuotationByProject(id);
            if (count == 0)
            {
                return new PagedApiResponse<QuotationForProjectResponse>(new List<QuotationForProjectResponse>(),
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: count);
            }
            var quotations = await service.GetQuotationsByProjectAsync(id, filter);
            return new PagedApiResponse<QuotationForProjectResponse>(quotations,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }
    }
}