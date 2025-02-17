using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IEquipmentService
{
    Task CreateEquipmentAsync(EquipmentCreateRequest request);
    
    Task<(IEnumerable<EquipmentResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter);
    
    Task<EquipmentResponse> GetEquipmentByIdAsync(Guid id);
    
    Task UpdateEquipmentAsync(Guid id, EquipmentCreateRequest request);
    
    Task DeleteEquipmentAsync(Guid id);
}