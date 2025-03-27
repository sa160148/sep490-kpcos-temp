using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;
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
        private readonly IFeedbackService _feedbackService;

        public MaintenancesController(IMaintenanceService maintenanceService, IFeedbackService feedbackService)
        {
            _maintenanceService = maintenanceService;
            _feedbackService = feedbackService;
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

        /// <summary>
        /// Cập nhật trạng thái công việc bảo trì
        /// </summary>
        /// <remarks>
        /// API này cho phép cập nhật trạng thái công việc bảo trì với ba chế độ khác nhau, ở các chế độ chỉ truyền 1 field.
        /// 
        /// **Chế độ 1: Phân công nhân viên (chuyển sang trạng thái PROCESSING)**
        /// - Cung cấp staffId để phân công nhân viên cho công việc
        /// - Nhân viên phải có chức vụ là CONSTRUCTOR
        /// - Nhân viên không được phân công cho các nhiệm vụ xây dựng, vấn đề dự án, hoặc nhiệm vụ bảo trì khác đang hoạt động
        /// - Trạng thái công việc bảo trì sẽ được cập nhật thành PROCESSING
        /// 
        /// **Mẫu yêu cầu chế độ 1:**
        /// 
        ///     {
        ///       "staffId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        ///     }
        /// 
        /// **Chế độ 2: Cập nhật hình ảnh (chuyển sang trạng thái PREVIEWING)**
        /// - Cung cấp imageUrl để tải lên hình ảnh hoặc tài liệu ghi nhận việc thực hiện công việc
        /// - Trạng thái công việc bảo trì sẽ được cập nhật thành PREVIEWING
        /// 
        /// **Mẫu yêu cầu chế độ 2:**
        /// 
        ///     {
        ///       "imageUrl": "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f6/Liquid_Rubber_Europe_coatings.JPG/800px-Liquid_Rubber_Europe_coatings.JPG"
        ///     }
        /// 
        /// **Chế độ 3: Cập nhật lý do (chuyển từ PREVIEWING sang PROCESSING)**
        /// - Cung cấp reason để giải thích lý do cần xử lý thêm
        /// - Trạng thái công việc bảo trì sẽ được cập nhật từ PREVIEWING sang PROCESSING
        /// 
        /// **Mẫu yêu cầu chế độ 3:**
        /// 
        ///     {
        ///       "reason": "Bộ lọc cần thay thế một số chi tiết bị hư hỏng. Cần có thêm 1-2 ngày để hoàn thành."
        ///     }
        /// 
        /// **Các tham số:**
        /// - staffId: ID người dùng của nhân viên được phân công (bắt buộc cho chế độ 1)
        /// - imageUrl: URL của hình ảnh hoàn thành công việc (bắt buộc cho chế độ 2)
        /// - reason: Lý do cần xử lý thêm (bắt buộc cho chế độ 3)
        /// - name: Tên công việc bảo trì (tùy chọn)
        /// - description: Mô tả công việc bảo trì (tùy chọn)
        /// </remarks>
        /// <param name="id">ID của công việc bảo trì cần cập nhật</param>
        /// <param name="request">Dữ liệu cập nhật cho công việc bảo trì</param>
        /// <response code="200">Cập nhật trạng thái công việc bảo trì thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc vi phạm điều kiện nghiệp vụ</response>
        /// <response code="401">Người dùng chưa đăng nhập</response>
        /// <response code="404">Không tìm thấy công việc bảo trì hoặc nhân viên</response>
        [HttpPut("tasks/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật trạng thái công việc bảo trì",
            Description = "Cập nhật trạng thái công việc bảo trì dựa trên ID của công việc, với ba chế độ: phân công nhân viên, cập nhật hình ảnh, hoặc cập nhật lý do.",
            OperationId = "UpdateMaintenanceTaskStatus",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> UpdateMaintenanceTaskStatusAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của công việc bảo trì cần cập nhật",
                Required = true
            )]
            Guid id,
            [FromBody]
            [SwaggerParameter(
                Description = "Dữ liệu cập nhật cho công việc bảo trì theo một trong ba chế độ (phân công nhân viên, cập nhật hình ảnh, hoặc cập nhật lý do)",
                Required = true
            )]
            CommandMaintenanceRequestTaskRequest request)
        {
            await _maintenanceService.UpdateMaintenanceTaskStatusAsync(id, request);
            return Ok();
        }

        /// <summary>
        /// Xác nhận hoàn thành công việc bảo trì
        /// </summary>
        /// <remarks>
        /// API này cho phép xác nhận hoàn thành một công việc bảo trì, chuyển trạng thái từ PREVIEWING sang DONE.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Công việc bảo trì phải đang ở trạng thái PREVIEWING để có thể xác nhận hoàn thành
        /// - Sau khi xác nhận, trạng thái công việc sẽ chuyển thành DONE
        /// - Hệ thống sẽ tự động kiểm tra tất cả các công việc bảo trì khác thuộc cùng yêu cầu bảo trì
        /// - Nếu tất cả các công việc bảo trì đều ở trạng thái DONE, yêu cầu bảo trì sẽ được cập nhật sang trạng thái DONE
        /// </remarks>
        /// <param name="id">ID của công việc bảo trì cần xác nhận hoàn thành</param>
        /// <response code="200">Xác nhận hoàn thành công việc bảo trì thành công</response>
        /// <response code="400">Công việc bảo trì không ở trạng thái PREVIEWING</response>
        /// <response code="401">Người dùng chưa đăng nhập</response>
        /// <response code="404">Không tìm thấy công việc bảo trì</response>
        [HttpPost("tasks/{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Xác nhận hoàn thành công việc bảo trì",
            Description = "Xác nhận hoàn thành công việc bảo trì, chuyển trạng thái từ PREVIEWING sang DONE và kiểm tra cập nhật trạng thái yêu cầu bảo trì nếu cần.",
            OperationId = "ConfirmMaintenanceTask",
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

        [HttpGet("tasks/{id}")]
        [ProducesResponseType(typeof(ApiResult<GetAllMaintenanceRequestTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy chi tiết công việc bảo trì",
            Description = "Lấy chi tiết công việc bảo trì dựa trên ID của công việc",
            OperationId = "GetMaintenanceTask",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult<GetAllMaintenanceRequestTaskResponse>> GetMaintenanceTaskAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của công việc bảo trì cần lấy chi tiết",
                Required = true
            )]
            Guid id)
        {
            var result = await _maintenanceService.GetMaintenanceTaskAsync(id);
            return Ok(result);
        }
        
        [HttpGet("task")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy danh sách công việc bảo trì",    
            Description = "Lấy danh sách công việc bảo trì dựa trên ID của yêu cầu bảo trì",    
            OperationId = "GetMaintenanceTasks",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<PagedApiResponse<GetAllMaintenanceRequestTaskResponse>> GetAllMaintenanceRequestTasksAsync(
            [FromQuery]
            GetAllMaintenanceRequestTaskFilterRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            (IEnumerable<GetAllMaintenanceRequestTaskResponse> data, int total) result;
            
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                result = await _maintenanceService.GetAllMaintenanceRequestTasksAsync(request, userId);
            }
            else
            {
                result = await _maintenanceService.GetAllMaintenanceRequestTasksAsync(request);
            }
            
            return new PagedApiResponse<GetAllMaintenanceRequestTaskResponse>(
                result.data, 
                request.PageNumber, 
                request.PageSize, 
                result.total);
        }

        [HttpGet("{id}/staff")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy danh sách nhân viên",
            Description = "Lấy danh sách nhân viên dựa trên ID của yêu cầu bảo trì",
            OperationId = "GetStaffs",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<PagedApiResponse<GetAllStaffResponse>> GetStaffsAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của yêu cầu bảo trì(maintenanceRequest) cần lấy danh sách nhân viên",
                Required = true
            )]
            Guid id,
            [FromQuery]
            GetAllStaffRequest request)
        {
            var result = await _maintenanceService.GetStaffsAsync(request, id);
            return new PagedApiResponse<GetAllStaffResponse>(
                result.data, 
                request.PageNumber, 
                request.PageSize, 
                result.total);
        }


        [HttpPut("{id}/staff")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật danh sách nhân viên",
            Description = "Cập nhật danh sách nhân viên cho yêu cầu bảo trì",
            OperationId = "UpdateStaffs",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> AssignStaffsAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của yêu cầu bảo trì(maintenanceRequest) cần cập nhật danh sách nhân viên",
                Required = true
            )]
            Guid id,
            [FromBody]
            [SwaggerParameter(
                Description = "Danh sách nhân viên cần cập nhật cho yêu cầu bảo trì(maintenanceRequest)",
                Required = true
            )]
            CommandMaintenanceRequestTaskRequest request)
        {
            await _maintenanceService.AssignStaffsAsync(id, request);
            return Ok();
        }   
    }
}
