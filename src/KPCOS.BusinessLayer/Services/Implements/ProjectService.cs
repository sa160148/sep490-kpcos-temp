using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{

    #region Feat: Base Funtion

    public async Task<IEnumerable<ProjectResponse>> GetsAsync()
    {
        /*
        var projectRepo = unitOfWork.Repository<Project>();
        var projects = projectRepo.Get()
            .Include(prokect => prokect.Package)
            .ThenInclude(package => package.PackageDetails)
            .Include(project => project.Customer)
            .ThenInclude(customer => customer.User)
            .Include(project => project.ProjectStaffs)
            .ThenInclude(projectStaff => projectStaff.Staff)
            .ThenInclude(projectStaff => projectStaff.User)
            .Include(project => project.Quotations)
            .ThenInclude(quotation => quotation.QuotationDetails)
            .ThenInclude(quotationDetail => quotationDetail.Service)
            .Include(project => project.Quotations)
            .ThenInclude(quotation => quotation.QuotationEquipments)
            .ThenInclude(quotationEquipment => quotationEquipment.Equipment)
            .Include(project => project.Designs)
            .ThenInclude(design => design.DesignImages)
            .Include(project => project.Contracts)
            .ThenInclude(contract => contract.PaymentBatches)
            .Include(project => project.ConstructionItems)
            .ThenInclude(constructionItem => constructionItem.ConstructionTasks)
            .ToList();

        return projects.Select(project => mapper.Map<ProjectResponse>(project));
        */
        return new List<ProjectResponse>();
    }

    public async Task<IEnumerable<ProjectResponse>> GetsAsync(PaginationFilter filter)
    {
        var repo = unitOfWork.Repository<Project>();
        var query = repo.Get();

        query = repo.GetPagingQueryable(filter.PageNumber, filter.PageSize);

        /*query = query.Include();*/

        var projects = await query.ToListAsync();

        var projectResponse = (IEnumerable<ProjectResponse>) mapper.Map<ProjectResponse>(projects);

        return projectResponse;
    }

    public async Task<ProjectResponse> GetAsync(Guid id)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        var project = projectRepo.Get()
                .Include(prokect => prokect.Package)
                .ThenInclude(package => package.PackageDetails)
                .Include(project => project.Customer)
                .ThenInclude(customer => customer.User)
                .Include(project => project.ProjectStaffs)
                .ThenInclude(projectStaff => projectStaff.Staff)
                .ThenInclude(projectStaff => projectStaff.User)
                .Include(project => project.Quotations)
                .ThenInclude(quotation => quotation.QuotationDetails)
                .ThenInclude(quotationDetail => quotationDetail.Service)
                .Include(project => project.Quotations)
                .ThenInclude(quotation => quotation.QuotationEquipments)
                .ThenInclude(quotationEquipment => quotationEquipment.Equipment)
                .Include(project => project.Designs)
                .ThenInclude(design => design.DesignImages)
                .Include(project => project.Contracts)
                .ThenInclude(contract => contract.PaymentBatches)
                .Include(project => project.ConstructionItems)
                .ThenInclude(constructionItem => constructionItem.ConstructionTasks)
            ;
        return mapper.Map<ProjectResponse>(project);
    }

    public async Task<int> CountAsync()
    {
        return await unitOfWork.Repository<Project>().Get().CountAsync();
    }

    public async Task<bool> CreateAsync(ProjectRequest request)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        Project? project = mapper.Map<Project>(request);
        await projectRepo.AddAsync(project, false);

        return await projectRepo.SaveAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, ProjectRequest request)
    {
        var projectRepo = unitOfWork.Repository<Project>();
        var project = await IsExistById(id);

        return false;
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
    #endregion
}