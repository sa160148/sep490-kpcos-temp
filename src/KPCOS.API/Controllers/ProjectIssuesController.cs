using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/project-issues")]
    public class ProjectIssuesController : BaseController
    {
        private readonly IProjectIssueService _projectIssueService;

        public ProjectIssuesController(IProjectIssueService projectIssueService)
        {
            _projectIssueService = projectIssueService;
        }
        
        /// <summary>
        /// Tạo mới một vấn đề cho dự án hồ Koi
        /// </summary>
        /// <param name="id">ID của hạng mục xây dựng liên quan đến vấn đề</param>
        /// <param name="request">Thông tin chi tiết của vấn đề cần tạo</param>
        /// <remarks>
        /// API này cho phép tạo một vấn đề mới cho dự án hồ Koi dựa trên hạng mục xây dựng.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Vấn đề được liên kết với hạng mục xây dựng cấp 1 (cha)
        /// - Nếu ID được cung cấp là của hạng mục xây dựng cấp 2 (con), hệ thống sẽ tự động tìm hạng mục cha tương ứng
        /// - Tên vấn đề phải là duy nhất trong cùng một hạng mục xây dựng cấp 1
        /// - Vấn đề mới luôn được tạo với trạng thái "OPENING"
        /// - Người dùng phải đăng nhập để tạo vấn đề
        /// 
        /// **Dữ liệu bắt buộc:**
        /// - Tên vấn đề (name)
        /// - ID loại vấn đề (issueTypeId)
        /// 
        /// **Ví dụ yêu cầu:**
        /// ```json
        /// {
        ///   "name": "Rò rỉ hồ Koi",
        ///   "description": "Phát hiện rò rỉ nước ở thành hồ Koi phía Đông, mất khoảng 2cm nước mỗi ngày",
        ///   "reason": "Có thể do vết nứt trong lớp lót hoặc ống thoát nước bị hỏng",
        ///   "issueTypeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "issueImages": [
        ///     {
        ///       "name": "Hình ảnh vết nứt hồ Koi",
        ///       "imageUrl": "https://example.com/images/pond-leak.jpg"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Kết quả tạo vấn đề</returns>
        /// <response code="200">Tạo vấn đề thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc vấn đề trùng tên đã tồn tại</response>
        /// <response code="404">Không tìm thấy hạng mục xây dựng với ID được cung cấp</response>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Tạo mới một vấn đề cho dự án hồ Koi",
            Description = "Tạo một vấn đề mới cho dự án hồ Koi dựa trên hạng mục xây dựng cấp 1. Nếu ID được cung cấp là của hạng mục cấp 2, hệ thống sẽ tự động tìm hạng mục cha.",
            OperationId = "CreateProjectIssue",
            Tags = new[] { "Project Issues" }
        )]
        public async Task<ApiResult> CreateProjectIssueAsync(
            [SwaggerParameter(
                Description = "ID của hạng mục xây dựng liên quan đến vấn đề",
                Required = true
            )]
            Guid id, 
            [FromBody] 
            [SwaggerParameter(
                Description = "Thông tin chi tiết của vấn đề cần tạo, bao gồm tên, mô tả, lý do, ID loại vấn đề và hình ảnh",
                Required = true
            )]
            CommandProjectIssueRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Vui lòng đăng nhập để tạo vấn đề hoặc không tìm thấy tài khoản");
            }
            await _projectIssueService.CreateProjectIssueAsync(id, request, userId);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin vấn đề dự án hồ Koi
        /// </summary>
        /// <param name="id">ID của vấn đề cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật cho vấn đề</param>
        /// <remarks>
        /// API này cho phép cập nhật thông tin của vấn đề dự án hồ Koi hiện có.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Nếu tên được cập nhật, tên mới phải là duy nhất trong cùng hạng mục xây dựng
        /// - Có thể đánh dấu vấn đề là đã giải quyết (isSolved=true), nhưng phải cung cấp giải pháp
        /// - Trạng thái vấn đề được cập nhật tự động dựa trên các hành động:
        ///   - Khi đánh dấu là đã giải quyết (isSolved=true), trạng thái chuyển thành "SOLVED"
        ///   - Khi cung cấp giải pháp và trạng thái hiện tại là "OPENING", trạng thái chuyển thành "PROCESSING"
        /// - Có thể thêm hình ảnh mới cho vấn đề
        /// 
        /// **Ví dụ yêu cầu cập nhật thông thường:**
        /// ```json
        /// {
        ///   "name": "Rò rỉ nghiêm trọng hồ Koi",
        ///   "description": "Rò rỉ nước ở thành hồ Koi phía Đông, mất khoảng 5cm nước mỗi ngày và đang trở nên nghiêm trọng hơn",
        ///   "reason": "Xác định do lớp lót EPDM bị rách ở đáy hồ",
        ///   "solution": "Cần tháo nước, loại bỏ lớp lót cũ và thay thế bằng lớp lót mới chất lượng cao",
        ///   "issueTypeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "issueImages": [
        ///     {
        ///       "name": "Hình ảnh vị trí rò rỉ mới",
        ///       "imageUrl": "https://example.com/images/pond-leak-severe.jpg"
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// **Ví dụ yêu cầu đánh dấu đã giải quyết:**
        /// ```json
        /// {
        ///   "solution": "Đã thay thế toàn bộ lớp lót EPDM, kiểm tra kín nước và đã bổ sung hệ thống thoát nước dự phòng ngày 15/7/2024",
        ///   "isSolved": true
        /// }
        /// ```
        /// </remarks>
        /// <returns>Kết quả cập nhật vấn đề</returns>
        /// <response code="200">Cập nhật vấn đề thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ, tên trùng lặp, hoặc thiếu giải pháp khi đánh dấu đã giải quyết</response>
        /// <response code="404">Không tìm thấy vấn đề với ID được cung cấp</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin vấn đề dự án hồ Koi",
            Description = "Cập nhật thông tin của vấn đề dự án hồ Koi hiện có, bao gồm tên, mô tả, lý do, giải pháp, trạng thái và có thể thêm hình ảnh mới.",
            OperationId = "UpdateProjectIssue",
            Tags = new[] { "Project Issues" }
        )]
        public async Task<ApiResult> UpdateProjectIssueAsync(
            [SwaggerParameter(
                Description = "ID của vấn đề cần cập nhật",
                Required = true
            )]
            Guid id,
            [FromBody]
            [SwaggerParameter(
                Description = "Thông tin cập nhật cho vấn đề",
                Required = true
            )]
            CommandProjectIssueRequest request)
        {
            await _projectIssueService.UpdateProjectIssueAsync(id, request);
            return Ok();
        }

        /// <summary>
        /// Xóa hình ảnh của vấn đề dự án hồ Koi
        /// </summary>
        /// <param name="id">ID của hình ảnh cần xóa</param>
        /// <remarks>
        /// API này cho phép xóa một hình ảnh cụ thể của vấn đề dự án hồ Koi.
        /// 
        /// **Lưu ý:**
        /// - Hình ảnh sẽ bị xóa hoàn toàn khỏi hệ thống
        /// - Không ảnh hưởng đến các hình ảnh khác của vấn đề
        /// </remarks>
        /// <returns>Kết quả xóa hình ảnh</returns>
        /// <response code="200">Xóa hình ảnh thành công</response>
        /// <response code="404">Không tìm thấy hình ảnh với ID được cung cấp</response>
        [HttpDelete("images/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Xóa hình ảnh của vấn đề dự án hồ Koi",
            Description = "Xóa một hình ảnh cụ thể của vấn đề dự án hồ Koi dựa trên ID của hình ảnh đó.",
            OperationId = "DeleteIssueImage",
            Tags = new[] { "Project Issues" }
        )]
        public async Task<ApiResult> DeleteIssueImageAsync(
            [SwaggerParameter(
                Description = "ID của hình ảnh cần xóa",
                Required = true
            )]
            Guid id)
        {
            await _projectIssueService.DeleteIssueImageAsync(id);
            return Ok();
        }
    }
}
