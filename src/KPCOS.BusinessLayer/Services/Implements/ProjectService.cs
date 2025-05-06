using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.Exceptions;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Google.Cloud.Firestore;
using KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.Common.Utilities;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectService(
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IEmailService emailService, 
    IFirebaseService firebaseService,
    IMaintenanceService maintenanceService) : IProjectService
{
    private string GetQuotationRequiredIncludes => 
        "Package,Customer.User,ProjectStaffs.Staff.User,Quotations,Contracts";
    private string GetDesignRequiredIncludes => 
        "Package,Customer.User,ProjectStaffs.Staff.User,Designs,Designs.DesignImages";

    public async Task<(IEnumerable<ProjectForListResponse> Data, int Count)> GetsAsync(
        GetAllProjectFilterRequest filter,
        Guid? userId = null,
        string? role = null)
    {
        var repo = unitOfWork.Repository<Project>();
        var query = repo.GetWithCount(
            filter: filter.GetExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "Package,ProjectStaffs.Staff.User,Designs.DesignImages",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        var projects = query.Data;

        var projectResponses = mapper.Map<IEnumerable<ProjectForListResponse>>(projects);

        return (projectResponses, query.Count);
    }

    /// <summary>
    /// Gets all projects for a user with quotation-related information and standout status
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user</param>
    /// <returns>Collection of projects with quotation information and standout status</returns>
    public async Task<(IEnumerable<GetAllProjectForQuotationResponse> Data, int Count)> GetAllProjectForQuotationByUserIdAsync(
        GetAllProjectByUserIdRequest filter, 
        string? userId, 
        string? role = null)
    {
        ValidateRequest(filter);
        
        // Get projects and count in a single query using GetWithCount
        var (projects, count) = unitOfWork.Repository<Project>().GetWithCount(
            filter: BuildProjectFilter(filter, userId, role),
            includeProperties: GetQuotationRequiredIncludes,
            orderBy: filter.GetOrder(),
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map the projects to response objects
        var mappedProjects = MapProjectsForQuotationToResponse(projects, userId, role);
        
        return (mappedProjects, count);
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
        var project = projectRepo.Get(
            filter: p => p.Id == id,
            includeProperties: "Customer.User,Package,Package.PackageDetails,Package.PackageDetails.PackageItem,ProjectStaffs.Staff,ProjectStaffs.Staff.User,Contracts"
        )
        .SingleOrDefault();



            // .Include(prj => prj.Customer)
            // .ThenInclude(cst => cst.User)
            // .Include(prj => prj.Package)
            // .ThenInclude(pack => pack.PackageDetails)
            // .ThenInclude(pd => pd.PackageItem)
            // .Include(prj => prj.ProjectStaffs)
            // .ThenInclude(ps => ps.Staff)
            // .ThenInclude(staff => staff.User)
            // .SingleOrDefaultAsync(prj => prj.Id == id)
            
        
        if (project is null)
        {
            throw new BadRequestException("Project không tồn tại");
        }

        var projectResult = mapper.Map<ProjectResponse>(project);
        projectResult.Staff = project.ProjectStaffs
            .Select(ps => mapper.Map<StaffResponse>(ps.Staff))
            .ToList();

        // Get and set contract value from active contract
        var activeContract = project.Contracts
            .FirstOrDefault(c => c.Status == EnumContractStatus.ACTIVE.ToString() && c.IsActive == true);
        
        if (activeContract != null)
        {
            projectResult.ContractValue = activeContract.ContractValue;
        }

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

    /// <summary>
    /// Counts projects with filters applied for accurate pagination
    /// </summary>
    /// <param name="filter">The filter criteria to apply</param>
    /// <param name="userId">The user ID</param>
    /// <param name="role">The user role</param>
    /// <returns>Count of projects matching the filter criteria</returns>
    public int CountProjectsWithFiltersAsync(GetAllProjectByUserIdRequest filter, string userId, string role)
    {
        var repo = unitOfWork.Repository<Project>();
        return repo.Get(filter: BuildProjectFilter(filter, userId, role)).Count();
    }

    /// <summary>
    /// Generates a project name based on area, depth, and customer name
    /// Adds a "Koi" prefix based on the project dimensions:
    /// - MiniKoi: Small projects (area ≤ 10m²)
    /// - KoiView: Medium-sized projects with decent depth (area ≤ 30m²)
    /// - AquaKoi: Small depth projects (depth ≤ 1m)
    /// - KoiFlow: Standard projects with good depth (area ≤ 100m² with depth > 1m)
    /// - KoiBay: Large projects (area > 100m²)
    /// - KoiDrop: Projects with extra deep construction (depth > 1.5m)
    /// - KoiBox: Projects with specific dimensions (area around 30-40m²)
    /// - KoiDeck: Standard projects (around 100m²)
    /// - KoiVilla: Medium projects (20-25m²)
    /// - KoiGarden: Projects with specific dimensions (15-20m²)
    /// Examples: "KoiFlow Bà Nga", "MiniKoi Bé Vy"
    /// </summary>
    /// <param name="area">The project area in square meters</param>
    /// <param name="depth">The project depth in meters</param>
    /// <param name="customerName">The customer name</param>
    /// <returns>A formatted project name</returns>
    private string GenerateProjectName(double area, double depth, string customerName)
    {
        string prefix;

        // Determine prefix based on area and depth
        if (area <= 10)
        {
            prefix = "MiniKoi";
        }
        else if (area <= 25 && depth <= 1)
        {
            prefix = "AquaKoi";
        }
        else if (area <= 30)
        {
            prefix = "KoiView";
        }
        else if (area > 100)
        {
            prefix = "KoiBay";
        }
        else if (area >= 15 && area <= 20)
        {
            prefix = "KoiGarden";
        }
        else if (area >= 20 && area <= 25)
        {
            prefix = "KoiVilla";
        }
        else if (area >= 30 && area <= 40)
        {
            prefix = "KoiBox";
        }
        else if (depth > 1.5 || (area > 9 && area.ToString().Contains("9")))
        {
            prefix = "KoiDrop";
        }
        else if (area == 100)
        {
            prefix = "KoiDeck";
        }
        else
        {
            prefix = "KoiFlow";
        }

        return $"{prefix} {customerName}";
    }

    public async Task CreateAsync(
        ProjectRequest request, 
        Guid userId)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        
        // Get customer with User information
        var customer = unitOfWork.Repository<Customer>()
            .Get(filter: c => c.UserId == userId, includeProperties: "User")
            .FirstOrDefault();

        if (customer == null)
        {
            throw new NotFoundException("Customer not found");
        }

        // Create project with request data
        Project? project = mapper.Map<Project>(request);

        // Fill in missing data from customer if needed
        project.CustomerName = request.CustomerName ?? customer.User.FullName;
        project.Email = request.Email ?? customer.User.Email;
        project.Phone = request.Phone ?? customer.User.Phone;
        project.Address = request.Address ?? customer.Address;
        
        // Generate project name based on area, depth and customer name
        project.Name = GenerateProjectName(project.Area, project.Depth, project.CustomerName);
        
        project.CustomerId = customer.Id;
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
    ///         <description>Consultant: Only one per project</description></item>
    ///     <item>
    ///         <description>Designer: Only one per project</description></item>
    ///     <item>
    ///         <description>Manager: Only one per project</description></item>
    ///     <item>
    ///         <description>Constructor: Multiple allowed per project</description></item>
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
            .GetWithCount(
                filter: builder,
                orderBy: filter.GetOrder(),
                includeProperties: "QuotationDetails.Service,QuotationEquipments.Equipment,Project.ProjectStaffs.Staff.User",
                pageIndex: filter.PageNumber,
                pageSize: filter.PageSize
            );

        var quotations = mapper.Map<List<QuotationForProjectResponse>>(query.Data);

        return (quotations, query.Count);
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
    public async Task<(IEnumerable<GetAllProjectForDesignResponse> Data, int Count)> GetAllProjectForDesignByUserIdAsync(
        GetAllProjectByUserIdRequest advandcedFilter, 
        string userId,
        string? role = null)
    {
        ValidateRequest(advandcedFilter);
        
        // Get projects and count in a single query using GetWithCount
        var (projects, count) = unitOfWork.Repository<Project>().GetWithCount(
            filter: BuildProjectFilter(advandcedFilter, userId, role),
            includeProperties: GetDesignRequiredIncludes,
            orderBy: null,
            pageIndex: advandcedFilter.PageNumber,
            pageSize: advandcedFilter.PageSize
        );
        
        // Map the projects to response objects
        var mappedProjects = projects.Select(project => MapProjectForDesignToResponse(project, userId, role));
        
        return (mappedProjects, count);
    }

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
    public async Task<(IEnumerable<GetAllContractResponse> data, int total)> GetContractByProjectAsync(
        GetAllContractFilterRequest filter)
    {
        // Validate project exists using existing method
        var project = await IsExistById(filter.ProjectId.Value);
        
        if (!project.IsActive == true)
        {
            throw new BadRequestException("Không tìm thấy Project");
        }

        // Get contracts with validation
        var contracts = unitOfWork.Repository<Contract>()
            .GetWithCount(
                filter: filter.GetExpressions(),
                includeProperties: "Project.Quotations",
                orderBy: filter.GetOrder(),
                pageIndex: filter.PageNumber,
                pageSize: filter.PageSize
            );

        var contractResponses = mapper.Map<List<GetAllContractResponse>>(contracts.Data);

        return (contractResponses, contracts.Count);
    }

    public async Task<(IEnumerable<GetAllDesignResponse> data, int total)> GetAllDesignByProjectAsync(
        Guid id, 
        GetAllDesignFilterRequest filter)
    {
        // Validate project exists and is active
        await IsExistById(id);

        var repo = unitOfWork.Repository<Design>();
        
        // Combine project ID filter with other filter conditions
        var combinedFilter = filter.GetExpressions();
        combinedFilter = combinedFilter.And(d => d.ProjectId == id);

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
    /// Checks if a project has any active contracts
    /// </summary>
    /// <param name="id">The project ID to check</param>
    /// <returns>Response indicating whether the project has any active contracts</returns>
    public async Task<IsContractApprovedByProjectResponse> IsContractApprovedByProjectAsync(Guid id)
    {
        var expression = PredicateBuilder.New<Contract>();
        expression = expression.And(contract => contract.Status == EnumContractStatus.ACTIVE.ToString());
        expression = expression.And(contract => contract.ProjectId == id);
        
        // Check if any contracts match the criteria
        var exists = unitOfWork.Repository<Contract>()
            .Get(filter: expression)
            .Any();
            
        return new IsContractApprovedByProjectResponse
        {
            IsExitActive = exists
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

    /// <summary>
    /// Validates if the staff position matches the required project status for assignment
    /// </summary>
    /// <param name="project">The project to assign staff to</param>
    /// <param name="staff">The staff member being assigned</param>
    /// <returns>Task indicating completion</returns>
    /// <remarks>
    /// <para>Each staff position can only be assigned to a project in a specific status:</para>
    /// <list type="bullet">
    ///     <item><description>Consultant: Only for REQUESTING projects, changes status to PROCESSING</description></item>
    ///     <item><description>Designer: Only for DESIGNING projects</description></item>
    ///     <item><description>Manager: Only for DESIGNING projects</description></item>
    ///     <item><description>Constructor: Only for CONSTRUCTING projects</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="BadRequestException">Thrown when position cannot be assigned to project or status mismatch</exception>
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
    /// <para>CONSULTANT: can be assigned to multiple projects simultaneously</para>
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
        var staffQuery = unitOfWork.Repository<Staff>()
            .Get(
                filter: s => s.UserId == userId, 
                includeProperties: "User,ProjectStaffs.Project")
            .SingleOrDefault();

        var staff = staffQuery 
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
            // Removing CONSULTANT restriction as per new requirements
            // [RoleEnum.CONSULTANT.ToString()] = EnumProjectStatus.PROCESSING.ToString(),
            // Consultants can now be assigned to multiple projects with PROCESSING status
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
    /// <param name="userId">Staff ID being assigned (actually the UserId in the Staff table)</param>
    /// <remarks>
    /// <para>Enforces the following uniqueness rules:</para>
    /// <list type="bullet">
    ///     <item><description>Consultant: Only one allowed per project</description></item>
    ///     <item><description>Designer: Only one allowed per project</description></item>
    ///     <item><description>Manager: Only one allowed per project</description></item>
    /// </list>
    /// <para>Constructor and other positions are not checked for uniqueness (multiple allowed)</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when staff not found</exception>
    /// <exception cref="BadRequestException">Thrown when duplicate position found in project</exception>
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
        expression = expression.And(
            design => design.Status == EnumDesignStatus.CONFIRMED.ToString() &&
            design.ProjectId == id &&
            design.Type == "3D"
        );
        
        // Check if any designs match the criteria instead of trying to get a single one
        var exists = unitOfWork.Repository<Design>()
            .Get(filter: expression)
            .Any();
            
        return new IsDesignExitByProjectResponse
        {
            IsExit3DConfirmed = exists
        };
    }

    public async Task<(IEnumerable<GetAllStaffResponse> data, int total)> GetAllStaffByProjectAsync(
        Guid id, 
        GetAllStaffRequest filter)
    {
        // Validate project exists
        await ValidateAndGetProject(id);
        
        // Create a predicate for filtering ProjectStaff by project ID
        var predicate = PredicateBuilder.New<ProjectStaff>(true);
        predicate = predicate.And(ps => ps.ProjectId == id);
        
        // Apply position filter if specified
        if (!string.IsNullOrEmpty(filter.Position))
        {
            predicate = predicate.And(ps => ps.Staff.Position == filter.Position);
        }
        
        // Apply idle filter for constructors if specified
        if (filter.IsIdle.HasValue && filter.IsIdle.Value && 
            (string.IsNullOrEmpty(filter.Position) || filter.Position == RoleEnum.CONSTRUCTOR.ToString()))
        {
            // For constructors, idle means either:
            // 1. Not assigned to any construction task, OR
            // 2. Only assigned to tasks with DONE status
            predicate = predicate.And(ps => 
                ps.Staff.Position == RoleEnum.CONSTRUCTOR.ToString() && 
                (
                    /*
                    // Not assigned to any construction task for this project
                    !ps.Staff.ConstructionTasks.Any(ct => ct.ConstructionItem.ProjectId == id) ||
                    */
                    
                    // OR all assigned tasks for this project are in DONE status
                    ps.Staff.ConstructionTasks
                        .Where(ct => ct.ConstructionItem.ProjectId == id)
                        .All(ct => ct.Status == EnumConstructionTaskStatus.DONE.ToString())
                )
            );
        }
        
        // Get data with count using the repository's GetWithCount method
        var result = unitOfWork.Repository<ProjectStaff>().GetWithCount(
            filter: predicate,
            includeProperties: "Staff.User,Staff.ConstructionTasks.ConstructionItem",
            orderBy: null,
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map the staff entities to response DTOs using AutoMapper
        var staffResponses = result.Data
            .Select(ps => mapper.Map<GetAllStaffResponse>(ps.Staff))
            .ToList();
        
        return (staffResponses, result.Count);
    }

    /// <summary>
    /// [DEPRECATED] Gets all construction tasks associated with a specific project with pagination and filtering
    /// </summary>
    /// <param name="filter">Filter criteria for construction tasks</param>
    /// <returns>Tuple containing the list of construction tasks and total count</returns>
    /// <remarks>
    /// <para>This method is deprecated and will be removed in a future version.</para>
    /// <para>Use the GetAllConstructionTaskAsync method in the ConstructionService instead.</para>
    /// </remarks>
    public async Task<(IEnumerable<GetAllConstructionTaskResponse> data, int total)> GetAllConstructionTaskByProjectAsync(
        GetAllConstructionTaskFilterRequest filter)
    {
        // Validate project exists
        await ValidateAndGetProject(filter.ProjectId.Value);
        
        // Execute the query with all filters, includes, and pagination
        var result = unitOfWork.Repository<ConstructionTask>().GetWithCount(
            filter: filter.GetExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "Staff,Staff.User,ConstructionItem",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map to response DTOs
        var response = mapper.Map<List<GetAllConstructionTaskResponse>>(result.Data);
        
        return (response, result.Count);
    }

    /// <summary>
    /// Gets all project issues associated with a specific project with pagination and filtering
    /// </summary>
    /// <param name="id">The project ID to get issues for</param>
    /// <param name="filter">Filter criteria for project issues including search, status, issue type, etc.</param>
    /// <param name="userId">Optional user ID to filter issues by assigned staff (for constructor role)</param>
    /// <returns>Tuple containing the list of project issues and total count</returns>
    public async Task<(IEnumerable<GetAllProjectIssueResponse> data, int total)> GetAllProjectIssueByProjectAsync(
        Guid id, 
        GetAllProjectIssueFilterRequest filter,
        Guid? userId = null)
    {
        // Validate and get project
        var project = await ValidateAndGetProject(id);
        
        // Get required includes for ProjectIssue
        var includeProperties = "IssueType,ConstructionItem,Staff,Staff.User";
        
        // Get construction items related to the project
        var constructionItems = unitOfWork.Repository<ConstructionItem>().Get(
            filter: ci => ci.ProjectId == id && ci.IsActive == true
        ).ToList();
        
        if (!constructionItems.Any())
        {
            return (Enumerable.Empty<GetAllProjectIssueResponse>(), 0);
        }
        
        // Get the construction item IDs
        var constructionItemIds = constructionItems.Select(ci => ci.Id).ToList();

        // Build base filter expression for construction item IDs
        var baseExpression = PredicateBuilder.New<ProjectIssue>(true);
        baseExpression = baseExpression.And(pi => constructionItemIds.Contains(pi.ConstructionItemId));
        
        // If userId is provided, check if user is a constructor
        if (userId.HasValue)
        {
            // Find the staff with the given userId
            var staff = await unitOfWork.Repository<Staff>().FirstOrDefaultAsync(s => s.UserId == userId.Value);
            
            // Only filter by staff ID if the user is a CONSTRUCTOR
            if (staff != null && staff.Position == RoleEnum.CONSTRUCTOR.ToString())
            {
                // Add filter to only include issues assigned to this staff
                baseExpression = baseExpression.And(issue => issue.StaffId == staff.Id);
            }
        }
        
        // Combine with the filter's expression
        var filterExpression = filter.GetExpressions();
        var combinedExpression = baseExpression.And(filterExpression);
        
        // Get project issues with filtering and pagination
        var projectIssuesResult = unitOfWork.Repository<ProjectIssue>().GetWithCount(
            filter: combinedExpression,
            includeProperties: includeProperties,
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map to response DTOs using AutoMapper
        var mappedResponse = mapper.Map<List<GetAllProjectIssueResponse>>(projectIssuesResult.Data);

        return (mappedResponse, projectIssuesResult.Count);
    }

    private async Task<User?> GetUserByUserId(Guid userId)
    {
        var user = unitOfWork.Repository<User>()
            .Get(
                filter: u => u.Id == userId,
                includeProperties: "Staff,Customer"
                )
            .SingleOrDefault();
        ;
        return user;
    }

    /// <summary>
    /// Gets all documents for a project with filtering
    /// </summary>
    /// <param name="projectId">Project ID to get documents for</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered documents and total count</returns>
    public async Task<(IEnumerable<GetAllDocResponse> data, int total)> GetAllDocAsync(GetAllDocFilterRequest filter)
    {
        // Validate project exists
        await ValidateAndGetProject(filter.ProjectId.Value);
        
        // Get the documents with DocType included
        var (documents, count) = unitOfWork.Repository<Doc>().GetWithCount(
            filter: filter.GetExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "DocType",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map the documents to DTOs
        var result = mapper.Map<IEnumerable<GetAllDocResponse>>(documents);
        
        return (result, count);
    }
    /// <summary>
    /// Changes a project's status to FINISHED
    /// </summary>
    /// <param name="id">The ID of the project to finish</param>
    /// <returns>Task representing the operation</returns>
    public async Task FinishProjectAsync(
        Guid id,
        CommandMaintenanceRequest maintenanceOptionalRequest)
    {
        // Get the project and validate it exists
        var projectRepo = unitOfWork.Repository<Project>();
        var project = projectRepo.Get(
                p => p.Id == id,
                includeProperties: "ConstructionItems,Docs,Customer"
                )
            .SingleOrDefault()
            ;
        if (project == null)
        {
            throw new NotFoundException("Dự án không tồn tại");
        }
        
        // Check if project is in CONSTRUCTING status
        if (project.Status != EnumProjectStatus.CONSTRUCTING.ToString())
        {
            throw new BadRequestException($"Dự án phải ở trạng thái CONSTRUCTING để hoàn thành, trạng thái hiện tại: {project.Status}");
        }
        
        // Check if all parent construction items (no parent ID) are DONE
        var parentConstructionItems = project.ConstructionItems
            .Where(ci => ci.ParentId == null && ci.IsActive == true)
            .ToList();
        
        if (!parentConstructionItems.Any())
        {
            throw new BadRequestException("Dự án không có hạng mục xây dựng");
        }
        
        // Directly check if all parent construction items are in DONE status
        if (!parentConstructionItems.All(ci => ci.Status == EnumConstructionItemStatus.DONE.ToString()))
        {
            throw new BadRequestException("Tất cả hạng mục xây dựng phải ở trạng thái hoàn thành");
        }
        
        // Check if project has at least one active document
        var activeDoc = project.Docs
            .Where(d => d.IsActive == true && d.Status == EnumDocStatus.ACTIVE.ToString())
            .FirstOrDefault();
        
        if (activeDoc == null)
        {
            throw new BadRequestException("Dự án phải có ít nhất một tài liệu với trạng thái ACTIVE");
        }
        
        // Update project status to FINISHED
        project.Status = EnumProjectStatus.FINISHED.ToString();

        // Auto create maintenance request with hardcoded values when maintenance package ID is provided, no validate maintenance package ID
        if (maintenanceOptionalRequest.MaintenancePackageId.HasValue)
        {
            // Get current time in SEA timezone and add 1 day
            var currentTime = GlobalUtility.GetCurrentSEATime();
            var estimateAt = currentTime.AddDays(1);
            
            maintenanceOptionalRequest.Name = "Bảo dưỡng/bảo trì " + project.Name;
            maintenanceOptionalRequest.Address = project.Address;
            maintenanceOptionalRequest.Area = project.Area;
            maintenanceOptionalRequest.Depth = project.Depth;
            maintenanceOptionalRequest.Duration = 3;
            maintenanceOptionalRequest.TotalValue = 0;
            maintenanceOptionalRequest.Type = EnumMaintenanceRequestType.SCHEDULED.ToString();
            maintenanceOptionalRequest.EstimateAt = DateOnly.FromDateTime(estimateAt);
            
            // Create maintenance request and save to database
            await maintenanceService.CreateMaintenanceRequestAsync(maintenanceOptionalRequest, project.Customer.UserId);
        }
        
        // Save changes of project to database
        await projectRepo.UpdateAsync(project);
    }
}