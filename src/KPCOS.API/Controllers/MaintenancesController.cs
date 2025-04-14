using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.MaintenanceRequestIssues;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.MaintenanceRequestIssues;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;
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
        /// - Yêu cầu bảo trì thông thường được tạo với trạng thái OPENING
        /// - Yêu cầu bảo trì sau dự án (totalValue = 0) được tạo với trạng thái REQUESTING và tên có thêm "bảo trì/bảo dưỡng sau dự án"
        /// - Nếu loại bảo trì là UNSCHEDULED, số lượng công việc bảo trì không được vượt quá 2
        /// - Ngày bảo trì sẽ được tự động sắp xếp để tránh cuối tuần và ngày lễ
        /// - Giá dịch vụ được tính dựa trên diện tích, độ sâu và gói bảo trì
        /// - Có thể áp dụng giảm giá theo nhóm tháng (6 tháng và 12 tháng)
        /// 
        /// **Mẫu yêu cầu thông thường:**
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
        /// **Mẫu yêu cầu bảo trì sau dự án:**
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
        ///       "totalValue": 0
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
        /// - totalValue: Tổng giá trị của yêu cầu bảo trì (tùy chọn)
        ///   - null: giá trị sẽ được tính tự động dựa trên diện tích, độ sâu và gói bảo trì
        ///   - 0: đánh dấu là yêu cầu bảo trì sau dự án, trạng thái sẽ là REQUESTING
        ///   - khác 0: sử dụng giá trị được cung cấp
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
        ///
        /// **Phản hồi:**
        /// - Bao gồm danh sách nhân viên (Staffs) được phân công cho mỗi công việc bảo trì cấp 1 (level 1 tasks)
        /// - Công việc bảo trì cấp 2 (level 2 tasks) chỉ có một nhân viên (Staff) được phân công
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Customer chưa đăng nhập");
            }
            var userId = Guid.Parse(userIdClaim);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            request.UserId = userId;
            request.Role = role;

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
        /// - Nhân viên không được phân công cho công việc bảo trì cấp 1 từ các yêu cầu bảo trì khác đang hoạt động (các công việc chưa DONE)
        /// - Nhân viên không được phân công cho các dự án đang trong giai đoạn thi công (CONSTRUCTING)
        /// - Nhân viên không được phân công cho các công việc xây dựng đang hoạt động (các công việc chưa DONE)
        /// - Nhân viên không được phân công cho các vấn đề dự án đang hoạt động (các vấn đề chưa DONE)
        /// - Đối với công việc cấp 2 (Level 2), nhân viên phải được phân công cho công việc cấp 1 (công việc cha) trước
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
        /// - Công việc bảo trì phải đang ở trạng thái PROCESSING
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
        /// - Công việc bảo trì phải đang ở trạng thái PREVIEWING
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
        /// <response code="400">Dữ liệu không hợp lệ hoặc vi phạm điều kiện nghiệp vụ: nhân viên đã được phân công vào công việc bảo trì khác, nhân viên đang tham gia dự án đang thi công, v.v.</response>
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
            Description = "Lấy chi tiết công việc bảo trì dựa trên ID của công việc. Đối với công việc bảo trì cấp 1 (Level 1), phản hồi sẽ bao gồm danh sách toàn bộ nhân viên (trường Staffs) được phân công và danh sách công việc con (trường Childs). Đối với công việc bảo trì cấp 2 (Level 2), phản hồi sẽ bao gồm thông tin nhân viên được phân công trực tiếp (trường Staff).",
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
        [ProducesResponseType(typeof(PagedApiResponse<GetAllMaintenanceRequestTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy danh sách công việc bảo trì",    
            Description = "Lấy danh sách công việc bảo trì dựa trên các tiêu chí lọc. Hỗ trợ việc lọc theo yêu cầu bảo trì (MaintenanceRequestId), trạng thái (Status), và nhiều tiêu chí khác.\n\n" + 
                          "Đối với công việc bảo trì cấp 1 (Level 1), phản hồi sẽ bao gồm danh sách toàn bộ nhân viên (trường Staffs) được phân công và danh sách công việc con (trường Childs). " + 
                          "Đối với công việc bảo trì cấp 2 (Level 2), phản hồi sẽ bao gồm thông tin nhân viên được phân công trực tiếp (trường Staff).\n\n" +
                          "Nếu người dùng đã đăng nhập, API sẽ tự động lọc danh sách công việc bảo trì dựa trên vai trò của người dùng: khách hàng sẽ chỉ thấy công việc thuộc yêu cầu bảo trì của họ, nhân viên xây dựng sẽ chỉ thấy công việc được phân công cho họ.",
            OperationId = "GetMaintenanceTasks",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<PagedApiResponse<GetAllMaintenanceRequestTaskResponse>> GetAllMaintenanceRequestTasksAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Các tham số lọc bao gồm MaintenanceRequestId, Status, IsChild, ParentId, From, To, Search, PageNumber, PageSize, SortColumn, và SortDir",
                Required = false
            )]
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
            Summary = "[Deprecated] Cập nhật danh sách nhân viên",
            Description = "DEPRECATED: Sử dụng API PUT /api/maintenances/tasks/{id} với staffId thay thế. API này phân công một nhân viên cho tất cả các công việc bảo trì cấp 1 của một yêu cầu bảo trì. Lưu ý API này sẽ bị loại bỏ trong phiên bản tương lai.",
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
                Description = "Thông tin nhân viên cần được phân công cho tất cả công việc bảo trì cấp 1 của yêu cầu bảo trì. Chỉ cần truyền trường staffId.",
                Required = true
            )]
            CommandMaintenanceRequestTaskRequest request)
        {
            // NOTE: This API is deprecated. Use PUT /api/maintenances/tasks/{id} with staffId instead.
            await _maintenanceService.AssignStaffsAsync(id, request);
            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResult<GetAllMaintenanceRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy chi tiết yêu cầu bảo trì",
            Description = "Lấy chi tiết yêu cầu bảo trì dựa trên ID của yêu cầu bảo trì. Phản hồi bao gồm thông tin đầy đủ về yêu cầu bảo trì, gói bảo trì, khách hàng, danh sách đánh giá phản hồi, và cấu trúc phân cấp đầy đủ của các công việc bảo trì.\n\n" +
                          "Cấu trúc phản hồi bao gồm:\n" +
                          "- Thông tin yêu cầu bảo trì: ID, tên, diện tích, độ sâu, địa chỉ, tổng giá trị, trạng thái, v.v.\n" +
                          "- Thông tin gói bảo trì (MaintenancePackage): ID, tên, mô tả, giá, v.v.\n" +
                          "- Thông tin khách hàng (Customer): ID, tên, email, v.v.\n" +
                          "- Danh sách công việc bảo trì cấp 1 (MaintenanceRequestTasks): bao gồm danh sách nhân viên được phân công (Staffs) cho mỗi công việc cấp 1.\n" +
                          "- Danh sách công việc bảo trì cấp 2 (Childs): mỗi công việc cấp 1 bao gồm danh sách các công việc cấp 2 và nhân viên được phân công (Staff) cho mỗi công việc cấp 2.\n" +
                          "- Danh sách đánh giá phản hồi (Feedbacks): các đánh giá từ khách hàng về yêu cầu bảo trì.",
            OperationId = "GetMaintenanceRequest",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult<GetAllMaintenanceRequestResponse>> GetDetailMaintenanceRequestAsync(
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của yêu cầu bảo trì(maintenanceRequest) cần lấy chi tiết",
                Required = true
            )]
            Guid id)
        {
            var result = await _maintenanceService.GetDetailMaintenanceRequestAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo bảo trì/bảo dưỡng bất thường mới cho yêu cầu bảo trì/bảo dưỡng
        /// </summary>
        /// <param name="request">Thông tin bảo trì/bảo dưỡng bất thường cần tạo</param>
        /// <returns>Thông báo thành công nếu tạo bảo trì/bảo dưỡng bất thường thành công</returns>
        /// <remarks>
        /// API này tạo một bảo trì/bảo dưỡng bất thường mới cho yêu cầu bảo trì/bảo dưỡng với các thông tin cần thiết.
        /// - Yêu cầu bắt buộc: Tên (Name), Mô tả (Description), ID yêu cầu bảo trì/bảo dưỡng (MaintenanceRequestId), Ngày dự kiến (EstimateAt)
        /// - Nguyên nhân (Cause) là tùy chọn
        /// - Trạng thái (Status) mặc định là OPENING
        /// - Ngày dự kiến phải nằm trong khoảng thời gian giữa công việc bảo trì/bảo dưỡng đầu tiên và cuối cùng
        /// - Ngày dự kiến không được trùng với các công việc bảo trì/bảo dưỡng hiện có
        /// - Ngày dự kiến không rơi vào ngày cuối tuần (sẽ được tự động chuyển sang ngày làm việc tiếp theo)
        /// - Không thể tạo bảo trì/bảo dưỡng bất thường cho yêu cầu bảo trì/bảo dưỡng đã hoàn thành (DONE)
        /// 
        /// Mẫu yêu cầu:
        /// 
        ///     {
        ///         "maintenanceRequestId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        ///         "cause": "Máy bơm nước bị kẹt và phát ra tiếng ồn bất thường",
        ///         "description": "Khi bật máy bơm, có tiếng ồn lớn và nước không được bơm lên. Máy bơm có thể bị kẹt do cặn bẩn hoặc rong rêu trong hồ.",
        ///         "estimateAt": "2025-04-17",
        ///         "name": "Kiểm tra máy bơm nước",
        ///         "issueImage": "https://example.com/images/pump_issue.jpg"
        ///     }
        /// </remarks>
        [HttpPost("issue")]
        [SwaggerOperation(
            Summary = "Tạo bảo trì/bảo dưỡng bất thường mới cho yêu cầu bảo trì/bảo dưỡng",
            Description = "Tạo bảo trì/bảo dưỡng bất thường mới cho yêu cầu bảo trì/bảo dưỡng với các thông tin cần thiết như nguyên nhân, mô tả và ngày dự kiến xử lý.",
            OperationId = "CreateMaintenanceRequestIssue",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult> CreateMaintenanceRequestIssueAsync(
            [FromBody]
            CommandMaintenanceRequestIssueRequest request)
        {
            await _maintenanceService.CreateMaintenanceRequestIssueAsync(request);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin và trạng thái vấn đề bảo trì/bảo dưỡng bất thường
        /// </summary>
        /// <param name="request">Thông tin cập nhật của vấn đề bảo trì/bảo dưỡng</param>
        /// <param name="id">ID của vấn đề bảo trì/bảo dưỡng cần cập nhật</param>
        /// <returns>Thông báo thành công nếu cập nhật vấn đề bảo trì/bảo dưỡng thành công</returns>
        /// <remarks>
        /// API này cập nhật thông tin và trạng thái vấn đề bảo trì/bảo dưỡng bất thường. Có 7 trường hợp cập nhật:
        /// 
        /// **1. Phân công nhân viên (OPENING → PROCESSING)**
        /// - Khi staffId có giá trị, hệ thống sẽ chuyển trạng thái từ OPENING thành PROCESSING
        /// - Các trường khác sẽ được bỏ qua
        /// 
        /// **2. Tải lên ảnh xác nhận (PROCESSING → PREVIEWING)**
        /// - Khi confirmImage có giá trị, hệ thống sẽ chuyển trạng thái từ PROCESSING thành PREVIEWING
        /// - Các trường khác sẽ được bỏ qua
        /// 
        /// **3. Từ chối ảnh xác nhận (PREVIEWING → PROCESSING)**
        /// - Khi reason có giá trị, hệ thống sẽ chuyển trạng thái từ PREVIEWING thành PROCESSING
        /// - Các trường khác sẽ được bỏ qua
        /// 
        /// **4. Giải quyết nhanh (Bất kỳ trạng thái nào ngoại trừ CANCELLED → DONE)**
        /// - Khi solution có giá trị và staffId, confirmImage, reason đều không có giá trị
        /// - Dùng cho các trường hợp khách hàng có thể tự thực hiện bảo trì/bảo dưỡng theo hướng dẫn
        /// - Hệ thống sẽ chuyển trạng thái thành DONE từ bất kỳ trạng thái nào ngoại trừ CANCELLED
        /// - Các trường khác sẽ được bỏ qua
        /// 
        /// **5. Cập nhật thông thường (Không thay đổi trạng thái)**
        /// - Chỉ cập nhật issueImage, description hoặc cause, solution
        /// - Trạng thái không thay đổi
        /// - Các trường khác sẽ được bỏ qua
        /// 
        /// **6. Hủy vấn đề (Bất kỳ trạng thái → CANCELLED)**
        /// - Khi status có giá trị CANCELLED
        /// - Dùng khi khách hàng muốn hủy vấn đề bảo trì/bảo dưỡng
        /// - Hệ thống sẽ cập nhật trạng thái thành CANCELLED
        /// - Trường reason là tùy chọn để giải thích lý do hủy
        /// 
        /// **7. Xác nhận hoàn thành (PREVIEWING → DONE)**
        /// - Khi status có giá trị DONE và trạng thái hiện tại là PREVIEWING thì chuyển trạng thái thành DONE. Hệ thống sẽ tự động kiểm tra tất cả các vấn đề và công việc bảo trì thuộc cùng yêu cầu bảo trì, nếu tất cả đã hoàn thành thì yêu cầu bảo trì sẽ được cập nhật thành trạng thái DONE.
        /// 
        /// **Lưu ý:** Mỗi trạng thái chỉ có thể chuyển đổi theo quy tắc được định nghĩa. Nếu trạng thái hiện tại không phù hợp, API sẽ trả về lỗi.
        /// 
        /// **Ví dụ 1: Administrator phân công nhân viên (OPENING -> PROCESSING):**
        /// ```json
        /// {
        ///   "staffId": "a8b4c7e9-5f12-4d36-8b7a-1c3e9d2f8e0a"
        /// }
        /// ```
        /// 
        /// **Ví dụ 2: Constructor tải lên ảnh xác nhận (PROCESSING -> PREVIEWING):**
        /// ```json
        /// {
        ///   "confirmImage": "https://upload.wikimedia.org/wikipedia/commons/1/17/Wallacepond.jpg"
        /// }
        /// ```
        /// 
        /// **Ví dụ 3: Administrator từ chối ảnh xác nhận (PREVIEWING -> PROCESSING):**
        /// ```json
        /// {
        ///   "reason": "Ảnh không rõ nét, không thể xác nhận việc vệ sinh hồ cá đã hoàn thành. Vui lòng chụp lại ảnh toàn cảnh hồ sau khi làm sạch."
        /// }
        /// ```
        /// 
        /// **Ví dụ 4: Administrator giải quyết nhanh (Bất kỳ trạng thái nào ngoại trừ CANCELLED -> DONE):**
        /// ```json
        /// {
        ///   "solution": "Hướng dẫn khách hàng tự vệ sinh bộ lọc: (1) Tắt hệ thống lọc, (2) Tháo rời bộ lọc, (3) Rửa sạch bằng nước, (4) Lắp lại và kiểm tra. Khách hàng đã thực hiện thành công và hồ cá đã hoạt động bình thường."
        /// }
        /// ```
        /// 
        /// **Ví dụ 5: Administrator hoặc Customer cập nhật thông thường (Không thay đổi trạng thái):**
        /// ```json
        /// {
        ///   "description": "Nước hồ cá Koi có màu xanh đục và có mùi. Đã kiểm tra các thông số nước và phát hiện tảo phát triển quá mức.",
        ///   "cause": "Hệ thống lọc hoạt động không hiệu quả và thiếu bảo trì định kỳ",
        ///   "solution": "Thực hiện bảo trì định kỳ hệ thống lọc và kiểm tra lại các thông số nước hồ cá Koi.",
        ///   "issueImage": "https://upload.wikimedia.org/wikipedia/commons/1/17/Wallacepond.jpg"
        /// }
        /// ```
        /// 
        /// **Ví dụ 6: Administrator hoặc Customer hủy vấn đề (Bất kỳ trạng thái → CANCELLED):**
        /// ```json
        /// {
        ///   "status": "CANCELLED"
        /// }
        /// ```
        /// 
        /// **Hoặc với lý do (tùy chọn):**
        /// ```json
        /// {
        ///   "status": "CANCELLED",
        ///   "reason": "Khách hàng đã đổi lịch vệ sinh hồ cá sang tháng sau do đang cải tạo khu vực xung quanh hồ."
        /// }
        /// ```
        /// 
        /// **Ví dụ 7: Administrator xác nhận hoàn thành (PREVIEWING -> DONE):**
        /// ```json
        /// {
        ///   "status": "DONE"
        /// }
        /// ```
        /// </remarks>
        [HttpPut("issue/{id}")]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin và trạng thái vấn đề bảo trì/bảo dưỡng bất thường",
            Description = "Cập nhật trạng thái vấn đề bảo trì/bảo dưỡng bất thường với các thông tin chi tiết. Có 7 trường hợp cập nhật:\n\n" +
                          "1. Phân công nhân viên: Khi staffId có giá trị thì chuyển trạng thái từ OPENING thành PROCESSING, các giá trị khác bỏ qua.\n\n" +
                          "2. Tải lên ảnh xác nhận: Khi confirmImage có giá trị thì chuyển trạng thái từ PROCESSING thành PREVIEWING, các giá trị khác bỏ qua.\n\n" +
                          "3. Từ chối ảnh xác nhận: Khi reason có giá trị thì chuyển trạng thái từ PREVIEWING thành PROCESSING, các giá trị khác bỏ qua.\n\n" +
                          "4. Giải quyết nhanh: Khi solution có giá trị và staffId, confirmImage, reason đều null thì chuyển trạng thái thành DONE từ bất kỳ trạng thái nào ngoại trừ CANCELLED, các giá trị khác bỏ qua.\n\n" +
                          "5. Cập nhật thông thường: Chỉ cập nhật issueImage, description, cause hoặc solution, các giá trị khác bỏ qua.\n\n" +
                          "6. Hủy vấn đề: Khi status có giá trị CANCELLED thì cập nhật trạng thái thành CANCELLED, các giá trị khác bỏ qua.\n\n" +
                          "7. Xác nhận hoàn thành: Khi status có giá trị DONE và trạng thái hiện tại là PREVIEWING thì chuyển trạng thái thành DONE. Hệ thống sẽ tự động kiểm tra tất cả các vấn đề và công việc bảo trì thuộc cùng yêu cầu bảo trì, nếu tất cả đã hoàn thành thì yêu cầu bảo trì sẽ được cập nhật thành trạng thái DONE.\n\n" +
                          "**Lưu ý: Mỗi trạng thái chỉ có thể chuyển đổi theo quy tắc được định nghĩa. Nếu trạng thái hiện tại không phù hợp, API sẽ trả về lỗi.**\n\n" +
                          "**Ví dụ 1 - Phân công nhân viên (OPENING -> PROCESSING):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"staffId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 2 - Tải lên ảnh xác nhận (PROCESSING -> PREVIEWING):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"confirmImage\": \"https://upload.wikimedia.org/wikipedia/commons/1/17/Wallacepond.jpg\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 3 - Từ chối ảnh xác nhận (PREVIEWING -> PROCESSING):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"reason\": \"Ảnh không rõ nét, vui lòng chụp lại\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 4 - Giải quyết nhanh (Any status except CANCELLED -> DONE):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"solution\": \"Đã sửa chữa ống nước bị rò rỉ và thay thế van điều khiển\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 5 - Cập nhật thông thường (No status change):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"description\": \"Hồ cá bị rò rỉ ở góc phía Bắc\",\n" +
                          "  \"cause\": \"Keo silicone bị lão hóa\",\n" +
                          "  \"issueImage\": \"https://upload.wikimedia.org/wikipedia/commons/1/17/Wallacepond.jpg\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 6 - Hủy vấn đề (Any status -> CANCELLED):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"status\": \"CANCELLED\",\n" +
                          "  \"reason\": \"Khách hàng đã tự khắc phục sự cố\"\n" +
                          "}\n" +
                          "```\n\n" +
                          "**Ví dụ 7 - Xác nhận hoàn thành (PREVIEWING -> DONE):**\n" +
                          "```json\n" +
                          "{\n" +
                          "  \"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n" +
                          "  \"status\": \"DONE\",\n" +
                          "  \"solution\": \"Đã thay thế bộ lọc và kiểm tra hệ thống tuần hoàn nước hoạt động bình thường\"\n" +
                          "}\n" +
                          "```",
            OperationId = "UpdateMaintenanceRequestIssue",
            Tags = new[] { "Maintenances" }
            )]
        public async Task<ApiResult> UpdateMaintenanceRequestIssueAsync(
            [FromBody]
            [SwaggerParameter(
                Description = "Thông tin cập nhật cho vấn đề bảo trì/bảo dưỡng bất thường dựa trên một trong năm trường hợp",
                Required = true
            )]
            CommandMaintenanceRequestIssueRequest request,
            [FromRoute]
            [SwaggerParameter(
                Description = "ID của vấn đề bảo trì/bảo dưỡng bất thường cần cập nhật",
                Required = true
            )]
            Guid id)
        {
            if (request.Id == Guid.Empty)
            {
                throw new BadRequestException("Id không được để trống");
            }
            
            // Ensure the request ID matches the route ID
            if (request.Id != id)
            {
                request.Id = id;
            }
            
            await _maintenanceService.UpdateMaintenanceRequestIssueAsync(request);
            return Ok();
        }

        [HttpGet("issue")]
        [SwaggerOperation(
            Summary = "Lấy danh sách yêu cầu bảo trì vảo trì/bảo dưỡng bất thường",
            Description = "Lấy danh sách yêu cầu bảo trì vảo trì/bảo dưỡng bất thường với các thông tin chi tiết.",
            OperationId = "GetMaintenanceRequestIssues",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<PagedApiResponse<GetAllMaintenanceRequestIssueResponse>> GetMaintenanceRequestIssuesAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Các tham số lọc bao gồm Search, IsActive, Status, IsPaid, CustomerId, Type, MaintenancePackageId, PageNumber, PageSize, SortColumn, và SortDir",
                Required = false
            )]
            GetAllMaintenanceRequestIssueFilterRequest request
        )
        {
            var result = await _maintenanceService.GetMaintenanceRequestIssuesAsync(request);
            return new PagedApiResponse<GetAllMaintenanceRequestIssueResponse>(
                result.data, 
                request.PageNumber, 
                request.PageSize, 
                result.total);
        }

        [HttpGet("issue/{id}")]
        [SwaggerOperation(
            Summary = "Lấy chi tiết yêu cầu ảo trì/bảo dưỡng bất thường",
            Description = "Lấy chi tiết yêu cầu ảo trì/bảo dưỡng bất thường với các thông tin chi tiết.",
            OperationId = "GetMaintenanceRequestIssue",
            Tags = new[] { "Maintenances" }
        )]
        public async Task<ApiResult<GetAllMaintenanceRequestIssueResponse>> GetMaintenanceRequestIssueAsync(
            [SwaggerParameter(
                Description = "ID của yêu cầu ảo trì/bảo dưỡng bất thường cần lấy chi tiết",
                Required = true
            )]
            Guid id)
        {
            var result = await _maintenanceService.GetMaintenanceRequestIssueAsync(id);
            return Ok(result);
    }
}
}
