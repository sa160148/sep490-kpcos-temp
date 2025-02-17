using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;
namespace KPCOS.BusinessLayer.Services;

public interface IServiceService
{
    Task CreateService(ServiceCreateRequest request);
    Task<ServiceReponse> GetServiceByIdAsync(Guid id);
    Task UpdateServiceAsync(Guid id, ServiceCreateRequest request);
    Task DeleteServiceAsync(Guid id);
    Task<PaginationResult<ServiceReponse>> GetsAsync(PaginationFilter filter);
    Task<(IEnumerable<ServiceReponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter);


}