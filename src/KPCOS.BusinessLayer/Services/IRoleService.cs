using KPCOS.BusinessLayer.DTOs.Response.Roles;

namespace KPCOS.BusinessLayer.Services;

public interface IRoleService
{
    public Task<List<RoleResponse?>> GetsAsync();
}
