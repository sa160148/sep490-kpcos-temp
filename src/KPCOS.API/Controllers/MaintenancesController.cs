using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class MaintenancesController : BaseController
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenancesController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        /// <summary>
        /// Tạo yêu cầu bảo trì mới
        /// </summary>
        /// <remarks>
        /// API này cho phép tạo yêu cầu bảo trì mới cho hồ/bể cá của khách hàng.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Khách hàng phải đăng nhập để sử dụng API này
        /// - Yêu cầu bảo trì mới được tạo với trạng thái OPENING
        /// - Nếu loại bảo trì là UNSCHEDULED, số lượng công việc bảo trì không được vượt quá 2
        /// - Ngày bảo trì sẽ được tự động sắp xếp để tránh cuối tuần và ngày lễ
        /// - Giá dịch vụ được tính dựa trên diện tích, độ sâu và gói bảo trì
        /// - Có thể áp dụng giảm giá theo nhóm tháng (6 tháng và 12 tháng)
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     {
        ///       "maintenancePackageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Bảo trì hồ cá Koi",
        ///       "area": 15.5,
        ///       "depth": 1.2,
        ///       "address": "123 Đường Lê Lợi, Quận 1, TP.HCM",
        ///       "type": "SCHEDULED",
        ///       "duration": 6,
        ///       "estimateAt": "2024-08-15",
        ///       "totalValue": null
        ///     }
        /// 
        /// **Các tham số:**
        /// - maintenancePackageId: ID của gói bảo trì (bắt buộc)
        /// - name: Tên yêu cầu bảo trì (bắt buộc)
        /// - area: Diện tích hồ/bể cá tính bằng m² (bắt buộc)
        /// - depth: Độ sâu hồ/bể cá tính bằng m (bắt buộc)
        /// - address: Địa chỉ nơi thực hiện bảo trì (bắt buộc)
        /// - type: Loại bảo trì (SCHEDULED, UNSCHEDULED hoặc OTHER) (bắt buộc)
        /// - duration: Số lượng công việc bảo trì (tháng) (tùy chọn, mặc định là 1)
        /// - estimateAt: Ngày dự kiến bắt đầu bảo trì (tùy chọn)
        /// - totalValue: Tổng giá trị của yêu cầu bảo trì (tùy chọn, sẽ được tính tự động nếu không cung cấp)
        /// </remarks>
        /// <param name="request">Thông tin chi tiết về yêu cầu bảo trì</param>
        /// <response code="200">Yêu cầu bảo trì được tạo thành công</response>
        /// <response code="400">Thông tin yêu cầu không hợp lệ (thiếu thông tin bắt buộc, type=UNSCHEDULED nhưng duration > 2, v.v.)</response>
        /// <response code="401">Người dùng chưa đăng nhập</response>
        /// <response code="404">Không tìm thấy gói bảo trì hoặc khách hàng</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Tạo yêu cầu bảo trì mới",
            Description = "API này cho phép tạo yêu cầu bảo trì mới cho hồ/bể cá của khách hàng, với các tùy chọn như loại bảo trì, thời hạn, và ngày dự kiến.",
            OperationId = "CreateMaintenanceRequest",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> CreateMaintenanceRequestAsync(
            [SwaggerParameter(
                Description = "Thông tin chi tiết về yêu cầu bảo trì mới, bao gồm gói bảo trì, thông số hồ/bể, địa chỉ, loại bảo trì và các thông tin khác",
                Required = true
            )]
            CommandMaintenanceRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Customer chưa đăng nhập");
            }

            var userId = Guid.Parse(userIdClaim);

            await _maintenanceService.CreateMaintenanceRequestAsync(request, userId);
            return Ok();
        }

        /// <summary>
        /// Lấy danh sách các yêu cầu bảo trì theo bộ lọc
        /// </summary>
        /// <remarks>
        /// API này cho phép lấy danh sách yêu cầu bảo trì với phân trang và lọc theo nhiều tiêu chí.
        /// 
        /// **Các tham số lọc:**
        /// - Search: Tìm kiếm theo tên hoặc địa chỉ
        /// - IsActive: Lọc theo trạng thái hoạt động (true/false)
        /// - Status: Lọc theo trạng thái yêu cầu (OPENING, PROCESSING, DONE)
        /// - IsPaid: Lọc theo trạng thái thanh toán (true/false)
        /// - CustomerId: Lọc theo ID khách hàng
        /// - Type: Lọc theo loại bảo trì (SCHEDULED, UNSCHEDULED, OTHER)
        /// - MaintenancePackageId: Lọc theo ID gói bảo trì
        /// - PageNumber: Số trang (bắt đầu từ 1)
        /// - PageSize: Số lượng phần tử trên mỗi trang
        /// - SortColumn: Cột sắp xếp (mặc định: CreatedAt)
        /// - SortDir: Hướng sắp xếp (Asc hoặc Desc, mặc định: Desc)
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     GET /api/maintenances?Search=Koi&amp;IsActive=true&amp;Status=OPENING&amp;IsPaid=false&amp;Type=SCHEDULED&amp;PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <param name="request">Các tham số lọc</param>
        /// <returns>Danh sách các yêu cầu bảo trì theo bộ lọc với phân trang</returns>
        /// <response code="200">Trả về danh sách các yêu cầu bảo trì</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllMaintenanceRequestResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách các yêu cầu bảo trì theo bộ lọc",
            Description = "Truy vấn danh sách yêu cầu bảo trì dựa trên các tiêu chí lọc như trạng thái, loại bảo trì, gói bảo trì và phân trang.",
            OperationId = "GetMaintenanceRequests",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<PagedApiResponse<GetAllMaintenanceRequestResponse>> GetMaintenanceRequestsAsync(
            [FromQuery] 
            [SwaggerParameter(
                Description = "Các tham số lọc bao gồm Search, IsActive, Status, IsPaid, CustomerId, Type, MaintenancePackageId, PageNumber, PageSize, SortColumn, và SortDir",
                Required = false
            )]
            GetAllMaintenanceRequestFilterRequest request)
        {
            var result = await _maintenanceService.GetMaintenanceRequestsAsync(request);
            return new PagedApiResponse<GetAllMaintenanceRequestResponse>(
            result.data, 
            request.PageNumber, 
            request.PageSize, 
            result.total);
        }

        [HttpPut("tasks/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật trạng thái công việc bảo trì",
            Description = "Cập nhật trạng thái công việc bảo trì dựa trên ID của công việc.",
            OperationId = "UpdateMaintenanceTaskStatus",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> UpdateMaintenanceTaskStatusAsync(
            [FromRoute] Guid id,
            [FromBody] CommandMaintenanceRequestTaskRequest request)
        {
            await _maintenanceService.UpdateMaintenanceTaskStatusAsync(id, request);
            return Ok();
        }

        [HttpPost("tasks/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Tạo công việc bảo trì",
            Description = "Tạo công việc bảo trì dựa trên ID của yêu cầu bảo trì.",
            OperationId = "CreateMaintenanceTask",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> ConfirmMaintenanceTaskAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của công việc bảo trì cần xác nhận hoàn thành",
                Required = true
            )]
            Guid id)
        {
            await _maintenanceService.ConfirmMaintenanceTaskAsync(id);
            return Ok();
        }

    }
}
