using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    /*[ApiController]*/
    public class UsersController(IUserService service) : BaseController
    {

    }
}
