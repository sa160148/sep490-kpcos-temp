using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.objects;
using KPCOS.BusinessLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KPCOS.WebFramework.Api;
namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    /*[ApiController]*/
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin")]
        public async Task<ApiResult<SigninResponse>> SignInAsync(SigninRequest request)
        {
            
            var response =  await _authService.SignInAsync(request);
            return response;
            
        }
        
        [HttpPost("signup")]
        public async Task<ActionResult<ApiResult>> SignUpAsync(SignupRequest request)
        {
            await _authService.SignUpAsync(request);
            return Ok();
        }
    }
}
