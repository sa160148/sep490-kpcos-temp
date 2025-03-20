using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface ITemplateContructionService
{
    Task CreateTemplateContructionAsync(TemplateContructionCreateRequest request);
    
    Task CreateTemplateContructionItemAsync(TemplateContructionItemCreateRequest request);
    
    Task<(IEnumerable<TemplateContructionResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllConstructionTemplateFilterRequest filter);
    
    Task<TemplateContructionDetailResponse> GetTemplateContructionByIdAsync(Guid id);
    
    Task ActiveTemplateContructionAsync(Guid id);
}