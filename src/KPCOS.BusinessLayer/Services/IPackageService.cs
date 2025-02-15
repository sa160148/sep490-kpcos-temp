using KPCOS.BusinessLayer.DTOs.Request;

namespace KPCOS.BusinessLayer.Services;

public interface IPackageService
{
    Task CreatePackageAsync(PackageCreateRequest request);
}