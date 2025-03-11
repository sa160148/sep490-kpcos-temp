using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq.Expressions;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using System.Linq;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using LinqKit;
using System.Threading.Tasks;
using System.Collections.Generic;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using System.Linq.Dynamic.Core;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using Google.Cloud.Firestore;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{
    private string GetQuotationRequiredIncludes => 
        "Package,Customer.User,ProjectStaffs.Staff.User,Quotations,Contracts";
    private string GetDesignRequiredIncludes => 
        "Package,Customer.User,ProjectStaffs.Staff.User,Designs,Designs.DesignImages";

    public async Task<IEnumerable<ProjectForListResponse>> GetsAsync(
        PaginationFilter filter, 
        string? userId, 
        string role)
    {
        var filterOption = new GetAllProjectByRoleRequest();
        var repo = unitOfWork.Repository<Project>();
        var query = repo.Get(
            filter: filterOption.GetExpressionsV2(Guid.Parse(userId), role),
            orderBy: null,
            includeProperties: "Package,ProjectStaffs.Staff.User",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );

        var projectResponses = query.Select(project => mapper.Map<ProjectForListResponse>(project)).ToList();

        return projectResponses;
    }

    /// <summary>
    /// Gets all projects for a user with quotation-related information and standout status
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user</param>
    /// <returns>Collection of projects with quotation information and standout status</returns>
    public async Task<IEnumerable<GetAllProjectForQuotationResponse>> GetAllProjectForQuotationByUserIdAsync(
        GetAllProjectByUserIdRequest filter, 
        string? userId, 
        string? role = null)
    {
        ValidateRequest(filter);
        var projects = GetFilteredProjects(filter, "Quotation",userId, role);
        return MapProjectsForQuotationToResponse(projects, userId, role);
    }

    /// <summary>
    /// Validates the project request filter
    /// </summary>
    /// <param name="filter">The filter to validate</param>
    /// <exception cref="BadRequestException">Thrown when filter is null or contains invalid status</exception>
    private void ValidateRequest(GetAllProjectByUserIdRequest filter)
    {
        if (filter == null)
            throw new BadRequestException("Filter không được để trống");

        if (filter.Status != null && !filter.Status.All(IsValidProjectStatus))
            throw new BadRequestException("Trạng thái dự án không hợp lệ");
    }

    private bool IsValidProjectStatus(string status)
    {
        return Enum.TryParse<EnumProjectStatus>(status, true, out _);
    }

    /// <summary>
    /// Gets filtered projects with all required related entities
    /// </summary>
    /// <remarks>
    /// Includes related entities:
    /// - Package
    /// - Customer and User
    /// - ProjectStaffs with Staff and User
    /// - Quotations
    /// - Contracts
    /// </remarks>
    private IEnumerable<Project> GetFilteredProjects(
        GetAllProjectByUserIdRequest filter, 
        string? purpose,
        string? userId, 
        string? role)
    {
        var baseQuery = unitOfWork.Repository<Project>().Get(
            filter: BuildProjectFilter(filter, userId, role),
            includeProperties: purpose switch
            {
                "Quotation" => GetQuotationRequiredIncludes,
                "Design" => GetDesignRequiredIncludes,
                _ => "Package,ProjectStaffs.Staff.User"
            },
            orderBy: null,
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );

        return baseQuery;
    }

    /// <summary>
    /// Builds the filter expression for projects based on user role and status
    /// </summary>
    /// <remarks>
    /// <para>Filtering logic:</para>
    /// <list type="bullet">
    ///     <item><description>Applies status filter if provided</description></item>
    ///     <item><description>Administrators can see all projects</description></item>
    ///     <item><description>Other users see only their projects or assigned projects</description></item>
    /// </list>
    /// </remarks>
    private Expression<Func<Project, bool>> BuildProjectFilter(
        GetAllProjectByUserIdRequest filter, 
        string? userId, 
        string? role)
    {
        var predicate = PredicateBuilder.New<Project>(true);

        // Add status filter if provided
        if (filter.Status?.Any() == true)
        {
            predicate = predicate.And(p => filter.Status.Contains(p.Status));
        }

        // Add role-based filter
        if (role == RoleEnum.ADMINISTRATOR.ToString())
        {
            return predicate; // Admin can see all projects
        }

        var parsedUserId = Guid.Parse(userId ?? throw new BadRequestException("UserId is required"));
        
        // User can see their own projects or projects they're assigned to
        predicate = predicate.And(p => 
            p.Customer.UserId == parsedUserId || 
            p.ProjectStaffs.Any(ps => ps.Staff.UserId == parsedUserId));

        return predicate;
    }

    private IEnumerable<GetAllProjectForQuotationResponse> MapProjectsForQuotationToResponse(
        IEnumerable<Project> projects,
        string userId,
        string? role)
    {
        return projects.Select(project => MapProjectsForQuotationToResponse(project, userId, role));
    }

    private GetAllProjectForQuotationResponse MapProjectsForQuotationToResponse(
        Project project, 
        string userId, 
        string? role)
    {
        var response = mapper.Map<GetAllProjectForQuotationResponse>(project);
        var userRoles = GetUserRolesInProject(project, userId);
        response.StandOut = DetermineStandOutFlagForQuotation(project, role, userRoles);
        
        // Manually map staff list
        response.Staffs = project.ProjectStaffs
            .Select(ps => mapper.Map<GetAllStaffForDesignResponse>(ps.Staff))
            .ToList();
            
        return response;
    }

    private UserProjectRoles GetUserRolesInProject(
        Project project, 
        string userId)
    {
        var parsedUserId = Guid.Parse(userId);
        var staffProject = project.ProjectStaffs
            .FirstOrDefault(ps => ps.Staff.UserId == parsedUserId);

        return new UserProjectRoles
        {
            IsStaff = staffProject != null,
            IsCustomer = project.Customer.UserId == parsedUserId,
            StaffRole = staffProject?.Staff.Position.ToUpper()
        };
    }

    /// <summary>
    /// Determines if a project should be marked as standing out based on user role and project status
    /// </summary>
    /// <remarks>
    /// <para>Standout rules by role:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Administrator: Projects with open quotations or approved quotations without active contracts</description>
    ///     </item>
    ///     <item>
    ///         <description>Consultant: Projects with no quotations or with updating/rejected quotations</description>
    ///     </item>
    ///     <item>
    ///         <description>Customer: Projects with processing contracts or preview quotations without approved/updating status</description>
    ///     </item>
    /// </list>
    /// </remarks>
    private bool DetermineStandOutFlagForQuotation(
        Project project, 
        string? userRole, 
        UserProjectRoles roles)
    {
        if (IsAdministratorRole(userRole, roles.StaffRole))
            return CheckAdministratorStandOutForQuotation(project);

        if (roles.StaffRole == RoleEnum.CONSULTANT.ToString())
            return CheckConsultantStandOut(project);

        if (roles.IsCustomer)
            return CheckCustomerStandOutForQuotation(project);

        return false;
    }

    private bool IsAdministratorRole(
        string? userRole, 
        string? staffRole) =>
        userRole == RoleEnum.ADMINISTRATOR.ToString() || 
        staffRole == RoleEnum.ADMINISTRATOR.ToString();

    private bool CheckAdministratorStandOutForQuotation(Project project)
    {
        var hasOpenQuotation = project.Quotations
            .Any(q => q.Status == EnumQuotationStatus.OPEN.ToString());

        var hasApprovedWithoutActiveContract = project.Quotations
            .Any(q => q.Status == EnumQuotationStatus.APPROVED.ToString() 
                      && !HasActiveOrProcessingContract(project, q.Id));

        return hasOpenQuotation || hasApprovedWithoutActiveContract;
    }

    private bool HasActiveOrProcessingContract(
        Project project, 
        Guid quotationId) =>
        project.Contracts.Any(c => c.QuotationId == quotationId 
                                  && (c.Status == EnumContractStatus.PROCESSING.ToString() ||
                                      c.Status == EnumContractStatus.ACTIVE.ToString()));

    private bool CheckConsultantStandOut(Project project)
    {
        if (!project.Quotations.Any())
            return true;

        return project.Quotations.Any(q => 
            q.Status == EnumQuotationStatus.UPDATING.ToString() || 
            q.Status == EnumQuotationStatus.REJECTED.ToString());
    }

    private bool CheckCustomerStandOutForQuotation(Project project)
    {
        if (project.Contracts.Any(c => c.Status == EnumContractStatus.PROCESSING.ToString()))
            return true;

        return project.Quotations.Any(q => 
            q.Status == EnumQuotationStatus.PREVIEW.ToString() 
            && !HasApprovedOrUpdatingQuotation(project));
    }

    private bool HasApprovedOrUpdatingQuotation(Project project) =>
        project.Quotations.Any(q => 
            q.Status == EnumQuotationStatus.APPROVED.ToString() || 
            q.Status == EnumQuotationStatus.UPDATING.ToString());

    /// <summary>
    /// Represents a user's roles and relationships to a project
    /// </summary>
    private class UserProjectRoles
    {
        /// <summary>
        /// Whether the user is assigned to the project as staff
        /// </summary>
        public bool IsStaff { get; set; }

        /// <summary>
        /// Whether the user is the customer for the project
        /// </summary>
        public bool IsCustomer { get; set; }

        /// <summary>
        /// The user's staff role in the project, if any
        /// </summary>
        public string? StaffRole { get; set; }
    }

    public async Task<ProjectResponse> GetAsync(Guid id)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        var project = await projectRepo.Get()
            .Include(prj => prj.Customer)
            .ThenInclude(cst => cst.User)
            .Include(prj => prj.Package)
            .ThenInclude(pack => pack.PackageDetails)
            .ThenInclude(pd => pd.PackageItem)
            .Include(prj => prj.ProjectStaffs)
            .ThenInclude(ps => ps.Staff)
            .ThenInclude(staff => staff.User)
            .SingleOrDefaultAsync(prj => prj.Id == id);
        
        if (project is null)
        {
            throw new BadRequestException("Project không tồn tại");
        }

        var projectResult = mapper.Map<ProjectResponse>(project);
        projectResult.Staff = project.ProjectStaffs
            .Select(ps => mapper.Map<StaffResponse>(ps.Staff))
            .ToList();

        return projectResult;
    }

    public async Task<int> CountAsync()
    {
        return await unitOfWork.Repository<Project>().Get().CountAsync();
    }

    public int CountProjectByUserIdAsync(Guid userId)
    {
        var repo = unitOfWork.Repository<Project>();
        return repo.Get(filter: p => p.Customer.UserId == userId).Count();
    }

    public async Task CreateAsync(
        ProjectRequest request, 
        Guid userId)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        Project? project = mapper.Map<Project>(request);    
        var customer = await unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(customer => customer.UserId == userId);

        project.CustomerId = customer.Id;
        project.IsActive = true;
        project.Status = EnumProjectStatus.REQUESTING.ToString();
        await projectRepo.AddAsync(project);
    }

    public Task<IEnumerable<StaffResponse>> GetsConsultantAsync(
        PaginationFilter filter, 
        Guid projectId)
    {
        throw new NotImplementedException();
    }

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
    public async Task AssignStaffAsync(
        Guid projectId, 
        Guid userId)
    {
        var project = await ValidateAndGetProject(projectId);
        var staff = await ValidateAndGetStaff(userId);
        
        await ValidateProjectStatusForStaffAssignment(project, staff);
        await ValidateUniquePositionInProject(projectId, userId);
        await IsNotInAnyProject(userId);

        await AssignStaffAndUpdateStatus(project, staff);
    }

    public int CountQuotationByProject(Guid id)
    {
        return unitOfWork.Repository<Quotation>()
            .Get(filter: q => q.ProjectId == id)
            .Count();
    }

    public async Task<(IEnumerable<QuotationForProjectResponse> data, int total)> GetQuotationsByProjectAsync(
        Guid id, 
        GetAllQuotationFilterRequest filter)
    {
        // First validate project exists
        var project = await IsExistById(id);
        if (!project.IsActive == true)
        {
            throw new BadRequestException("Không tìm thấy Project");
        }
        
        var builder = filter.GetExpressions();
        builder = builder.And(q => q.ProjectId == id);

        // Get data with all conditions using repository's features
        var query = unitOfWork.Repository<Quotation>()
            .Get(
                filter: builder,
                orderBy: filter.GetOrder(),
                includeProperties: "QuotationDetails.Service,QuotationEquipments.Equipment,Project.ProjectStaffs.Staff.User",
                pageIndex: filter.PageNumber,
                pageSize: filter.PageSize
            ).ToList();

        var quotations = mapper.Map<List<QuotationForProjectResponse>>(query);

        return (quotations, quotations.Count);
    }

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
    public async Task<IEnumerable<GetAllProjectForDesignResponse>> GetAllProjectForDesignByUserIdAsync(
        GetAllProjectByUserIdRequest advandcedFilter, 
        string userId,
        string? role = null)
    {
        ValidateRequest(advandcedFilter);
        var projects = GetFilteredProjects(advandcedFilter, "Design", userId, role);
        return projects.Select(project => MapProjectForDesignToResponse(project, userId, role));
    }

    /// <summary>
    /// Gets all contracts associated with a specific project with pagination
    /// </summary>
    /// <param name="id">The project ID to get contracts for</param>
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
    public async Task<(IEnumerable<GetAllContractResponse> data, int total)> GetContractByProjectAsync(
        Guid id, 
        GetAllContractFilterRequest filter)
    {
        // Validate project exists using existing method
        var project = await IsExistById(id);
        
        if (!project.IsActive == true)
        {
            throw new BadRequestException("Không tìm thấy Project");
        }

        var advancedFilter = filter.GetExpressions();
        advancedFilter.And(c => c.ProjectId == id);
        // Get contracts with validation
        var contracts = unitOfWork.Repository<Contract>()
            .Get(
                filter: c => c.ProjectId == id && c.IsActive == true,
                includeProperties: "Project.Quotations",
                orderBy: q => q.OrderByDescending(c => c.CreatedAt),
                pageIndex: filter.PageNumber,
                pageSize: filter.PageSize
            ).ToList();
        
        if (!contracts.Any())
        {
            return (Enumerable.Empty<GetAllContractResponse>(), 0);
        }

        var contractResponses = contracts.Select(contract =>
        {
            var response = mapper.Map<GetAllContractResponse>(contract);
            
            // Find the quotation associated with this contract and get its total price
            var quotation = contract.Project.Quotations
                .FirstOrDefault(q => q.Id == contract.QuotationId);
            return response;
        }).ToList();

        return (contractResponses, contracts.Count());
    }

    public async Task<(IEnumerable<GetAllDesignResponse> data, int total)> GetAllDesignByProjectAsync(
        Guid id, 
        GetAllDesignFilterRequest filter)
    {
        // Validate project exists and is active
        await IsExistById(id);

        var repo = unitOfWork.Repository<Design>();
        
        // Combine project ID filter with other filter conditions
        var combinedFilter = filter.GetExpressions().And(d => d.ProjectId == id);

        // Get data with all conditions applied
        var (designs, total) = repo.GetWithCount(
            filter: combinedFilter,
            includeProperties: "DesignImages,Project.ProjectStaffs.Staff.User",
            orderBy: filter.GetOrder(),
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );

        var designResponses = mapper.Map<List<GetAllDesignResponse>>(designs);
        return (designResponses, total);
    }

    /// <summary>
    /// Checks if a project has any approved quotations
    /// </summary>
    /// <param name="id">The project ID to check</param>
    /// <returns>Response indicating whether the project has any approved quotations</returns>
    public async Task<IsQuotationApprovedByProjectResponse> IsQuotationApprovedByProjectAsync(Guid id)
    {
        var expression = PredicateBuilder.New<Quotation>();
        expression = expression.And(quotation => quotation.Status == EnumQuotationStatus.APPROVED.ToString());
        expression = expression.And(quotation => quotation.ProjectId == id);
        
        // Check if any quotations match the criteria
        var exists = unitOfWork.Repository<Quotation>()
            .Get(filter: expression)
            .Any();
            
        return new IsQuotationApprovedByProjectResponse
        {
            IsExitApproved = exists
        };
    }

    /// <summary>
    /// Maps a project entity to a design response with standout status and latest design image
    /// </summary>
    /// <param name="project">The project entity to map</param>
    /// <param name="userId">The ID of the user requesting the project</param>
    /// <param name="role">The role of the user</param>
    /// <returns>Project response with design information and standout status</returns>
    private GetAllProjectForDesignResponse MapProjectForDesignToResponse(
        Project project, 
        string userId, 
        string? role)
    {
        var response = mapper.Map<GetAllProjectForDesignResponse>(project);
        var userRoles = GetUserRolesInProject(project, userId);
        response.StandOut = DetermineStandOutFlagForDesign(project, role, userRoles);
        
        // Set design URL if exists
        var latestDesign = project.Designs
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();
        if (latestDesign?.DesignImages?.Any() == true)
        {
            response.ImageUrl = latestDesign.DesignImages.First().ImageUrl;
        }
        
        // Map project staff
        if (project.ProjectStaffs != null)
        {
            response.Staffs = project.ProjectStaffs
                .Select(ps => new GetAllStaffForDesignResponse
                {
                    Id = ps.Staff.UserId,
                    FullName = ps.Staff.User.FullName,
                    Email = ps.Staff.User.Email,
                    Position = ps.Staff.Position,
                    Avatar = ps.Staff.User.Avatar ?? ""
                })
                .ToList();
        }
        
        return response;
    }

    /// <summary>
    /// Determines if a project should be marked as standing out based on user role and design status
    /// </summary>
    /// <param name="project">The project to check</param>
    /// <param name="userRole">The user's system role</param>
    /// <param name="roles">The user's roles specific to this project</param>
    /// <returns>True if the project should stand out, false otherwise</returns>
    /// <remarks>
    /// <para>Standout rules by role:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Administrator: True when project has no manager assigned</description>
    ///     </item>
    ///     <item>
    ///         <description>Manager: True when project has no designer OR has designs in OPENING status</description>
    ///     </item>
    ///     <item>
    ///         <description>Designer: True when project has no designs, or has rejected/edit status designs</description>
    ///     </item>
    ///     <item>
    ///         <description>Customer: True when any design has preview status</description>
    ///     </item>
    /// </list>
    /// </remarks>
    private bool DetermineStandOutFlagForDesign(
        Project project, 
        string? userRole, 
        UserProjectRoles roles)
    {
        if (IsAdministratorRole(userRole, roles.StaffRole))
            return CheckAdministratorStandOutForDesign(project);

        if (roles.StaffRole == RoleEnum.MANAGER.ToString())
            return CheckManagerStandOutForDesign(project);

        if (roles.StaffRole == RoleEnum.DESIGNER.ToString())
            return CheckDesignerStandOut(project);

        if (roles.IsCustomer)
            return CheckCustomerStandOutForDesign(project);

        return false;
    }

    private bool CheckAdministratorStandOutForDesign(Project project)
    {
        return !project.ProjectStaffs
            .Any(ps => ps.Staff.Position == RoleEnum.MANAGER.ToString());
    }

    private bool CheckManagerStandOutForDesign(Project project)
    {
        var hasDesigner = project.ProjectStaffs
            .Any(ps => ps.Staff.Position == RoleEnum.DESIGNER.ToString());
        
        var hasOpenDesign = project.Designs
            .Any(d => d.Status == EnumDesignStatus.OPENING.ToString());

        return !hasDesigner || hasOpenDesign;
    }

    private bool CheckDesignerStandOut(Project project)
    {
        if (!project.Designs.Any())
            return true;

        return project.Designs.Any(d => 
            d.Status == EnumDesignStatus.REJECTED.ToString() || 
            d.Status == EnumDesignStatus.EDITING.ToString());
    }

    private bool CheckCustomerStandOutForDesign(Project project)
    {
        return project.Designs.Any(d => d.Status == EnumDesignStatus.PREVIEWING.ToString());
    }

    private async Task<Project> ValidateAndGetProject(Guid projectId)
    {
        var project = await unitOfWork.Repository<Project>().FindAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException("Project không tồn tại");
        }
        return project;
    }

    private async Task<Staff> ValidateAndGetStaff(Guid userId)
    {
        var staff = unitOfWork.Repository<Staff>()
            .Get(filter: s => s.UserId == userId, includeProperties: "User")
            .SingleOrDefault();

        if (staff == null)
        {
            throw new NotFoundException("Staff không tồn tại");
        }
        return staff;
    }

    private Task ValidateProjectStatusForStaffAssignment(
        Project project, 
        Staff staff)
    {
        var allowedAssignments = new Dictionary<string, (string RequiredStatus, string? NewStatus)>
        {
            [RoleEnum.CONSULTANT.ToString()] = (
                EnumProjectStatus.REQUESTING.ToString(),
                EnumProjectStatus.PROCESSING.ToString()
            ),
            [RoleEnum.MANAGER.ToString()] = (
                EnumProjectStatus.DESIGNING.ToString(),
                null // No status change for manager
            ),
            [RoleEnum.DESIGNER.ToString()] = (
                EnumProjectStatus.DESIGNING.ToString(),
                null // No status change for designer
            ),
            [RoleEnum.CONSTRUCTOR.ToString()] = (
                EnumProjectStatus.CONSTRUCTING.ToString(),
                null // No status change for constructor
            )
        };

        var position = staff.Position.ToUpper();
        if (!allowedAssignments.TryGetValue(position, out var statusInfo))
        {
            throw new BadRequestException($"Vị trí {position} không thể được phân công vào dự án");
        }

        if (project.Status != statusInfo.RequiredStatus)
        {
            var statusMessages = new Dictionary<string, string>
            {
                [RoleEnum.CONSULTANT.ToString()] = "đang yêu cầu",
                [RoleEnum.MANAGER.ToString()] = "đang xử lý",
                [RoleEnum.DESIGNER.ToString()] = "đang xử lý",
                [RoleEnum.CONSTRUCTOR.ToString()] = "đang thiết kế"
            };

            throw new BadRequestException(
                $"Chỉ có thể phân công {position} cho dự án {statusMessages[position]}");
        }

        return Task.CompletedTask;
    }

    private async Task AssignStaffAndUpdateStatus(
        Project project, 
        Staff staff)
    {
        var projectStaff = new ProjectStaff
        {
            ProjectId = project.Id,
            StaffId = staff.Id
        };

        // Only update status for Consultant assignments
        if (staff.Position.ToUpper() == RoleEnum.CONSULTANT.ToString())
        {
            project.Status = EnumProjectStatus.PROCESSING.ToString();
        }

        await unitOfWork.Repository<ProjectStaff>().AddAsync(projectStaff, false);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var repo = unitOfWork.Repository<Project>();
        var project = await IsExistById(id);

        project.IsActive = false;

        await repo.UpdateAsync(project, false);
        return await unitOfWork.SaveManualChangesAsync() > 0;
    }

    private async Task<Project> IsExistById(Guid id)
    {
        var project = await unitOfWork.Repository<Project>().FindAsync(id);
        if (project == null)
        {
            throw new NotFoundException();
        }
        return project;
    }

    /// <summary>
    /// Validate the staff with userId is available to assign to the project.
    /// <para>With validation rules for each position as: </para>
    /// <para>CONSULTANT: only for project when other projects have any status but not PROCESSING</para>
    /// <para>DESIGNER: only for project when other projects have any status but not DESIGNING</para>
    /// <para>CONSTRUCTOR: only for project when other projectss have any status but not CONSTRUCTING</para>
    /// <para>MANAGER: possible when other projects that have status FINISHED, any have status is not FINISHED can not assign</para>
    /// <para>ADMINISTRATOR: can not be assigned</para>
    /// </summary>
    /// <param name="staffId">staff will be assigned into project, but this is UserId in table staff</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="BadRequestException"></exception>
    private async Task<bool> IsNotInAnyProject(Guid userId)
    {
        var staffQuery = unitOfWork.Repository<Staff>().Get()
            .Include(s => s.User)
            .Include(s => s.ProjectStaffs)
                .ThenInclude(ps => ps.Project)
            .Where(s => s.UserId == userId);

        var staff = await staffQuery.SingleOrDefaultAsync() 
            ?? throw new NotFoundException("Staff không tồn tại");

        var position = staff.Position.ToUpper();

        // Administrator cannot be assigned to any project
        if (position == RoleEnum.ADMINISTRATOR.ToString())
        {
            throw new BadRequestException("Administrator không thể được phân công vào dự án");
        }

        // Special validation for Manager - all other projects must be FINISHED
        if (position == RoleEnum.MANAGER.ToString())
        {
            var hasUnfinishedProjects = staff.ProjectStaffs
                .Any(ps => ps.Project.IsActive == true && 
                          ps.Project.Status != EnumProjectStatus.FINISHED.ToString());

            if (hasUnfinishedProjects)
            {
                throw new BadRequestException($"Manager {staff.User.Email} có dự án chưa hoàn thành");
            }
            
            return true;
        }

        // Check specific status restrictions for each role
        var statusRestrictions = new Dictionary<string, string>
        {
            [RoleEnum.CONSULTANT.ToString()] = EnumProjectStatus.PROCESSING.ToString(),
            [RoleEnum.DESIGNER.ToString()] = EnumProjectStatus.DESIGNING.ToString(),
            [RoleEnum.CONSTRUCTOR.ToString()] = EnumProjectStatus.CONSTRUCTING.ToString()
        };

        if (statusRestrictions.TryGetValue(position, out var restrictedStatus))
        {
            var hasRestrictedProject = staff.ProjectStaffs
                .Any(ps => ps.Project.IsActive == true && 
                          ps.Project.Status == restrictedStatus);

            if (hasRestrictedProject)
            {
                var errorMessages = new Dictionary<string, string>
                {
                    [RoleEnum.CONSULTANT.ToString()] = $"Consultant {staff.User.Email} đang có dự án đang xử lý",
                    [RoleEnum.DESIGNER.ToString()] = $"Designer {staff.User.Email} đang có dự án đang thiết kế",
                    [RoleEnum.CONSTRUCTOR.ToString()] = $"Constructor {staff.User.Email} đang có dự án đang thi công"
                };

                throw new BadRequestException(errorMessages[position]);
            }
        }

        return true;
    }

    /// <summary>
    /// Validates if staff with specific positions (Consultant, Designer, Manager) already exists in the project
    /// </summary>
    /// <param name="projectId">Project ID to check</param>
    /// <param name="staffId">Staff ID being assigned but this is actually is UserId in table staff</param>
    /// <exception cref="BadRequestException">Thrown when duplicate position found</exception>
    private async Task ValidateUniquePositionInProject(
        Guid projectId, 
        Guid userId)
    {
        var staff = unitOfWork.Repository<Staff>()
            .Get(filter: s => s.UserId == userId, includeProperties: "User")
            .SingleOrDefault();

        if (staff == null)
        {
            throw new NotFoundException("Staff không tồn tại");
        }

        // Only check unique positions for Consultant, Designer, and Manager
        var uniquePositions = new[] 
        { 
            RoleEnum.CONSULTANT.ToString(), 
            RoleEnum.DESIGNER.ToString(), 
            RoleEnum.MANAGER.ToString() 
        };

        // Skip validation for Constructor and other positions
        if (!uniquePositions.Contains(staff.Position.ToUpper()))
        {
            return;
        }

        var existingStaff = unitOfWork.Repository<ProjectStaff>()
            .Get(
                filter: ps => ps.ProjectId == projectId && 
                             ps.Staff.Position.ToUpper() == staff.Position.ToUpper(),
                includeProperties: "Staff.User"
            ).SingleOrDefault();

        if (existingStaff != null)
        {
            var positionMessages = new Dictionary<string, string>
            {
                [RoleEnum.CONSULTANT.ToString()] = "Consultant",
                [RoleEnum.DESIGNER.ToString()] = "Designer",
                [RoleEnum.MANAGER.ToString()] = "Manager"
            };

            throw new BadRequestException(
                $"Dự án đã có {positionMessages[staff.Position.ToUpper()]} {existingStaff.Staff.User.Email}");
        }
    }

    /// <summary>
    /// Checks if a project has any confirmed 3D designs
    /// </summary>
    /// <param name="id">The project ID to check</param>
    /// <returns>Response indicating whether the project has any confirmed 3D designs</returns>
    /// <remarks>
    /// <para>This method checks if there are any designs with type '3D' and status 'CONFIRMED' for the specified project</para>
    /// <para>Used to determine if a project can proceed to the next stage in the workflow</para>
    /// </remarks>
    public async Task<IsDesignExitByProjectResponse> IsDesign3DConfirmedAsync(Guid id)
    {
        var expression = PredicateBuilder.New<Design>();
        expression = expression.And(design => design.Status == EnumDesignStatus.CONFIRMED.ToString());
        expression = expression.And(design => design.ProjectId == id);
        expression = expression.And(design => design.Type == "3D");
        
        // Check if any designs match the criteria instead of trying to get a single one
        var exists = unitOfWork.Repository<Design>()
            .Get(filter: expression)
            .Any();
            
        return new IsDesignExitByProjectResponse
        {
            IsExit3DConfirmed = exists
        };
    }
}