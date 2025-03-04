using System.Linq.Expressions;
using System.Security.Claims;
using KPCOS.API.Extensions.ServicesAddIn;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Users;
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
    [Route("api/staff")]
    /*[ApiController]*/
    public class StaffsController(IUserService userService, IAuthService authService) : BaseController
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
        // [CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> RegiterStaffAsync(UserRequest request)
        {
            await userService.RegiterStaffAsync(request);
            return Ok();
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
        // [CustomAuthorize("ADMINISTRATOR")]
        public async Task<PagedApiResponse<StaffResponse>> GetsStaffAsync([FromQuery]PaginationFilter filter)
        {
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
        
        /// <summary>
        /// Get available managers
        /// </summary>
        /// <param name="filter">Pagination parameters (PageNumber and PageSize)</param>
        /// <returns>A paginated list of managers who are not assigned to any active projects or only have finished projects</returns>
        /// <remarks>
        /// This endpoint returns managers who:
        /// - Are active in the system
        /// - Are not assigned to any active unfinished projects
        /// - Or have only finished projects
        /// 
        /// Sample request:
        ///     GET /api/staff/manager?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Returns the list of available managers</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(typeof(PagedApiResponse<StaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpGet("manager")]
        // [CustomAuthorize("ADMINISTRATOR")]
        public async Task<PagedApiResponse<StaffResponse>> GetAllManagers([FromQuery] PaginationFilter filter)
        {
            var response = await userService.GetsManagerAsync(filter);
            return new PagedApiResponse<StaffResponse>(response.data, filter.PageNumber, filter.PageSize, response.total);
        }
        
        /// <summary>
        /// Get available designers
        /// </summary>
        /// <param name="filter">Pagination parameters (PageNumber and PageSize)</param>
        /// <returns>A paginated list of designers who are not assigned to any active designing projects</returns>
        /// <remarks>
        /// This endpoint returns designers who:
        /// - Are active in the system
        /// - Are not assigned to any active projects in designing phase
        /// - Can be assigned to projects in other phases
        /// 
        /// Sample request:
        ///     GET /api/staff/designer?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Returns the list of available designers</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(typeof(PagedApiResponse<StaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpGet("designer")]
        // [CustomAuthorize("MANAGER")]
        public async Task<PagedApiResponse<StaffResponse>> GetAllDesigners([FromQuery] PaginationFilter filter)
        {
            var response = await userService.GetsDesignerAsync(filter);
            return new PagedApiResponse<StaffResponse>(response.data, filter.PageNumber, filter.PageSize, response.total);
        }
        
        /// <summary>
        /// Get available constructors
        /// </summary>
        /// <param name="filter">Pagination parameters (PageNumber and PageSize)</param>
        /// <returns>A paginated list of constructors who are not assigned to any active construction projects</returns>
        /// <remarks>
        /// This endpoint returns constructors who:
        /// - Are active in the system
        /// - Are not assigned to any active projects in construction phase
        /// - Can be assigned to projects in other phases
        /// 
        /// Sample request:
        ///     GET /api/staff/constructor?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Returns the list of available constructors</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(typeof(PagedApiResponse<StaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [HttpGet("constructor")]
        public async Task<PagedApiResponse<StaffResponse>> GetAllConstructors([FromQuery] PaginationFilter filter)
        {
            var response = await userService.GetsConstructorAsync(filter);
            return new PagedApiResponse<StaffResponse>(response.data, filter.PageNumber, filter.PageSize, response.total);
        }
        
        // [CustomAuthorize("ADMINISTRATOR")]
        [HttpGet("consultant")]
        public async Task<PagedApiResponse<StaffResponse>> GetConsultant ([FromQuery] PaginationFilter filter)
        {
            
            var response = await userService.GetsConsultantAsync(filter);
            return new PagedApiResponse<StaffResponse>(response.Data, filter.PageNumber, filter.PageSize, response.TotalRecords);
        }
    }
}
