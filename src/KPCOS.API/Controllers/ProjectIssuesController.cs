using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
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
        /// - Vấn đề chỉ có thể được liên kết với hạng mục xây dựng cấp 1 (cha, không có parentId)
        /// - Hạng mục xây dựng cấp 2 (con) không thể được sử dụng để tạo vấn đề
        /// - Tên vấn đề phải là duy nhất trong cùng một hạng mục xây dựng cấp 1
        /// - Vấn đề mới luôn được tạo với trạng thái "OPENING"
        /// - Các trường được chấp nhận: tên, mô tả, nguyên nhân, giải pháp, hình ảnh, loại vấn đề, và ngày dự kiến hoàn thành
        /// - Các trường khác (lý do, hình ảnh xác nhận, nhân viên, ngày thực tế hoàn thành) sẽ bị bỏ qua và đặt là null
        /// 
        /// **Dữ liệu bắt buộc:**
        /// - Tên vấn đề (name)
        /// - Nguyên nhân (cause)
        /// - ID loại vấn đề (issueTypeId)
        /// - Hình ảnh vấn đề (issueImage)
        /// 
        /// **Dữ liệu tùy chọn:**
        /// - Mô tả (description)
        /// - Giải pháp (solution)
        /// - Ngày dự kiến hoàn thành (estimateAt) - định dạng ISO 8601: "YYYY-MM-DD"
        /// 
        /// **Ví dụ yêu cầu:**
        /// ```json
        /// {
        ///   "name": "Rò rỉ hồ Koi",
        ///   "description": "Phát hiện rò rỉ nước ở thành hồ Koi phía Đông, mất khoảng 2cm nước mỗi ngày",
        ///   "cause": "Vết nứt trong lớp lót EPDM",
        ///   "solution": "Cần thay thế lớp lót EPDM bị hỏng và kiểm tra kỹ toàn bộ mối nối",
        ///   "issueTypeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "issueImage": "https://example.com/images/pond-leak.jpg",
        ///   "estimateAt": "2024-04-30"
        /// }
        /// ```
        /// 
        /// **Lưu ý:** Các trường khác nếu được cung cấp trong yêu cầu sẽ bị bỏ qua, bao gồm:
        /// - reason (lý do)
        /// - confirmImage (hình ảnh xác nhận)
        /// - staffId (ID nhân viên)
        /// - actualAt (ngày thực tế hoàn thành)
        /// </remarks>
        /// <returns>Kết quả tạo vấn đề</returns>
        /// <response code="200">Tạo vấn đề thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ, vấn đề trùng tên đã tồn tại, hoặc hạng mục xây dựng không phải cấp 1</response>
        /// <response code="404">Không tìm thấy hạng mục xây dựng với ID được cung cấp</response>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Tạo mới một vấn đề cho dự án hồ Koi",
            Description = "Tạo một vấn đề mới cho dự án hồ Koi dựa trên hạng mục xây dựng cấp 1. Chỉ chấp nhận hạng mục xây dựng cấp 1 (không có parentId).",
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
                Description = "Thông tin chi tiết của vấn đề cần tạo, bao gồm tên, mô tả, nguyên nhân, giải pháp, ID loại vấn đề, hình ảnh và ngày dự kiến hoàn thành",
                Required = true
            )]
            CommandProjectIssueRequest request)
        {
            await _projectIssueService.CreateProjectIssueAsync(id, request, Guid.Empty);
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
        /// - API hỗ trợ 4 trường hợp cập nhật với chuyển đổi trạng thái khác nhau:
        ///   1. Cập nhật thông thường: Không thay đổi trạng thái trừ khi đặt mới giá trị trạng thái
        ///   2. Chỉ cập nhật nhân viên: Nếu trạng thái hiện tại là "OPENING", trạng thái chuyển thành "PROCESSING"
        ///   3. Chỉ cập nhật hình ảnh xác nhận: Trạng thái chuyển thành "PREVIEWING"
        ///   4. Chỉ cập nhật lý do: Nếu trạng thái hiện tại là "PREVIEWING", trạng thái chuyển thành "PROCESSING"
        /// - Khi gán nhân viên phụ trách (staffId), hệ thống sẽ kiểm tra:
        ///   1. Nhân viên phải tồn tại trong hệ thống
        ///   2. Nhân viên phải được phân công cho dự án liên quan
        ///   3. Nhân viên không được đang phụ trách công việc hoặc vấn đề khác chưa hoàn thành
        /// 
        /// **Lưu ý quan trọng:** Trường `staffId` trong yêu cầu phải là ID của người dùng (User.Id), không phải ID của nhân viên (Staff.Id).
        /// Hệ thống sẽ tự động tìm nhân viên tương ứng với người dùng.
        /// 
        /// **Ví dụ yêu cầu cập nhật thông thường:**
        /// ```json
        /// {
        ///   "name": "Rò rỉ nghiêm trọng hồ Koi",
        ///   "description": "Rò rỉ nước ở thành hồ Koi phía Đông, mất khoảng 5cm nước mỗi ngày và đang trở nên nghiêm trọng hơn",
        ///   "cause": "Vết nứt lớn trong lớp lót EPDM",
        ///   "solution": "Cần tháo nước, loại bỏ lớp lót cũ và thay thế bằng lớp lót mới chất lượng cao",
        ///   "issueImage": "https://example.com/images/pond-leak-severe.jpg",
        ///   "status": "PROCESSING"
        /// }
        /// ```
        /// 
        /// **Ví dụ yêu cầu chỉ thay đổi nhân viên phụ trách (Trường hợp 2):**
        /// ```json
        /// {
        ///   "staffId": "3fa85f64-5717-4562-b3fc-2c963f66afa9"
        /// }
        /// ```
        /// Trong ví dụ trên, `staffId` là ID của người dùng (User.Id), không phải ID của nhân viên (Staff.Id).
        /// 
        /// **Ví dụ yêu cầu chỉ cập nhật hình ảnh xác nhận (Trường hợp 3):**
        /// ```json
        /// {
        ///   "confirmImage": "https://example.com/images/confirmation-image.jpg"
        /// }
        /// ```
        /// 
        /// **Ví dụ yêu cầu chỉ cập nhật lý do (Trường hợp 4):**
        /// ```json
        /// {
        ///   "reason": "Sau khi kiểm tra xác nhận, phát hiện lớp lót EPDM bị rách do va chạm với vật sắc nhọn trong quá trình lắp đặt"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Kết quả cập nhật vấn đề</returns>
        /// <response code="200">Cập nhật vấn đề thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc tên trùng lặp</response>
        /// <response code="404">Không tìm thấy vấn đề với ID được cung cấp</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin vấn đề dự án hồ Koi",
            Description = "Cập nhật thông tin vấn đề với 4 trường hợp: 1) Cập nhật thông thường 2) Cập nhật chỉ nhân viên 3) Cập nhật chỉ hình ảnh xác nhận 4) Cập nhật chỉ lý do. Mỗi trường hợp có quy tắc chuyển đổi trạng thái riêng.",
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
        /// <param name="id">ID của vấn đề cần xóa hình ảnh</param>
        /// <remarks>
        /// API này cho phép xóa hình ảnh của vấn đề dự án hồ Koi.
        /// 
        /// **Lưu ý:**
        /// - Hình ảnh sẽ bị xóa hoàn toàn khỏi hệ thống
        /// - Vấn đề sẽ không còn hình ảnh sau khi thực hiện hành động này
        /// </remarks>
        /// <returns>Kết quả xóa hình ảnh</returns>
        /// <response code="200">Xóa hình ảnh thành công</response>
        /// <response code="404">Không tìm thấy vấn đề với ID được cung cấp</response>
        [HttpDelete("images/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Xóa hình ảnh của vấn đề dự án hồ Koi",
            Description = "Xóa hình ảnh của vấn đề dự án hồ Koi bằng cách xóa trường hình ảnh khỏi vấn đề đó.",
            OperationId = "DeleteIssueImage",
            Tags = new[] { "Project Issues" }
        )]
        public async Task<ApiResult> DeleteIssueImageAsync(
            [SwaggerParameter(
                Description = "ID của vấn đề cần xóa hình ảnh",
                Required = true
            )]
            Guid id)
        {
            await _projectIssueService.DeleteIssueImageAsync(id);
            return Ok();
        }

        [HttpPut("{id}/confirm")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Xác nhận hoàn thành vấn đề dự án hồ Koi",
            Description = "Xác nhận vấn đề đã được giải quyết và chuyển trạng thái từ PREVIEWING sang DONE. Đồng thời kiểm tra và cập nhật trạng thái của hạng mục xây dựng liên quan nếu cần.",
            OperationId = "ConfirmProjectIssue",
            Tags = new[] { "Project Issues" }
        )]
        public async Task<ApiResult> ConfirmProjectIssueAsync(
            [SwaggerParameter(
                Description = "ID của vấn đề cần xác nhận",
                Required = true
            )]
            Guid id)
        {
            await _projectIssueService.ConfirmProjectIssueAsync(id);
            return Ok();
        }
        
    }
}
