using KPCOS.BusinessLayer.DTOs.Request;

namespace KPCOS.BusinessLayer.Services;

public interface IConstructionServices
{
    Task CreateConstructionAsync(ConstructionRequest request);
    
}