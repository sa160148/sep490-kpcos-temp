using KPCOS.BusinessLayer.DTOs.Request.IssueTypes;
using KPCOS.BusinessLayer.DTOs.Response.IssueTypes;

namespace KPCOS.BusinessLayer.Services;

public interface IIssueTypeService
{
    
    Task CreateIssueTypeAsync(IssueTypeRequest request);
    
    Task<IssueTypeResponse> GetIssueTypeByIdAsync(Guid id);
    
    Task UpdateIssueTypeAsync(Guid id, IssueTypeRequest typeRequest);
    
    Task DeleteIssueTypeAsync(Guid id);
    
    Task<(IEnumerable<IssueTypeResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllIssueTypeFilterRequest filter);
    
}