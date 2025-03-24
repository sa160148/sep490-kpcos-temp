using KPCOS.BusinessLayer.DTOs.Request.DocsType;
using KPCOS.BusinessLayer.DTOs.Response.DocsType;

namespace KPCOS.BusinessLayer.Services;

public interface IDocTypeService
{
    Task CreateDocTypeAsync(DocsTypeRequest request);
    
    Task<DocsTypeResponse> GetDocTypeByIdAsync(Guid id);
    
    Task UpdateDocTypeAsync(Guid id, DocsTypeRequest typeRequest);
    
    Task DeleteDocTypeAsync(Guid id);
    
    Task<(IEnumerable<DocsTypeResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllDocsTypeFilterRequest filter);

}