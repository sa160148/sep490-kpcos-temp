using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.BusinessLayer.Services.Implements;
using KPCOS.Common;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace KPCOS.API.Controllers
{
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