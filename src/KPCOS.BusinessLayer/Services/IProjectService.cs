using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectForListResponse>> GetsAsync(PaginationFilter filter, string? userId, string role);

    Task<ProjectResponse> GetAsync(Guid id);
    Task<int> CountAsync();

    Task CreateAsync(ProjectRequest request, Guid userId);
    Task<IEnumerable<StaffResponse>> GetsConsultantAsync(PaginationFilter filter, Guid projectId);
    Task AssignConsultantAsync(Guid id, StaffAssignRequest request);
}