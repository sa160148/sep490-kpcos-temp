using KPCOS.BusinessLayer.DTOs.Request.IssueTypes;
using KPCOS.BusinessLayer.DTOs.Response.IssueTypes;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class IssueTypeService : IIssueTypeService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageItemService> _logger;
    
    
    public IssueTypeService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    
    
    
    public async Task CreateIssueTypeAsync(IssueTypeRequest typeRequest)
    {
        IRepository<IssueType> issueTypeRepo = _unitOfWork.Repository<IssueType>();
        var issueTypeRaw = await issueTypeRepo.SingleOrDefaultAsync(service => service!.Name == typeRequest.Name);
        if (issueTypeRaw != null)
        {
            throw new BadRequestException("Mục đã tồn tại");
        }
        
        
        var issueType = new IssueType
        {
            Name = typeRequest.Name,
        };
        await issueTypeRepo.AddAsync(issueType, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IssueTypeResponse> GetIssueTypeByIdAsync(Guid id)
    {
        IRepository<IssueType> issueTypeRepo = _unitOfWork.Repository<IssueType>(); 
        var issueType =  await issueTypeRepo.FindAsync(id);
        if (issueType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        return new IssueTypeResponse
        {
            Id = issueType.Id,
            Name = issueType.Name,
        };
    }

    public async Task UpdateIssueTypeAsync(Guid id, IssueTypeRequest typeRequest)
    {
        IRepository<IssueType> issueTypeRepo = _unitOfWork.Repository<IssueType>();
        var issueType =  await issueTypeRepo.FindAsync(id);
        if (issueType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        
        var issueTypeRaw = await issueTypeRepo.SingleOrDefaultAsync(service => service!.Name == typeRequest.Name);
        if (issueTypeRaw != null && issueTypeRaw.Id != id)
        {
            throw new BadRequestException("Mục đã tồn tại");
        }
        issueType.Name = typeRequest.Name;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteIssueTypeAsync(Guid id)
    {
        IRepository<IssueType> issueTypeRepo = _unitOfWork.Repository<IssueType>();
        var issueType =  await issueTypeRepo.FindAsync(id);
        if (issueType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        issueTypeRepo.RemoveAsync(issueType, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<IssueTypeResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllIssueTypeFilterRequest filter)
    {
        var predicate = PredicateBuilder.New<IssueType>(true);
        
        // Combine with filter expressions from the filter object
        predicate = predicate.And(filter.GetExpressions());
        IRepository<IssueType> issueTypeRepo = _unitOfWork.Repository<IssueType>();
       
        var (items, total) = issueTypeRepo.GetWithCount(
            filter: predicate,
            orderBy: filter.GetOrder(),
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        return (items.Select(x => new IssueTypeResponse
        {
            Id = x.Id,
            Name = x.Name,
        }), total);
    }
}