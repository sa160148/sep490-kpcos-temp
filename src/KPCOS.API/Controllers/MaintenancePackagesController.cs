using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/maintenance-packages")]
    public class MaintenancePackagesController : BaseController
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenancePackagesController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        /// <summary>
        /// Tạo mục bảo trì mới (maintenance item)
        /// </summary>
        /// <remarks>
        /// API này cho phép tạo mục bảo trì mới để sử dụng trong các gói bảo trì.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Mục bảo trì mới được tạo với trạng thái hoạt động (IsActive = true)
        /// - Tên mục bảo trì phải là duy nhất
        /// - Mô tả là tùy chọn, nhưng nên cung cấp để mô tả chi tiết mục bảo trì
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     {
        ///       "name": "Kiểm tra và xử lý tảo",
        ///       "description": "Kiểm tra nồng độ tảo trong nước, xử lý và loại bỏ tảo có hại"
        ///     }
        /// 
        /// **Các tham số:**
        /// - name: Tên mục bảo trì (bắt buộc)
        /// - description: Mô tả chi tiết mục bảo trì (tùy chọn)
        /// </remarks>
        /// <param name="request">Thông tin chi tiết về mục bảo trì mới</param>
        /// <response code="200">Mục bảo trì được tạo thành công</response>
        /// <response code="400">Thông tin mục bảo trì không hợp lệ (thiếu tên hoặc tên đã tồn tại)</response>
        [HttpPost("item")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Tạo mục bảo trì mới (maintenance item)",
            Description = "API này cho phép tạo mục bảo trì mới để sử dụng trong các gói bảo trì.",
            OperationId = "CreateMaintenanceItem",
            Tags = new[] { "MaintenancePackages" }
        )]
        //[CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> CreateMaintenancePackageItemAsync(
            [SwaggerParameter(
                Description = "Thông tin chi tiết về mục bảo trì mới, bao gồm tên và mô tả",
                Required = true
            )]
            [FromBody] 
            CommandMaintenanceItemRequest request)
        {
            await _maintenanceService.CreateMaintenancePackageItemAsync(request);
            return Ok();
        }

        /// <summary>
        /// Lấy danh sách các mục bảo trì theo bộ lọc
        /// </summary>
        /// <remarks>
        /// API này cho phép lấy danh sách mục bảo trì với phân trang và lọc theo nhiều tiêu chí.
        /// 
        /// **Các tham số lọc:**
        /// - Search: Tìm kiếm theo tên hoặc mô tả
        /// - IsActive: Lọc theo trạng thái hoạt động (true/false)
        /// - PageNumber: Số trang (bắt đầu từ 1)
        /// - PageSize: Số lượng phần tử trên mỗi trang
        /// - SortColumn: Cột sắp xếp (mặc định: CreatedAt)
        /// - SortDir: Hướng sắp xếp (Asc hoặc Desc, mặc định: Desc)
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     GET /api/maintenance-packages/item?Search=tảo&amp;IsActive=true&amp;PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <param name="request">Các tham số lọc</param>
        /// <returns>Danh sách các mục bảo trì theo bộ lọc với phân trang</returns>
        /// <response code="200">Trả về danh sách các mục bảo trì</response>
        [HttpGet("item")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllMaintenanceItemResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách các mục bảo trì theo bộ lọc",
            Description = "Truy vấn danh sách mục bảo trì dựa trên các tiêu chí lọc như tên, trạng thái hoạt động và phân trang.",
            OperationId = "GetMaintenanceItems",
            Tags = new[] { "MaintenancePackages" }
        )]
        //[CustomAuthorize("ADMINISTRATOR")]
        public async Task<PagedApiResponse<GetAllMaintenanceItemResponse>> GetAllMaintenanceItemAsync(
            [FromQuery] 
            [SwaggerParameter(
                Description = "Các tham số lọc bao gồm Search, IsActive, PageNumber, PageSize, SortColumn, và SortDir",
                Required = false
            )]
            GetAllMaintenanceItemFilterRequest request)
        {
            var maintenancePackageItems = await _maintenanceService.GetAllMaintenanceItemAsync(request);
            return new PagedApiResponse<GetAllMaintenanceItemResponse>(maintenancePackageItems.data, 
            request.PageNumber, 
            request.PageSize, 
            maintenancePackageItems.total);
        }

        /// <summary>
        /// Tạo gói bảo trì mới
        /// </summary>
        /// <remarks>
        /// API này cho phép tạo gói bảo trì mới với các mục bảo trì đi kèm.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Gói bảo trì mới được tạo với trạng thái hoạt động (IsActive = true)
        /// - Tên gói bảo trì phải là duy nhất
        /// - Có thể thêm nhiều mục bảo trì vào gói
        /// - Giá và tỷ lệ phải được cung cấp để tính toán chi phí bảo trì
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     {
        ///       "name": "Gói bảo trì cao cấp",
        ///       "description": "Gói bảo trì đầy đủ dành cho hồ cá Koi cao cấp",
        ///       "price": 1000000,
        ///       "rate": 10,
        ///       "status": "ACTIVE",
        ///       "maintenanceItems": [
        ///         "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "7b2c1c48-776f-49a0-86c5-25e9ec628f17"
        ///       ]
        ///     }
        /// 
        /// **Các tham số:**
        /// - name: Tên gói bảo trì (bắt buộc)
        /// - description: Mô tả chi tiết gói bảo trì (tùy chọn)
        /// - price: Giá cơ bản của gói bảo trì (bắt buộc)
        /// - rate: Tỷ lệ giảm giá theo thể tích (%) (bắt buộc)
        /// - status: Trạng thái gói bảo trì (tùy chọn, mặc định là ACTIVE)
        /// - maintenanceItems: Danh sách ID của các mục bảo trì thuộc gói (tùy chọn)
        /// </remarks>
        /// <param name="request">Thông tin chi tiết về gói bảo trì mới</param>
        /// <response code="200">Gói bảo trì được tạo thành công</response>
        /// <response code="400">Thông tin gói bảo trì không hợp lệ (thiếu thông tin bắt buộc hoặc tên đã tồn tại)</response>
        /// <response code="404">Không tìm thấy một hoặc nhiều mục bảo trì được chỉ định</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Tạo gói bảo trì mới",
            Description = "API này cho phép tạo gói bảo trì mới với các mục bảo trì đi kèm.",
            OperationId = "CreateMaintenancePackage",
            Tags = new[] { "MaintenancePackages" }
        )]
        //[CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> CreateMaintenancePackageAsync(
            [SwaggerParameter(
                Description = "Thông tin chi tiết về gói bảo trì mới, bao gồm tên, mô tả, giá, tỷ lệ và danh sách mục bảo trì",
                Required = true
            )]
            [FromBody] 
            CommandMaintenancePackageRequest request)
        {
            await _maintenanceService.CreateMaintenancePackageAsync(request);
            return Ok();
        }

        /// <summary>
        /// Lấy danh sách các gói bảo trì theo bộ lọc
        /// </summary>
        /// <remarks>
        /// API này cho phép lấy danh sách gói bảo trì với phân trang và lọc theo nhiều tiêu chí.
        /// 
        /// **Các tham số lọc:**
        /// - Search: Tìm kiếm theo tên hoặc mô tả
        /// - IsActive: Lọc theo trạng thái hoạt động (true/false)
        /// - Status: Lọc theo trạng thái gói (ACTIVE, INACTIVE)
        /// - PageNumber: Số trang (bắt đầu từ 1)
        /// - PageSize: Số lượng phần tử trên mỗi trang
        /// - SortColumn: Cột sắp xếp (mặc định: CreatedAt)
        /// - SortDir: Hướng sắp xếp (Asc hoặc Desc, mặc định: Desc)
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     GET /api/maintenance-packages?Search=cao%20cấp&amp;IsActive=true&amp;Status=ACTIVE&amp;PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <param name="request">Các tham số lọc</param>
        /// <returns>Danh sách các gói bảo trì theo bộ lọc với phân trang</returns>
        /// <response code="200">Trả về danh sách các gói bảo trì</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllMaintenancePackageResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách các gói bảo trì theo bộ lọc",
            Description = "Truy vấn danh sách gói bảo trì dựa trên các tiêu chí lọc như tên, trạng thái và phân trang.",
            OperationId = "GetMaintenancePackages",
            Tags = new[] { "MaintenancePackages" }
        )]
        public async Task<PagedApiResponse<GetAllMaintenancePackageResponse>> GetAllMaintenancePackageAsync(
            [FromQuery] 
            [SwaggerParameter(
                Description = "Các tham số lọc bao gồm Search, IsActive, Status, PageNumber, PageSize, SortColumn, và SortDir",
                Required = false
            )]
            GetAllMaintenancePackageFilterRequest request)
        {
            var maintenancePackages = await _maintenanceService.GetAllMaintenancePackageAsync(request);
            return new PagedApiResponse<GetAllMaintenancePackageResponse>(maintenancePackages.data, 
            request.PageNumber, 
            request.PageSize, 
            maintenancePackages.total);
        }

        /// <summary>
        /// Lấy thông tin chi tiết gói bảo trì theo ID
        /// </summary>
        /// <remarks>
        /// API này cho phép lấy thông tin chi tiết của một gói bảo trì dựa trên ID.
        /// 
        /// **Mẫu phản hồi:**
        /// 
        ///     {
        ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Gói bảo trì cao cấp",
        ///       "description": "Gói bảo trì đầy đủ dành cho hồ cá Koi cao cấp",
        ///       "priceList": [1000000, 950000, 902500, 857375, 814506],
        ///       "maintenanceItems": [
        ///         {
        ///           "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "name": "Kiểm tra và xử lý tảo",
        ///           "description": "Kiểm tra nồng độ tảo trong nước, xử lý và loại bỏ tảo có hại"
        ///         },
        ///         {
        ///           "id": "7b2c1c48-776f-49a0-86c5-25e9ec628f17",
        ///           "name": "Kiểm tra hệ thống lọc",
        ///           "description": "Kiểm tra và vệ sinh hệ thống lọc, thay thế vật liệu lọc nếu cần"
        ///         }
        ///       ],
        ///       "status": "ACTIVE",
        ///       "isActive": true,
        ///       "createdAt": "2024-03-20T10:00:00Z",
        ///       "updatedAt": "2024-03-20T10:00:00Z"
        ///     }
        /// </remarks>
        /// <param name="id">ID của gói bảo trì cần lấy thông tin</param>
        /// <returns>Thông tin chi tiết của gói bảo trì</returns>
        /// <response code="200">Trả về thông tin chi tiết của gói bảo trì</response>
        /// <response code="404">Không tìm thấy gói bảo trì với ID được cung cấp</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetAllMaintenancePackageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy thông tin gói bảo trì theo ID",
            Description = "Truy vấn thông tin gói bảo trì dựa trên ID",
            OperationId = "GetMaintenancePackageById",
            Tags = new[] { "MaintenancePackages" }
        )]
        public async Task<ApiResult<GetAllMaintenancePackageResponse>> GetDetailMaintenancePackageByIdAsync(
            [FromRoute] Guid id)
        {
            var maintenancePackage = await _maintenanceService.GetDetailMaintenancePackageByIdAsync(id);
            return Ok(maintenancePackage);
        }

        /// <summary>
        /// Xóa mục bảo trì khỏi gói bảo trì
        /// </summary>
        /// <remarks>
        /// API này cho phép xóa một mục bảo trì khỏi gói bảo trì.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Chỉ có thể xóa mục bảo trì khỏi gói bảo trì
        /// - Không thể xóa mục bảo trì nếu nó đang được sử dụng trong các yêu cầu bảo trì đang hoạt động
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     DELETE /api/maintenance-packages/3fa85f64-5717-4562-b3fc-2c963f66afa6/item/7b2c1c48-776f-49a0-86c5-25e9ec628f17
        /// </remarks>
        /// <param name="id">ID của gói bảo trì</param>
        /// <param name="itemId">ID của mục bảo trì cần xóa</param>
        /// <response code="200">Mục bảo trì đã được xóa khỏi gói bảo trì</response>
        /// <response code="404">Không tìm thấy mục bảo trì trong gói bảo trì</response>
        [HttpDelete("{id}/item/{itemId}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Xóa mục bảo trì khỏi gói bảo trì",
            Description = "API này cho phép xóa mục bảo trì khỏi gói bảo trì theo ID",
            OperationId = "DeleteMaintenancePackageItem",
            Tags = new[] { "MaintenancePackages" }
        )]
        [CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> DeleteMaintenancePackageItemAsync(
            [FromRoute] Guid id,
            [FromRoute] Guid itemId)
        {
            await _maintenanceService.DeleteMaintenancePackageItemAsync(id, itemId);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin gói bảo trì
        /// </summary>
        /// <remarks>
        /// API này cho phép cập nhật thông tin của một gói bảo trì.
        /// 
        /// **Quy tắc và hành vi:**
        /// - Có thể cập nhật tên, mô tả, giá, tỷ lệ và trạng thái của gói bảo trì
        /// - Có thể thêm mới các mục bảo trì vào gói
        /// - Không thể xóa mục bảo trì hiện có (phải sử dụng API xóa mục bảo trì)
        /// 
        /// **Mẫu yêu cầu:**
        /// 
        ///     {
        ///       "name": "Gói bảo trì cao cấp (Cập nhật)",
        ///       "description": "Gói bảo trì đầy đủ dành cho hồ cá Koi cao cấp với các dịch vụ mới",
        ///       "price": 1200000,
        ///       "rate": 15,
        ///       "maintenanceItems": [
        ///         "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "7b2c1c48-776f-49a0-86c5-25e9ec628f17",
        ///         "9d3e2f1a-8b7c-6d5e-4f3a-2b1c-0d9e8f7a6b5c"
        ///       ]
        ///     }
        /// </remarks>
        /// <param name="id">ID của gói bảo trì cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật của gói bảo trì</param>
        /// <response code="200">Gói bảo trì đã được cập nhật thành công</response>
        /// <response code="404">Không tìm thấy gói bảo trì với ID được cung cấp</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Cập nhật gói bảo trì",
            Description = "API này cho phép cập nhật gói bảo trì theo ID",
            OperationId = "UpdateMaintenancePackage",
            Tags = new[] { "MaintenancePackages" }
        )]
        [CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> UpdateMaintenancePackageAsync(
            [FromRoute] Guid id,
            [FromBody] CommandMaintenancePackageRequest request)
        {
            await _maintenanceService.UpdateMaintenancePackageAsync(id, request);
            return Ok();
        }
    }
}
