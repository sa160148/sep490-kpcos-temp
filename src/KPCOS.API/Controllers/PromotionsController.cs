using KPCOS.BusinessLayer.DTOs.Request.Promotions;
using KPCOS.BusinessLayer.DTOs.Response.Promotions;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    /// <summary>
    /// Controller quản lý khuyến mãi cho dịch vụ xây dựng và bảo trì hồ cá Koi
    /// </summary>
    [Route("api/[controller]")]
    public class PromotionsController : BaseController
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>
        /// Lấy danh sách khuyến mãi có phân trang và lọc tùy chọn
        /// </summary>
        /// <param name="filter">Thông số lọc và phân trang</param>
        /// <returns>Danh sách khuyến mãi đã phân trang</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách khuyến mãi",
            Description = "Lấy danh sách khuyến mãi theo trang và kích thước trang với các tùy chọn lọc"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Danh sách khuyến mãi", typeof(PagedApiResponse<GetAllPromotionResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Lỗi yêu cầu không hợp lệ")]
        public async Task<PagedApiResponse<GetAllPromotionResponse>> GetAllPromotions(
            [FromQuery] GetAllPromotionFilterRequest filter
        )
        {
            var promotions = await _promotionService.GetAllPromotions(filter);
            return new PagedApiResponse<GetAllPromotionResponse>(
                promotions.data, 
                promotions.total, 
                filter.PageNumber, 
                filter.PageSize);
        }

        /// <summary>
        /// Tạo mới khuyến mãi cho dịch vụ hồ cá Koi
        /// </summary>
        /// <param name="request">Thông tin chi tiết khuyến mãi cần tạo</param>
        /// <returns>Kết quả thành công</returns>
        /// <remarks>
        /// Mẫu yêu cầu:
        /// ```json
        /// {
        ///   "name": "Khuyến mãi hè 2023",
        ///   "code": "KM2023HE",
        ///   "discount": 15,
        ///   "startAt": "2023-06-01T00:00:00",
        ///   "expiredAt": "2023-08-31T23:59:59",
        ///   "description": "Khuyến mãi 15% cho tất cả dịch vụ xây dựng và bảo trì hồ cá Koi trong mùa hè"
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo khuyến mãi",
            Description = "Tạo khuyến mãi mới với mã tự động nếu không được cung cấp. Trạng thái được xác định dựa trên ngày bắt đầu và kết thúc."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Khuyến mãi đã được tạo thành công")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Lỗi yêu cầu không hợp lệ")]
        public async Task<ApiResult> CreatePromotion(
            [FromBody] CommandPromotionRequest request
        )
        {
            await _promotionService.CreatePromotion(request);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin khuyến mãi
        /// </summary>
        /// <param name="id">ID khuyến mãi</param>
        /// <param name="request">Thông tin cập nhật khuyến mãi</param>
        /// <returns>Kết quả thành công</returns>
        /// <remarks>
        /// Mẫu yêu cầu:
        /// ```json
        /// {
        ///   "name": "Khuyến mãi thu đông 2023",
        ///   "code": "KM2023THU",
        ///   "discount": 20,
        ///   "startAt": "2023-09-01T00:00:00",
        ///   "expiredAt": "2023-12-31T23:59:59",
        ///   "description": "Khuyến mãi 20% cho dịch vụ bảo trì hồ cá Koi mùa thu đông"
        /// }
        /// ```
        /// </remarks>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Cập nhật khuyến mãi",
            Description = "Cập nhật khuyến mãi theo id. Trạng thái sẽ được tự động cập nhật dựa trên ngày bắt đầu và kết thúc nếu có."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Khuyến mãi đã được cập nhật thành công")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Lỗi yêu cầu không hợp lệ")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Không tìm thấy khuyến mãi")]
        public async Task<ApiResult> UpdatePromotion(
            [FromRoute] Guid id,
            [FromBody] CommandPromotionRequest request
        )
        {
            await _promotionService.UpdatePromotion(id, request);
            return Ok();
        }

        /// <summary>
        /// Xóa khuyến mãi hoặc đặt trạng thái không hoạt động nếu đang được sử dụng
        /// </summary>
        /// <param name="id">ID khuyến mãi</param>
        /// <returns>Kết quả thành công</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Xóa khuyến mãi",
            Description = "Xóa khuyến mãi theo id, cài đặt is_active = false nếu khuyến mãi đã được sử dụng trong báo giá, hoặc xóa hoàn toàn nếu chưa có báo giá nào sử dụng"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Khuyến mãi đã được xóa thành công")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Lỗi yêu cầu không hợp lệ")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Không tìm thấy khuyến mãi")]
        public async Task<ApiResult> DeletePromotion(
            [FromRoute] Guid id
        )
        {
            await _promotionService.DeletePromotion(id);
            return Ok();
        }

        /// <summary>
        /// Lấy thông tin chi tiết khuyến mãi theo ID
        /// </summary>
        /// <param name="id">ID khuyến mãi</param>
        /// <returns>Thông tin chi tiết khuyến mãi</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Lấy khuyến mãi theo id",
            Description = "Lấy thông tin chi tiết của khuyến mãi theo id"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Khuyến mãi", typeof(GetAllPromotionResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Lỗi yêu cầu không hợp lệ")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Không tìm thấy khuyến mãi")]
        public async Task<ApiResult<GetAllPromotionResponse>> GetPromotionById(
            [FromRoute] Guid id
        )
        {
            var promotion = await _promotionService.GetPromotionById(id);
            return Ok(promotion);
        }
    }
}
