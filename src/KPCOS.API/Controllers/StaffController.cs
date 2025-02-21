using System.Security.Claims;
using KPCOS.API.Extensions.ServicesAddIn;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Pagination;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    /*[ApiController]*/
    public class StaffController(IUserService userService, IAuthService authService) : BaseController
    {
        /// <summary>
        /// Create staff
        /// </summary>
        /// <param name="request">
        /// <para><see cref="UserRequest"/> request object contains fullName, email, password,
        /// phone, position property.</para>
        ///
        /// fullName: string.
        /// email: string.
        /// password: string.
        /// phone: string.
        /// position: enum(CONSTRUCTOR, MANAGER, CONSULTANT, ADMINISTRATOR, DESIGNER).
        /// </param>
        /// <returns>
        /// An Object with a JSON format.  <see cref="ApiResult"/>
        /// </returns>
        /// <remarks>
        /// <para>Create a new staff, only ADMINISTRATOR can create staff.</para>  
        /// Sample request:
        /// 
        ///     POST /api/users/staff
        ///     {
        ///         "fullName": "root"
        ///         "email": "root@gmail.com",
        ///         "password": "string",
        ///         "phone": "0123456789",
        ///         "position": "CONSTRUCTOR",
        ///     }
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ApiResult> RegiterStaffAsync(UserRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Vui lòng đăng nhập lại");
            }
            var isValidPosition = await authService.GetPositionAsync(Guid.Parse(userId));
            if (isValidPosition != RoleEnum.ADMINISTRATOR)
            {
                throw new Exception("Không có khả năng truy cập");
            }

            var response = await userService.RegiterStaffAsync(request);
            if (response)
            {
                return new ApiResult(true, ApiResultStatusCode.Success);
            }
            return new ApiResult(false, ApiResultStatusCode.ServerError);
        }


        /// <summary>
        /// Get all staff
        /// </summary>
        /// <param name="request">The request containing filter parameters for booking statuses (such as HOLD, VALIDATION, etc.), user ID, and other query criteria.</param>
        /// <returns>
        /// A list of bookings that match the filter criteria. If an error occurs, an error response will be returned.
        /// </returns>
        /// <remarks>
        /// <para>Retrieve a paginated list of staff, only ADMINISTRATOR can get all staff</para>
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [ProducesResponseType(typeof(PagedApiResponse<StaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpGet]
        /*[Authorize/*(Roles = "ADMINISTRATOR")#1#]*/
        /*[RequiresClaim("ADMINISTRATOR", "true")]*/
        public async Task<PagedApiResponse<StaffResponse>> GetsStaffAsync([FromQuery]PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Vui lòng đăng nhập lại");
            }
            var isValidPosition = await authService.GetPositionAsync(Guid.Parse(userId));
            if (isValidPosition != RoleEnum.ADMINISTRATOR)
            {
                throw new Exception("Không có khả năng truy cập");
            }
            
            var count = await userService.CountStaffAsync();
            if (count == 0)
            {
                return new PagedApiResponse<StaffResponse>(new List<StaffResponse>(),
                    filter.PageNumber,
                    filter.PageSize,
                    count);
            }

            var response = await userService.GetsStaffAsync(filter);
            return new PagedApiResponse<StaffResponse>(
                response,
                filter.PageNumber,
                filter.PageSize,
                count);
        }
    }
}
