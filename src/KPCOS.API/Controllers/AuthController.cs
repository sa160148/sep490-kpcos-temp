using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
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

        /// <summary>
        /// FEATURE : Login, signin
        /// </summary>
        /// <param name="request">
        /// request object contains email property and password property. 
        /// </param>
        /// <returns>
        /// An Object with a JSON format that contains User Id, Avatar, Role name.
        /// </returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/signin
        ///     {
        ///         "email": "root@gmail.com",
        ///         "password": "string"
        ///     }
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [HttpPost("signin")]
        [ProducesResponseType(typeof(ApiResult<SigninResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResult<SigninResponse>> SignInAsync(SigninRequest request)
        {
            
            var response =  await _authService.SignInAsync(request);
            return new ApiResult<SigninResponse>(true, ApiResultStatusCode.Success, response, String.Empty);
            
        }

        /// <summary>
        /// FEATURE : Register, signup by customer
        /// </summary>
        /// <param name="request">
        /// request object contains fullName, email, password,
        /// phone, address, dob property. 
        /// </param>
        /// <returns>
        /// An Object with a JSON format.
        /// </returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/signup
        ///     {
        ///         "fullName": "root"
        ///         "email": "root@gmail.com",
        ///         "password": "string",
        ///         "phone": "0123456789",
        ///         "address": "Hanoi",
        ///         "gender": "MALE"
        ///         "dob": "2022-01-01",
        ///     }
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpPost("signup")]
        public async Task<ApiResult> SignUpAsync(SignupRequest request)
        {
            await _authService.SignUpAsync(request);
            return new ApiResult(true, ApiResultStatusCode.Success, String.Empty);
        }
    }
}
