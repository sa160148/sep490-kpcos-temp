using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;

namespace KPCOS.BusinessLayer.Services;

public interface IServiceService
{
    Task CreateService(ServiceCreateRequest request);
    Task<ServiceReponse> GetServiceByIdAsync(Guid id);
    Task UpdateServiceAsync(Guid id, ServiceCreateRequest request);
    Task DeleteServiceAsync(Guid id);
    Task<List<ServiceReponse>> GetsAsync();
}