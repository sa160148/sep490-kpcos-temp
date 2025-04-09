using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.TemplateConstructions;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using LinqKit;
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
        var templateContruction = await _unitOfWork.Repository<ConstructionTemplate>().
            SingleOrDefaultAsync(s => s.Id == request.IdTemplateContruction);
        if (templateContruction == null)
        {
            throw new BadRequestException("Template không tồn tại");
        }
        
        var templateContructionItem = new ConstructionTemplateItem
        {
            Category = request.Category,
            Name = request.Name,
            Description = request.Description,
            Idparent = request.IdParent,
            Idtemplate = request.IdTemplateContruction,
            Duration = request.Duration,
            
        };
        await templateContructionItemRepo.AddAsync(templateContructionItem, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<TemplateContructionResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllConstructionTemplateFilterRequest filter)
    {
        
        var predicate = PredicateBuilder.New<ConstructionTemplate>(true);
        
        // Combine with filter expressions from the filter object
        predicate = predicate.And(filter.GetExpressions());
        IRepository<ConstructionTemplate> templateContructionRepo = _unitOfWork.Repository<ConstructionTemplate>();
       
        var (items, total) = templateContructionRepo.GetWithCount(
            filter: predicate,
            orderBy: filter.GetOrder(),
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        return (items.Select(x => new TemplateContructionResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        }), total);
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
                OrderBy(x => x.CreatedAt).
                Select(x => new TemplateContructionItemResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Category = x.Category,
                    
                    Child = _unitOfWork.Repository<ConstructionTemplateItem>().
                        Get().
                        Where(y => y.Idparent == x.Id).
                        OrderBy(x => x.CreatedAt).
                        Select(y => new TemplateContructionItemResponse
                        { 
                            Id = y.Id,
                            Name = y.Name,
                            Description = y.Description,
                            Duration = y.Duration
                        }).ToList(),
                    Duration = _unitOfWork.Repository<ConstructionTemplateItem>()
                        .Get()
                        .Where(y => y.Idparent == x.Id)
                        .Sum(y => y.Duration)
                }).ToList(),
        };
        
    }

    
    
    public async Task ActiveTemplateContructionAsync(Guid id)
    {
        IRepository<ConstructionTemplate> templateContructionRepo = _unitOfWork.Repository<ConstructionTemplate>();
        IRepository<ConstructionTemplateItem> templateContructionItemRepo = _unitOfWork.Repository<ConstructionTemplateItem>();
        var templateContruction = await templateContructionRepo.FindAsync(id);
        if (templateContruction == null)
        {
            throw new BadRequestException("Template không tồn tại");
        }

        // check current active status
        bool currentActive = templateContruction.IsActive ?? false;

        if (currentActive)
        {
            templateContruction.IsActive = false;
        }
        else
        {
            // cout template construction by id contruction
            var templateContructionItems = templateContructionItemRepo.GetWithCount(
                filter: x => x.Idtemplate == id && x.Idparent == null,
                orderBy: x => x.OrderBy(x => x.CreatedAt),
                pageIndex: 1,
                pageSize: int.MaxValue
            );
            
            if (templateContructionItems.Count < 3)
            {
                throw new BadRequestException("Template không đủ 3 item");
            }
            templateContruction.IsActive = true;
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}