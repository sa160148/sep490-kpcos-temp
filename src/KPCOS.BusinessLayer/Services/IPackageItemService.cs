using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IPackageItemService
{
    Task CreatePackageItemAsync(PackageItemCreateRequest request);
    
    Task<PackageItemResponse> GetPackageItemByIdAsync(Guid id);
    Task UpdatePackageItemAsync(Guid id, PackageItemCreateRequest request);
    Task DeletePackageItemAsync(Guid id);
    Task<(IEnumerable<PackageItemResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter);
}