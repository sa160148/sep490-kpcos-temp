using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IPackageService
{
    Task CreatePackageAsync(PackageCreateRequest request);
    
    Task<(IEnumerable<PackageResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter);
}