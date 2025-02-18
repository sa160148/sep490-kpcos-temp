using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class ProjectsController(IProjectService service) : BaseController
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

        [HttpPost("")]
        public async Task<ApiResult> CreateAsync(ProjectRequest request)
        {
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
