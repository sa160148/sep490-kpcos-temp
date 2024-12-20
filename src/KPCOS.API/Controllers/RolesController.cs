using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KPCOS.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly ILogger<RolesController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RolesController(ILogger<RolesController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var query = _unitOfWork.Repository<Role>().Get();
        var roles = query.ToList();
        if (roles.IsNullOrEmpty())
        {
            return StatusCode(404, new BaseResponse
            {
                Message = "no roles found",
                ResponseCode = 404
            });
        }

        return StatusCode(200, new BaseResponse<List<Role>>
        {
            ResponseCode = 200,
            Message = "",
            Data = roles
        });
    }
}