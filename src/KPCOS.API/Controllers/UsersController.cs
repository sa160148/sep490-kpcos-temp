using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService service) : BaseController
    {
        /// <summary>
        /// Lấy thông tin chi tiết người dùng theo ID
        /// </summary>
        /// <param name="id">ID người dùng</param>
        /// <returns>Thông tin chi tiết người dùng</returns>
        [HttpGet("info")]
        [SwaggerOperation(
            Summary = "Lấy thông tin chi tiết người dùng",
            Description = "Lấy thông tin chi tiết của một người dùng dựa trên ID",
            OperationId = "GetUserById",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Thành công", typeof(GetDetailUserResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Không tìm thấy người dùng")]
        public async Task<ApiResult<GetDetailUserResponse>> GetUserByIdAsync(
            [FromQuery]
            GetAllUserFilterRequest filter
        )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && filter.Id == null)
            {
                filter.Id = Guid.Parse(userIdClaim);
            }
            if (filter.Id == null)
            {
                throw new BadRequestException("User ID is required");
            }
            var user = await service.GetUserByIdAsync(filter.Id.Value);
            return Ok(user);
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        /// <param name="filter">Bộ lọc phân trang</param>
        /// <returns>Danh sách người dùng</returns>
        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Lấy danh sách tất cả người dùng",
            Description = "Lấy danh sách tất cả người dùng với phân trang",
            OperationId = "GetAllUsers",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Thành công", typeof(PagedApiResponse<GetDetailUserResponse>))]
        public async Task<PagedApiResponse<GetDetailUserResponse>> GetAllUsersAsync(
            [FromQuery]
            GetAllUserFilterRequest filter
        )
        {
            var (data, totalRecords) = await service.GetAllUsersAsync(filter);
            return new PagedApiResponse<GetDetailUserResponse>(
                data, 
                filter.PageNumber, 
                filter.PageSize,
                totalRecords
            );
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="id">ID người dùng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin người dùng",
            Description = "Cập nhật thông tin của một người dùng dựa trên ID",
            OperationId = "UpdateUser",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Cập nhật thành công")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Không tìm thấy người dùng")]
        public async Task<ApiResult> UpdateUserAsync(Guid id, CommandUserRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await service.UpdateUserAsync(id, request);
            return Ok();
        }
    }
}
