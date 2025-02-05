using KPCOS.BusinessLayer.DTOs.Response;

namespace KPCOS.BusinessLayer.Services;

public interface IRoleService
{
    public Task<List<RoleResponse?>> GetsAsync();
}
