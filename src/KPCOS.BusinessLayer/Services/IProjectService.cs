using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectForListResponse>> GetsAsync(PaginationFilter filter, string? userId, string role);
    
    /// <summary>
    /// Gets all projects for a user with quotation-related information and standout status
    /// </summary>
    /// <param name="filter">Filter and pagination parameters including optional status filtering</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user (ADMINISTRATOR, CONSULTANT, etc.)</param>
    /// <returns>Collection of projects with quotation information and standout status</returns>
    /// <remarks>
    /// <para>Access Rules:</para>
    /// <list type="bullet">
    ///     <item><description>Administrators can see all projects</description></item>
    ///     <item><description>Other users can only see their own projects or projects they're assigned to</description></item>
    /// </list>
    /// <para>StandOut Flag Rules by Role:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Administrator: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>Open quotations</description></item>
    ///                 <item><description>Approved quotations without active/processing contracts</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Consultant: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>No quotations</description></item>
    ///                 <item><description>Quotations in UPDATING or REJECTED status</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Customer: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>Contracts in PROCESSING status</description></item>
    ///                 <item><description>PREVIEW quotations without any APPROVED/UPDATING quotations</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// <list type="bullet">
    ///     <item><description>Filter is null</description></item>
    ///     <item><description>Status filter contains invalid project status</description></item>
    ///     <item><description>UserId is null or invalid</description></item>
    /// </list>
    /// </exception>
    Task<IEnumerable<GetAllProjectForQuotationResponse>> GetAllProjectForQuotationByUserIdAsync(
        GetAllProjectByUserIdRequest filter, 
        string? userId, 
        string? role = null);

    Task<ProjectResponse> GetAsync(Guid id);
    Task<int> CountAsync();
    int CountProjectByUserIdAsync(Guid userId);

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

    /// <summary>
    /// Gets all projects for design purposes with design-related information and standout status
    /// </summary>
    /// <param name="advandcedFilter">Filter and pagination parameters including optional status filtering</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user (ADMINISTRATOR, MANAGER, DESIGNER, etc.)</param>
    /// <returns>Collection of projects with design information and standout status</returns>
    /// <remarks>
    /// <para>Access Rules:</para>
    /// <list type="bullet">
    ///     <item><description>Administrators can see all projects</description></item>
    ///     <item><description>Other users can only see their own projects or projects they're assigned to</description></item>
    /// </list>
    /// <para>StandOut Flag Rules by Role:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Administrator: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>No manager assigned</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Manager: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>No designer assigned</description></item>
    ///                 <item><description>OR any designs in OPENING status</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Designer: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>No designs</description></item>
    ///                 <item><description>OR designs in REJECTED/EDITING status</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>Customer: Projects marked when they have:
    ///             <list type="bullet">
    ///                 <item><description>Any design in PREVIEWING status</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// <list type="bullet">
    ///     <item><description>Filter is null</description></item>
    ///     <item><description>Status filter contains invalid project status</description></item>
    ///     <item><description>UserId is null or invalid</description></item>
    /// </list>
    /// </exception>
    Task<IEnumerable<GetAllProjectForDesignResponse>> GetAllProjectForDesignByUserIdAsync(
        GetAllProjectByUserIdRequest advandcedFilter, 
        string userId, 
        string? role = null);

    Task<(IEnumerable<GetAllContractResponse> data, int total)> GetContractByProjectAsync(Guid id, PaginationFilter filter);
}