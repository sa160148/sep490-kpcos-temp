using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class TemplateContructionService : ITemplateContructionService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceService> _logger;
    
    public TemplateContructionService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ServiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task CreateTemplateContructionAsync(TemplateContructionCreateRequest request)
    {
        IRepository<ConstructionTemplate> templateContructionRepo = _unitOfWork.Repository<ConstructionTemplate>();
        var templateContructionRaw = await templateContructionRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (templateContructionRaw != null)
        {
            throw new BadRequestException("Template đã tồn tại");
        }
        _logger.LogInformation("Template không tồn tại");
        
        var templateContruction = new ConstructionTemplate
        {
            Name = request.Name,
            Description = request.Description,
        };
        
        await templateContructionRepo.AddAsync(templateContruction, false);
        await _unitOfWork.SaveChangesAsync();
        
    }

    public async Task CreateTemplateContructionItemAsync(TemplateContructionItemCreateRequest request)
    {
        IRepository<ConstructionTemplateItem> templateContructionItemRepo = _unitOfWork.Repository<ConstructionTemplateItem>();
        
        // check template contruction item is valid
        var templateContruction = await _unitOfWork.Repository<ConstructionTemplate>().SingleOrDefaultAsync(s => s.Id == request.IdTemplateContruction);
        if (templateContruction == null)
        {
            throw new BadRequestException("Template không tồn tại");
        }
        
        var templateContructionItem = new ConstructionTemplateItem
        {
            Name = request.Name,
            Description = request.Description,
            Idparent = request.IdParent,
            Idtemplate = request.IdTemplateContruction,
        };
        await templateContructionItemRepo.AddAsync(templateContructionItem, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<TemplateContructionResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        IRepository<ConstructionTemplate> templateContructionRepo = _unitOfWork.Repository<ConstructionTemplate>();
        var pageData = await _unitOfWork.Repository<ConstructionTemplate>()
            .Get()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        var totalRecords = await templateContructionRepo.Get().CountAsync();
        return (Data: pageData.Select(templateContruction => new TemplateContructionResponse
        {
            Id = templateContruction.Id,
            Name = templateContruction.Name,
            Description = templateContruction.Description,
            IsActive = templateContruction.IsActive,
        }), TotalRecords: totalRecords);
    }

    public async Task<TemplateContructionDetailResponse> GetTemplateContructionByIdAsync(Guid id)
    {
        IRepository<ConstructionTemplate> templateContructionRepo = _unitOfWork.Repository<ConstructionTemplate>();
        var templateContruction = await templateContructionRepo.FindAsync(id);
        if (templateContruction == null)
        {
            throw new BadRequestException("Template không tồn tại");
        }
        return new TemplateContructionDetailResponse
        {
            Id = templateContruction.Id,
            Name = templateContruction.Name,
            Description = templateContruction.Description,
            IsActive = templateContruction.IsActive,
            TemplateContructionItems = _unitOfWork.Repository<ConstructionTemplateItem>().
                Get().
                Where(x => x.Idtemplate == id && x.Idparent == null).
                Select(x => new TemplateContructionItemResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Child = _unitOfWork.Repository<ConstructionTemplateItem>().
                        Get().
                        Where(y => y.Idparent == x.Id).
                        Select(y => new TemplateContructionItemResponse
                        { 
                            Id = y.Id,
                            Name = y.Name,
                            Description = y.Description,
                        }).ToList(),
                }).ToList(),
        };
        
    }
}