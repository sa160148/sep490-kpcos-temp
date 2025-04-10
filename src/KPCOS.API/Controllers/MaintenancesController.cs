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


    }
}
