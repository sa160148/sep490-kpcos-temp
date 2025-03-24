using KPCOS.BusinessLayer.DTOs.Request.DocsType;
using KPCOS.BusinessLayer.DTOs.Response.DocsType;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class DocTypeService : IDocTypeService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageItemService> _logger;
    
    public DocTypeService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task CreateDocTypeAsync(DocsTypeRequest request)
    {
        IRepository<DocType> docTypeRepo = _unitOfWork.Repository<DocType>();
        var docTypeRaw = await docTypeRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (docTypeRaw != null)
        {
            throw new BadRequestException("Mục đã tồn tại");
        }
        
        var docType = new DocType
        {
            Name = request.Name,
        };
        await docTypeRepo.AddAsync(docType, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DocsTypeResponse> GetDocTypeByIdAsync(Guid id)
    {
        IRepository<DocType> docTypeRepo = _unitOfWork.Repository<DocType>(); 
        var docType =  await docTypeRepo.FindAsync(id);
        if (docType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        return new DocsTypeResponse
        {
            Id = docType.Id,
            Name = docType.Name,
        };
    }

    public async Task UpdateDocTypeAsync(Guid id, DocsTypeRequest typeRequest)
    {
        IRepository<DocType> docTypeRepo = _unitOfWork.Repository<DocType>();
        var docType = await docTypeRepo.FindAsync(id);
        if (docType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        docType.Name = typeRequest.Name;
        await docTypeRepo.UpdateAsync(docType, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteDocTypeAsync(Guid id)
    {
        IRepository<DocType> docTypeRepo = _unitOfWork.Repository<DocType>();
        var docType = await docTypeRepo.FindAsync(id);
        if (docType == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        await docTypeRepo.RemoveAsync(docType, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<DocsTypeResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllDocsTypeFilterRequest filter)
    {
        var predicate = PredicateBuilder.New<DocType>(true);
        
        // Combine with filter expressions from the filter object
        predicate = predicate.And(filter.GetExpressions());
        IRepository<DocType> issueTypeRepo = _unitOfWork.Repository<DocType>();

        var (items, total) = issueTypeRepo.GetWithCount(
            filter: predicate,
            orderBy: filter.GetOrder(),
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        return (items.Select(service => new DocsTypeResponse
        {
            Id = service.Id,
            Name = service.Name,
        }), total);
    }
}