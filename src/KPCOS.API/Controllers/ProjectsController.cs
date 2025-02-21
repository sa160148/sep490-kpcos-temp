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

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class ProjectsController(IProjectService service, IAuthService authService) : BaseController
    {
        [HttpGet("")]
        public async Task<PagedApiResponse<ProjectResponse>> GetsAsync([FromQuery] PaginationFilter filter)
        {
            var count = await service.CountAsync();
            if (count == 0)
            {
                return new PagedApiResponse<ProjectResponse>(new List<ProjectResponse>(),
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: count);
            }
            var projects = await service.GetsAsync();
            return new PagedApiResponse<ProjectResponse>(projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }

        [HttpGet("{id}")]
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
        public async Task<ApiResult> CreateAsync(ProjectRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Vui lòng đăng nhập lại");
            }
            var isValidPosition = await authService.GetPositionAsync(Guid.Parse(userId));
            if (isValidPosition != RoleEnum.CUSTOMER)
            {
                throw new Exception("Không có khả năng truy cập");
            }

            bool isCreated = await service.CreateAsync(request);
            return new ApiResult(isCreated, 
                isCreated ? ApiResultStatusCode.Success : ApiResultStatusCode.ServerError);
        }

        [HttpDelete("{id}")]
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            bool isDeleted = await service.DeleteAsync(id);
            return new ApiResult(isDeleted, 
                isDeleted ? ApiResultStatusCode.Success : ApiResultStatusCode.ServerError);
        }
    }
}
