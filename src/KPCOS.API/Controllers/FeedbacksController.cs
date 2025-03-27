using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Feedbacks;
using KPCOS.BusinessLayer.DTOs.Response.Feedbacks;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class FeedbacksController : BaseController
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Tạo đánh giá mới cho dự án hoặc yêu cầu bảo trì
        /// </summary>
        /// <remarks>
        /// API này cho phép khách hàng tạo đánh giá cho dự án hoặc yêu cầu bảo trì của họ.
        /// Mỗi dự án hoặc yêu cầu bảo trì chỉ có thể có một đánh giá từ một khách hàng.
        /// **Điểm đánh giá là bắt buộc(0-5)**, và tên có thể được tạo tự động nếu không cung cấp.
        /// 
        /// Ví dụ yêu cầu:
        /// 
        ///     POST /api/feedbacks
        ///     {
        ///       "no": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Đánh giá tuyệt vời về hồ cá Koi",
        ///       "description": "Đội ngũ đã làm rất tốt với hồ cá Koi của tôi. Hệ thống lọc nước hoạt động hoàn hảo, và bố trí đá trông tự nhiên và đẹp. Cá đang phát triển tốt trong môi trường mới!",
        ///       "imageUrl": "https://example.com/my-koi-pond.jpg",
        ///       "rating": 5,
        ///       "type": "PROJECT"
        ///     }
        /// 
        /// </remarks>
        /// <param name="request">Chi tiết đánh giá bao gồm No (ID tham chiếu), điểm đánh giá và các trường tùy chọn</param>
        /// <response code="200">Đánh giá được tạo thành công</response>
        /// <response code="400">Đầu vào không hợp lệ, thiếu trường bắt buộc, hoặc đánh giá đã tồn tại</response>
        /// <response code="401">Người dùng chưa xác thực</response>
        /// <response code="404">Không tìm thấy dự án hoặc yêu cầu bảo trì</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost]
        [CustomAuthorize]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Tạo đánh giá mới",
            Description = "Tạo đánh giá cho dự án hoặc yêu cầu bảo trì với điểm đánh giá và chi tiết tùy chọn"
        )]
        public async Task<ApiResult> CreateFeedbackAsync(CommandFeedbackRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập");
            }

            await _feedbackService.CreateFeedbackAsync(request, Guid.Parse(userIdClaim));
            return Ok();
        }

        /// <summary>
        /// Cập nhật đánh giá hiện có
        /// </summary>
        /// <remarks>
        /// API này cho phép khách hàng cập nhật đánh giá hiện có của họ.
        /// Chỉ tên, mô tả, **điểm đánh giá(0-5)** và hình ảnh URL có thể được cập nhật.
        /// ID tham chiếu (No) và Loại không thể thay đổi.
        /// 
        /// Ví dụ yêu cầu:
        /// 
        ///     PUT /api/feedbacks/{id}
        ///     {
        ///       "name": "Cập nhật đánh giá - Dự án hồ cá Koi",
        ///       "description": "Sau khi sử dụng hồ trong vài tháng, tôi thậm chí còn ấn tượng hơn với chất lượng công việc. Hệ sinh thái cân bằng và yêu cầu bảo trì tối thiểu. Đội ngũ đã phản hồi nhanh chóng mọi câu hỏi của tôi.",
        ///       "imageUrl": "https://example.com/koi-pond-update.jpg",
        ///       "rating": 5
        ///     }
        /// 
        /// </remarks>
        /// <param name="id">ID của đánh giá cần cập nhật</param>
        /// <param name="request">Chi tiết đánh giá cập nhật</param>
        /// <response code="200">Đánh giá được cập nhật thành công</response>
        /// <response code="400">Đầu vào không hợp lệ</response>
        /// <response code="401">Người dùng chưa xác thực</response>
        /// <response code="404">Không tìm thấy đánh giá</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPut("{id}")]
        [CustomAuthorize]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Cập nhật đánh giá hiện có",
            Description = "Cập nhật tên, mô tả, điểm đánh giá hoặc URL hình ảnh của đánh giá"
        )]
        public async Task<ApiResult> UpdateFeedbackAsync(
            [SwaggerParameter(
                Description = "ID của đánh giá cần cập nhật",
                Required = true
            )]
            [FromRoute]
            Guid id,
            [FromBody]
            CommandFeedbackRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập");
            }

            await _feedbackService.UpdateFeedbackAsync(id, request, Guid.Parse(userIdClaim));
            return Ok();
        }
        
        /// <summary>
        /// Lấy danh sách đánh giá với phân trang và các tùy chọn lọc
        /// </summary>
        /// <remarks>
        /// API này lấy danh sách đánh giá với phân trang và các tùy chọn lọc.
        /// Đối với mỗi đánh giá, thông tin khách hàng liên quan và chi tiết dự án/yêu cầu bảo trì được bao gồm.
        /// 
        /// Ví dụ yêu cầu:
        /// 
        ///     GET /api/feedbacks?Type=PROJECT&amp;Rating=5&amp;PageNumber=1&amp;PageSize=10
        /// 
        /// Tùy chọn lọc:
        /// - Search: Lọc theo tên đánh giá
        /// - Type: Lọc theo loại đánh giá (PROJECT hoặc MAINTENANCE)
        /// - No: Lọc theo ID dự án hoặc yêu cầu bảo trì liên quan
        /// - Rating: Lọc theo giá trị đánh giá
        /// - FromCreatedAt/ToCreatedAt: Lọc theo khoảng thời gian tạo
        /// - PageNumber/PageSize: Thông số phân trang
        /// </remarks>
        /// <param name="request">Thông số lọc và phân trang</param>
        /// <response code="200">Danh sách đánh giá với metadata phân trang</response>
        /// <response code="401">Người dùng chưa xác thực</response>
        /// <response code="404">Không tìm thấy khách hàng</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet]
        [CustomAuthorize]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllFeedbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Lấy danh sách đánh giá có phân trang",
            Description = "Lấy đánh giá với lọc theo loại, điểm đánh giá, khoảng thời gian và tìm kiếm văn bản. Nếu người dùng đã đăng nhập với vai trò khách hàng, chỉ trả về đánh giá của khách hàng đó."
        )]
        public async Task<PagedApiResponse<GetAllFeedbackResponse>> GetAllFeedbackAsync(
            [FromQuery] 
            [SwaggerParameter(
                Description = "Tùy chọn lọc bao gồm Search, Type, No, Rating, FromCreatedAt, ToCreatedAt và thông số phân trang",
                Required = false
            )]
            GetAllFeedbackFilterRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllFeedbackResponse> data, int total) result;
            
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                result = await _feedbackService.GetAllFeedbackAsync(request, userId);
            }
            result = await _feedbackService.GetAllFeedbackAsync(request);
            return new PagedApiResponse<GetAllFeedbackResponse>(
                result.data, 
                request.PageNumber, 
                request.PageSize, 
                result.total);
        }
    }
}
