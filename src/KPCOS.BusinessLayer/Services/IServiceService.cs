using KPCOS.BusinessLayer.DTOs.Request;

namespace KPCOS.BusinessLayer;

public interface IServiceService
{
    Task CreateService(ServiceCreateRequest request);
}