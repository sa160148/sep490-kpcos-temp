using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using KPCOS.BusinessLayer.DTOs.Response.objects;


namespace KPCOS.BusinessLayer.Services.Implements;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RoleResponse?>> GetsAsync()
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var roles = await roleRepo.Get().ToListAsync();
        return roles.Select(role => new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        }).ToList();
    }

}
