using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IQuotationService
{
    Task CreateQuotationAsync(QuotationCreateRequest request);
    
    Task<(IEnumerable<QuotationResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter);
    //
    Task<QuotationResponse> GetQuotationByIdAsync(Guid id);
    //
    // Task UpdateQuotationAsync(Guid id, QuotationCreateRequest request);
    // Task DeleteQuotationAsync(Guid id);
    
    Task RejectOrAcceptQuotationAsync(Guid id, QuotationRejectOrAcceptRequest request);
    
    Task ApproveOrCancelEditQuotationAsync(Guid id, QuotationApproveOrEditRequest request);
    
    Task UpdateQuotationAsync(Guid id, QuotationCreateRequest request);
    
    Task RewriteQuotationAsync (Guid id, QuotationCreateRequest request);
    
}