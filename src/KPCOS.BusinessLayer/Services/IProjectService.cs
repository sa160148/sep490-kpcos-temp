using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponse>> GetsAsync();
    Task<IEnumerable<ProjectResponse>> GetsAsync(PaginationFilter filter);

    Task<ProjectResponse> GetAsync(Guid id);
    Task<int> CountAsync();

    Task<bool> CreateAsync(ProjectRequest request);
    Task<bool> UpdateAsync(Guid id, ProjectRequest request);
    Task<bool> DeleteAsync(Guid id);
}