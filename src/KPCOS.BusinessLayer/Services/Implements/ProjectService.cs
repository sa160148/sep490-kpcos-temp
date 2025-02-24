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

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{
    public async Task<IEnumerable<ProjectForListResponse>> GetsAsync(PaginationFilter filter, string? userId, string role)
    {
        var filterOption = new GetAllProjectByRoleRequest();
        var repo = unitOfWork.Repository<Project>();
        var query = repo.Get(
            filter: filterOption.GetExpressionsV2(Guid.Parse(userId), role),
            orderBy: null,
            includeProperties: "Package",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );

        var projectResponses = query.Select(project => mapper.Map<ProjectForListResponse>(project)).ToList();

        return projectResponses;
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

    public async Task CreateAsync(ProjectRequest request, Guid userId)
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

    public Task<IEnumerable<StaffResponse>> GetsConsultantAsync(PaginationFilter filter, Guid projectId)
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
    public async Task AssignStaffAsync(Guid projectId, Guid userId)
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

    public Task<IEnumerable<QuotationForProjectResponse>> GetQuotationsByProjectAsync(Guid id, PaginationFilter filter)
    {
        var query = unitOfWork.Repository<Quotation>()
            .Get(
                filter: q => q.ProjectId == id,
                includeProperties: "Project",
                orderBy: q => q.OrderByDescending(q => q.CreatedAt),
                pageIndex: filter.PageNumber,
                pageSize: filter.PageSize
            );
        
        var quotations = query.ToList();

        return Task.FromResult(quotations.Select(q => mapper.Map<QuotationForProjectResponse>(q)));
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

    private Task ValidateProjectStatusForStaffAssignment(Project project, Staff staff)
    {
        var allowedAssignments = new Dictionary<string, (string RequiredStatus, string NewStatus)>
        {
            [RoleEnum.CONSULTANT.ToString()] = (
                EnumProjectStatus.REQUESTING.ToString(),
                EnumProjectStatus.PROCESSING.ToString()
            ),
            [RoleEnum.DESIGNER.ToString()] = (
                EnumProjectStatus.PROCESSING.ToString(),
                EnumProjectStatus.DESIGNING.ToString()
            ),
            [RoleEnum.CONSTRUCTOR.ToString()] = (
                EnumProjectStatus.DESIGNING.ToString(),
                EnumProjectStatus.CONSTRUCTING.ToString()
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
                [RoleEnum.DESIGNER.ToString()] = "đang xử lý",
                [RoleEnum.CONSTRUCTOR.ToString()] = "đang thiết kế"
            };

            throw new BadRequestException(
                $"Chỉ có thể phân công {position} cho dự án {statusMessages[position]}");
        }

        return Task.CompletedTask;
    }

    private async Task AssignStaffAndUpdateStatus(Project project, Staff staff)
    {
        var projectStaff = new ProjectStaff
        {
            ProjectId = project.Id,
            StaffId = staff.Id
        };

        var statusUpdates = new Dictionary<string, string>
        {
            [RoleEnum.CONSULTANT.ToString()] = EnumProjectStatus.PROCESSING.ToString(),
            [RoleEnum.DESIGNER.ToString()] = EnumProjectStatus.DESIGNING.ToString(),
            [RoleEnum.CONSTRUCTOR.ToString()] = EnumProjectStatus.CONSTRUCTING.ToString()
        };

        project.Status = statusUpdates[staff.Position.ToUpper()];

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
    private async Task ValidateUniquePositionInProject(Guid projectId, Guid userId)
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
}