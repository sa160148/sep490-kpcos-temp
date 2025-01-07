using KPCOS.BusinessLayer.DTOs.Response.objects;

namespace KPCOS.BusinessLayer.Services;

public interface IRoleService
{
    public Task<List<RoleResponse?>> GetsAsync();
}
