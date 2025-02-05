using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KPCOS.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RolesController : BaseController
{
    private readonly ILogger<RolesController> _logger;
    private readonly IRoleService _roleService;

    public RolesController(ILogger<RolesController> logger, IRoleService roleService)
    {
        _logger = logger;
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ApiResult<List<RoleResponse>>> Get()
    {
        var roles = await _roleService.GetsAsync();
        return roles;
    }
}