using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Pagination;
using KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Docs;

namespace KPCOS.BusinessLayer.Services;

public interface IProjectService
{
    Task<(IEnumerable<ProjectForListResponse> Data, int Count)> GetsAsync(
        GetAllProjectFilterRequest filter, 
        Guid? userId = null, 
        string? role = null);
    
    /// <summary>
    /// Gets all projects for a user with quotation-related information and standout status
    /// </summary>
    /// <param name="filter">Filter and pagination parameters including optional status filtering</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user (ADMINISTRATOR, CONSULTANT, etc.)</param>
    /// <returns>Tuple containing collection of projects with quotation information and the total count</returns>
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
    Task<(IEnumerable<GetAllProjectForQuotationResponse> Data, int Count)> GetAllProjectForQuotationByUserIdAsync(
        GetAllProjectByUserIdRequest filter, 
        string? userId, 
        string? role = null);

    Task<ProjectResponse> GetAsync(Guid id);
    Task<int> CountAsync();
    int CountProjectByUserIdAsync(Guid userId);

    Task CreateAsync(
        ProjectRequest request, 
        Guid userId);
    Task<IEnumerable<StaffResponse>> GetsConsultantAsync(
        PaginationFilter filter, 
        Guid projectId);
    
    
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
    Task AssignStaffAsync(
        Guid projectId, 
        Guid userId);

    int CountQuotationByProject(Guid id);
    Task<(IEnumerable<QuotationForProjectResponse> data, int total)> GetQuotationsByProjectAsync(
        Guid id, 
        GetAllQuotationFilterRequest filter);

    /// <summary>
    /// Gets all projects for design purposes with design-related information and standout status
    /// </summary>
    /// <param name="advandcedFilter">Filter and pagination parameters including optional status filtering</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user (ADMINISTRATOR, MANAGER, DESIGNER, etc.)</param>
    /// <returns>Tuple containing collection of projects with design information and the total count</returns>
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
    Task<(IEnumerable<GetAllProjectForDesignResponse> Data, int Count)> GetAllProjectForDesignByUserIdAsync(
        GetAllProjectByUserIdRequest advandcedFilter, 
        string userId, 
        string? role = null);

    /// <summary>
    /// Gets all contracts associated with a specific project with pagination
    /// </summary>
    /// <param name="filter">Pagination parameters (pageNumber and pageSize)</param>
    /// <returns>Tuple containing the list of contracts and total count</returns>
    /// <remarks>
    /// <para>Returns active contracts for a project with:</para>
    /// <list type="bullet">
    ///     <item><description>Basic contract information</description></item>
    ///     <item><description>Contract value from the contract itself</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="BadRequestException">Thrown when project is inactive</exception>
    Task<(IEnumerable<GetAllContractResponse> data, int total)> GetContractByProjectAsync(
        GetAllContractFilterRequest filter);

    /// <summary>
    /// Gets all designs associated with a specific project with pagination
    /// </summary>
    /// <param name="id">The project ID to get designs for</param>
    /// <param name="filter">Pagination parameters (pageNumber and pageSize)</param>
    /// <returns>Tuple containing the list of designs and total count</returns>
    /// <remarks>
    /// <para>Returns all designs for a project with:</para>
    /// <list type="bullet">
    ///     <item><description>Basic design information (ID, version, status, etc.)</description></item>
    ///     <item><description>Design images associated with each design</description></item>
    ///     <item><description>Staff information who created the design</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="BadRequestException">Thrown when project is inactive</exception>
    Task<(IEnumerable<GetAllDesignResponse> data, int total)> GetAllDesignByProjectAsync(
        Guid id, 
        GetAllDesignFilterRequest filter);

    Task<IsDesignExitByProjectResponse> IsDesign3DConfirmedAsync(Guid id);

    /// <summary>
    /// Checks if a project has any approved quotations
    /// </summary>
    /// <param name="id">The project ID to check</param>
    /// <returns>Response indicating whether the project has any approved quotations</returns>
    /// <remarks>
    /// <para>This method checks if there are any quotations with status 'APPROVED' for the specified project</para>
    /// <para>Used to determine if a project can proceed to the next stage in the workflow</para>
    /// </remarks>
    Task<IsQuotationApprovedByProjectResponse> IsQuotationApprovedByProjectAsync(Guid id);

    /// <summary>
    /// Checks if a project has any active contracts
    /// </summary>
    /// <param name="id">The project ID to check</param>
    /// <returns>Response indicating whether the project has any active contracts</returns>
    /// <remarks>
    /// <para>This method checks if there are any contracts with status 'ACTIVE' for the specified project</para>
    /// <para>Used to determine if a project has progressed to the active contract stage in the workflow</para>
    /// </remarks>
    Task<IsContractApprovedByProjectResponse> IsContractApprovedByProjectAsync(Guid id);

    Task<(IEnumerable<GetAllStaffResponse> data, int total)> GetAllStaffByProjectAsync(
        Guid id, 
        GetAllStaffRequest filter);

    /// <summary>
    /// [DEPRECATED] Gets all construction tasks associated with a specific project with pagination and filtering
    /// </summary>
    /// <param name="filter">Filter criteria for construction tasks including search, status, overdue status, etc.</param>
    /// <returns>Tuple containing the list of construction tasks and total count</returns>
    /// <remarks>
    /// <para>Returns construction tasks for a project with:</para>
    /// <list type="bullet">
    ///     <item><description>Basic task information (ID, name, status, deadlines, etc.)</description></item>
    ///     <item><description>Associated staff information (if assigned)</description></item>
    ///     <item><description>Parent construction item information</description></item>
    /// </list>
    /// <para>Tasks can be filtered by:</para>
    /// <list type="bullet">
    ///     <item><description>Search term (matches against task name)</description></item>
    ///     <item><description>Active status</description></item>
    ///     <item><description>Task status (OPENING, PROCESSING, PREVIEWING, DONE)</description></item>
    ///     <item><description>Overdue status (tasks with deadlines in the past that are not DONE)</description></item>
    ///     <item><description>Construction item ID (to get tasks for a specific construction item)</description></item>
    /// </list>
    /// <para>If the user with userId is a CONSTRUCTOR, only tasks assigned to them are returned.
    /// For all other staff roles, all tasks for the project are returned regardless of assignment.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="BadRequestException">Thrown when project is inactive</exception>
    Task<(IEnumerable<GetAllConstructionTaskResponse> data, int total)> GetAllConstructionTaskByProjectAsync(
        GetAllConstructionTaskFilterRequest filter);

    /// <summary>
    /// Gets all project issues associated with a specific project with pagination and filtering
    /// </summary>
    /// <param name="id">The project ID to get issues for</param>
    /// <param name="filter">Filter criteria for project issues including search, status, issue type, etc.</param>
    /// <param name="userId">Optional user ID to filter issues by assigned staff (only applies for CONSTRUCTOR role)</param>
    /// <returns>Tuple containing the list of project issues and total count</returns>
    /// <remarks>
    /// <para>Returns project issues for a project with:</para>
    /// <list type="bullet">
    ///     <item><description>Basic issue information (ID, name, description, status, etc.)</description></item>
    ///     <item><description>Issue type information</description></item>
    ///     <item><description>Associated construction item information</description></item>
    ///     <item><description>User information who reported the issue</description></item>
    ///     <item><description>Issue images</description></item>
    /// </list>
    /// <para>Issues can be filtered by:</para>
    /// <list type="bullet">
    ///     <item><description>Search term (matches against name, description, solution, or reason)</description></item>
    ///     <item><description>Status</description></item>
    ///     <item><description>Issue type ID</description></item>
    ///     <item><description>Construction item ID</description></item>
    ///     <item><description>User ID (who reported the issue)</description></item>
    /// </list>
    /// <para>If the user with userId is a CONSTRUCTOR, only issues assigned to them are returned.
    /// For all other staff roles, all issues for the project are returned regardless of assignment.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="BadRequestException">Thrown when project is inactive</exception>
    Task<(IEnumerable<GetAllProjectIssueResponse> data, int total)> GetAllProjectIssueByProjectAsync(
        Guid id, 
        GetAllProjectIssueFilterRequest filter,
        Guid? userId = null);
        
    /// <summary>
    /// Gets all documents for a specific project with filtering and pagination
    /// </summary>
    /// <param name="filter">Filter criteria including search term, document types, pagination</param>
    /// <returns>Tuple containing collection of documents and total count</returns>
    /// <remarks>
    /// <para>This method returns all documents associated with a project filtered by the specified criteria.</para>
    /// <para>Documents can be filtered by:</para>
    /// <list type="bullet">
    ///     <item><description>Search term (matches against document name)</description></item>
    ///     <item><description>Document type IDs</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    Task<(IEnumerable<GetAllDocResponse> data, int total)> GetAllDocAsync(GetAllDocFilterRequest filter);
    
    /// <summary>
    /// Changes a project's status to FINISHED
    /// </summary>
    /// <param name="id">The ID of the project to finish</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>This method changes a project's status to FINISHED.</para>
    /// <para>A project can only be finished if it has reached the appropriate stage in the workflow.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="BadRequestException">Thrown when project is not in a state that can be finished</exception>
    Task FinishProjectAsync(Guid id);
}