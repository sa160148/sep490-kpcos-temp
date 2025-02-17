using KPCOS.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [ApiController]
    //[AllowAnonymous]
    [ApiResultFilter]
    [Route("api/v{version:apiVersion}/[controller]")]// api/v1/[controller]
    public class BaseController : ControllerBase
    {
       
    }
}
