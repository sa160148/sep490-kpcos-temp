using KPCOS.BusinessLayer.DTOs.Request;

namespace KPCOS.BusinessLayer.Services;

public interface IContractService
{
    Task CreateContractAsync(ContractCreateRequest request);
}