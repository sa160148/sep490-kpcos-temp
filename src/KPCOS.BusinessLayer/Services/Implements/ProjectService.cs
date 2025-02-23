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

    public async Task AssignConsultantAsync(Guid id, StaffAssignRequest request)
    {
        if (!(await IsNotInAnyProject(request.StaffId)))
        {
            throw new BadRequestException("Staff đang ở poject khác chưa hoàn thành");
        }
        var projectStaff = new ProjectStaff
        {
            ProjectId = id,
            StaffId = request.StaffId
        };

        var project = await unitOfWork.Repository<Project>().FindAsync(id);
        project.Status = EnumProjectStatus.PROCESSING.ToString();

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

    private async Task<bool> IsNotInAnyProject(Guid staffId)
    {
        var projectStaff = unitOfWork.Repository<ProjectStaff>()
            .Get(
                filter: ps => ps.StaffId == staffId &&
                              (ps.Project.Status == EnumProjectStatus.CONSTRUCTING.ToString() ||
                               ps.Project.Status == EnumProjectStatus.REQUESTING.ToString() ||
                               ps.Project.Status == EnumProjectStatus.DESIGNING.ToString() ||
                               ps.Project.Status == EnumProjectStatus.PROCESSING.ToString()),
                includeProperties: "Project"
            ).FirstOrDefault();

        if (projectStaff != null)
        {
            throw new NotFoundException();
        }
        return true;
    }
}