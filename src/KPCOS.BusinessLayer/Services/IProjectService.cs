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
    
    
    /// <summary>
    /// Assigns staff to a project following the project status chain and validation rules
    /// </summary>
    /// <param name="projectId">The ID of the project to assign staff to</param>
    /// <param name="userId">The User ID of the staff member (from Staff.UserId)</param>
    /// <remarks>
    /// <para>Project Status Chain and Staff Assignment Rules:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>REQUESTING → PROCESSING: Only Consultant can be assigned</description>
    ///     </item>
    ///     <item>
    ///         <description>PROCESSING → DESIGNING: Only Designer can be assigned</description>
    ///     </item>
    ///     <item>
    ///         <description>DESIGNING → CONSTRUCTING: Only Constructor can be assigned</description>
    ///     </item>
    /// </list>
    /// <para>Position-specific Rules:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Consultant: Cannot be assigned if they have any PROCESSING projects</description>
    ///     </item>
    ///     <item>
    ///         <description>Designer: Cannot be assigned if they have any DESIGNING projects</description>
    ///     </item>
    ///     <item>
    ///         <description>Constructor: Cannot be assigned if they have any CONSTRUCTING projects</description>
    ///     </item>
    ///     <item>
    ///         <description>Administrator: Cannot be assigned to any project</description>
    ///     </item>
    /// </list>
    /// <para>Uniqueness Rules:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Consultant: Only one per project</description>
    ///     </item>
    ///     <item>
    ///         <description>Designer: Only one per project</description>
    ///     </item>
    ///     <item>
    ///         <description>Manager: Only one per project</description>
    ///     </item>
    ///     <item>
    ///         <description>Constructor: Multiple allowed per project</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project or staff not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// <list type="bullet">
    ///     <item><description>Staff position doesn't match project status</description></item>
    ///     <item><description>Staff already has restricted project status</description></item>
    ///     <item><description>Position already exists in project (for unique positions)</description></item>
    ///     <item><description>Administrator attempts to be assigned</description></item>
    /// </list>
    /// </exception>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AssignStaffAsync(Guid projectId, Guid userId);

    int CountQuotationByProject(Guid id);
    Task<IEnumerable<QuotationForProjectResponse>> GetQuotationsByProjectAsync(Guid id, PaginationFilter filter);
}