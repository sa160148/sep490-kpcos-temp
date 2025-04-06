using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.BusinessLayer.Services;

using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.WebFramework.Api;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using Swashbuckle.AspNetCore.Annotations;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using LinqKit;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;

namespace KPCOS.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    /// <param name="authService"></param>
    [Route("api/[controller]")]
    public class ProjectsController(
        IProjectService service, 
        IAuthService authService, 
        IConstructionServices constructionService, 
        IPaymentService paymentService,
        IFeedbackService feedbackService) : BaseController
    {
        /// <summary>
        /// Lấy danh sách dự án có phân trang dựa trên vai trò người dùng và các tiêu chí lọc
        /// </summary>
        /// <param name="filter">Các tiêu chí lọc cho dự án bao gồm:
        /// - Search: Lọc theo tên hoặc mô tả dự án
        /// - Status: Lọc theo trạng thái dự án (REQUESTING, PROCESSING, DESIGNING, CONSTRUCTING, FINISHED)
        /// - Area: Lọc theo diện tích tối thiểu (mét vuông)
        /// - Depth: Lọc theo độ sâu tối thiểu (mét)
        /// - PriceMin/PriceMax: Lọc theo khoảng giá báo giá đã xác nhận
        /// - PackageIds: Lọc theo ID gói dịch vụ (danh sách GUID phân cách bởi dấu phẩy)
        /// - Templatedesignids: Lọc theo ID mẫu thiết kế (danh sách GUID phân cách bởi dấu phẩy)
        /// - IsActive: Lọc theo trạng thái hoạt động (true/false)
        /// - IsDesignPublish: Khi true, API sẽ trả về các dự án như một showroom/template (chỉ trả về các dự án đã hoàn thành)
        /// - PageNumber: Số trang (bắt đầu từ 1)
        /// - PageSize: Số lượng item trên mỗi trang
        /// </param>
        /// <returns>Danh sách dự án có phân trang với thông tin cơ bản và nhân viên được phân công</returns>
        /// <remarks>
        /// API này trả về dự án dựa trên vai trò của người dùng và quyền hạn:
        /// 
        /// Quyền truy cập theo vai trò:
        /// - ADMINISTRATOR: Có thể xem tất cả dự án
        /// - CUSTOMER: Chỉ có thể xem dự án của họ
        /// - Nhân viên (CONSULTANT, DESIGNER, MANAGER, CONSTRUCTOR): Chỉ có thể xem dự án được phân công
        /// 
        /// Mỗi dự án bao gồm:
        /// - Thông tin cơ bản (tên, địa chỉ, diện tích, v.v.)
        /// - Thông tin gói dịch vụ
        /// - Chi tiết nhân viên được phân công
        /// - Hình ảnh thumbnail (khi IsDesignPublish=true và có thiết kế 3D được công bố)
        /// 
        /// Chức năng Showroom/Template (IsDesignPublish=true):
        /// - Tự động lọc chỉ lấy các dự án đã FINISHED
        /// - Hiển thị hình ảnh thumbnail từ thiết kế 3D đầu tiên được công bố
        /// - Bỏ qua các điều kiện về quyền truy cập của người dùng
        /// - Chỉ hiển thị các dự án có thiết kế đã được công bố (IsPublic=true)
        /// 
        /// Các giá trị trạng thái có thể:
        /// - REQUESTING: Trạng thái ban đầu cho dự án mới
        /// - PROCESSING: Dự án trong giai đoạn tư vấn
        /// - DESIGNING: Dự án trong giai đoạn thiết kế
        /// - CONSTRUCTING: Dự án trong giai đoạn thi công
        /// - FINISHED: Dự án đã hoàn thành
        /// 
        /// Ví dụ request:
        /// 
        ///     GET /api/projects?Search=modern&amp;Status=FINISHED&amp;IsDesignPublish=true&amp;PageNumber=1&amp;PageSize=10
        /// 
        /// Ví dụ response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Dự án của Nguyễn Văn A",
        ///       "customerName": "Nguyễn Văn A",
        ///       "email": "nguyenvana@example.com",
        ///       "phone": "+84909123456",
        ///       "note": "Ghi chú cho dự án",
        ///       "address": "123 Đường ABC",
        ///       "area": 150,
        ///       "depth": 3.5,
        ///       "status": "FINISHED",
        ///       "packageName": "Gói Premium",
        ///       "thumbnail": "https://example.com/designs/3d-view.jpg",
        ///       "isActive": true,
        ///       "createdAt": "2024-01-01T00:00:00Z",
        ///       "updatedAt": "2024-01-01T00:00:00Z",
        ///       "staffs": [
        ///         {
        ///           "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "fullName": "Trần Thị B",
        ///           "position": "DESIGNER",
        ///           "avatar": "https://example.com/avatar.jpg"
        ///         }
        ///       ]
        ///     }
        ///   ],
        ///   "pageNumber": 1,
        ///   "pageSize": 10,
        ///   "totalPages": 1,
        ///   "totalRecords": 1
        /// }
        /// ```
        /// 
        /// Tham số:
        /// - Search (string): Từ khóa tìm kiếm theo tên dự án (ví dụ: "modern", "house")
        /// - Status (string): Danh sách trạng thái dự án, phân cách bởi dấu phẩy (ví dụ: "PROCESSING,DESIGNING")
        /// - Area (number): Diện tích tối thiểu tính bằng mét vuông (ví dụ: 100)
        /// - Depth (number): Độ sâu tối thiểu tính bằng mét (ví dụ: 3.5)
        /// - PriceMin (number): Giá tối thiểu của báo giá đã xác nhận (ví dụ: 10000)
        /// - PriceMax (number): Giá tối đa của báo giá đã xác nhận (ví dụ: 50000)
        /// - PackageIds (string): Danh sách ID gói dịch vụ, phân cách bởi dấu phẩy
        /// - Templatedesignids (string): Danh sách ID mẫu thiết kế, phân cách bởi dấu phẩy
        /// - IsActive (boolean): Lọc theo trạng thái hoạt động (true/false)
        /// - IsDesignPublish (boolean): Khi true, trả về dự án như showroom/template
        /// - PageNumber (integer): Số trang, bắt đầu từ 1
        /// - PageSize (integer): Số lượng item trên mỗi trang (ví dụ: 10)
        /// </remarks>
        /// <response code="200">Thành công. Trả về danh sách dự án có phân trang</response>
        /// <response code="401">Không được phép. Người dùng chưa đăng nhập</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedApiResponse<ProjectForListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Lấy danh sách dự án có phân trang với chức năng showroom",
            Description = "Trả về dự án dựa trên vai trò người dùng hoặc như một showroom khi IsDesignPublish=true. Bao gồm thông tin dự án và hình ảnh thumbnail từ thiết kế 3D được công bố.",
            OperationId = "GetsAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<ProjectForListResponse>> GetsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria including:\n" +
                            "- Search: Filter by project name (e.g., \"modern\")\n" +
                            "- Status: Filter by status (e.g., \"PROCESSING,DESIGNING\")\n" +
                            "- Area: Minimum area in m² (e.g., 100)\n" +
                            "- Depth: Minimum depth in m (e.g., 3.5)\n" +
                            "- PriceMin/Max: Price range for confirmed quotations\n" +
                            "- IsActive: Active status (true/false)\n" +
                            "- IsDesignPublish: When true, returns projects as a showroom/template (only finished projects)\n" +
                            "- PageNumber/PageSize: Pagination parameters",
                Required = false
            )]
            GetAllProjectFilterRequest filter)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            (IEnumerable<ProjectForListResponse> Data, int Count) projects;

            // for case get all project with design publish status and using as a showroom, ignore user and role and hardcode status to finished.
            if (filter.IsDesignPublish.HasValue && filter.IsDesignPublish == true)
            {
                filter.UserId = null;
                filter.Role = null;
                filter.Status = EnumProjectStatus.FINISHED.ToString();
                projects = await service.GetsAsync(filter);
                return new PagedApiResponse<ProjectForListResponse>(
                    projects.Data,
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    totalRecords: projects.Count
                );
            }

            if (userIdClaim == null && roleClaim == null && filter.IsDesignPublish == null)
            {
                throw new UnauthorizedAccessException("Không được phép truy cập");
            }

            var userId = Guid.Parse(userIdClaim);
            filter.UserId = userId;
            filter.Role = roleClaim;
            projects = await service.GetsAsync(filter);
            return new PagedApiResponse<ProjectForListResponse>(
                projects.Data,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: projects.Count
            );
        }
        
        /// <summary>
        /// Gets projects for consultation with quotation base on project with actor: ADMINISTRATOR, CONSULTANT, CUSTOMER
        /// </summary>
        /// <param name="filter">Pagination parameters (pageNumber and pageSize)</param>
        /// <returns>Paginated list of projects with quotation information and standout status</returns>
        /// <remarks>
        /// <para>Retrieve a paginated list of project for consultation with each Role user: Administrator, Customer, Staff Consultant.</para>
        /// <para>This endpoint returns projects with status REQUESTING and PROCESSING.</para>
        /// 
        /// Projects are marked as standout based on role:
        /// 
        /// For Administrator:
        /// - Has open quotations
        /// - Has approved quotations without active/processing contracts
        /// 
        /// For Consultant:
        /// - Has no quotations
        /// - Has quotations in UPDATING or REJECTED status
        /// 
        /// For Customer:
        /// - Has contracts in PROCESSING status
        /// - Has PREVIEW quotations without any APPROVED/UPDATING quotations
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of projects</response>
        /// <response code="401">Unauthorized. User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("consultation")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectForQuotationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<PagedApiResponse<GetAllProjectForQuotationResponse>> GetsProjectForConsultationAsync(
            [FromQuery] GetAllProjectFilterRequest filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            var advandcedFilter = new GetAllProjectByUserIdRequest();
            advandcedFilter.GetExpressionsV2(Guid.Parse(userId), role).And(filter.GetExpressions());
            advandcedFilter.PageNumber = filter.PageNumber;
            advandcedFilter.PageSize = filter.PageSize;
            advandcedFilter.Status = new List<string>()
            {
                EnumProjectStatus.REQUESTING.ToString(),
                EnumProjectStatus.PROCESSING.ToString()
            };

            // Get projects and count in a single query
            var (projects, count) = await service.GetAllProjectForQuotationByUserIdAsync(advandcedFilter, userId, role);
            
            return new PagedApiResponse<GetAllProjectForQuotationResponse>(
                projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }
        
        /// <summary>
        /// Get projects for design phase with staff information and standout flags
        /// </summary>
        /// <param name="filter">Pagination parameters (PageNumber and PageSize)</param>
        /// <returns>Paginated list of projects with design information, staff details, and standout status</returns>
        /// <remarks>
        /// This endpoint returns projects with status DESIGNING and includes:
        /// - Basic project information (name, address, area, etc.)
        /// - Staff assigned to the project (designers, managers, etc.)
        /// - Latest design image URL
        /// - Standout flag based on user role
        /// 
        /// Standout flag rules by role:
        /// 
        /// For Administrator:
        /// - Project has no manager assigned
        /// 
        /// For Manager:
        /// - Project has no designer assigned, OR
        /// - Project has designs in OPENING status
        /// 
        /// For Designer:
        /// - Project has no designs, OR
        /// - Project has designs in REJECTED or EDITING status
        /// 
        /// For Customer:
        /// - Project has any design in PREVIEWING status
        /// 
        /// Sample request:
        /// 
        ///     GET /api/projects/design?PageNumber=1&amp;PageSize=10
        /// 
        /// Sample response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "name": "Project Name",
        ///       "customerName": "Customer Name",
        ///       "address": "123 Main St",
        ///       "area": 150,
        ///       "status": "DESIGNING",
        ///       "imageUrl": "https://example.com/image.jpg",
        ///       "standOut": true,
        ///       "staffs": [
        ///         {
        ///           "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "fullName": "Staff Name",
        ///           "email": "staff@example.com",
        ///           "position": "DESIGNER",
        ///           "avatar": "https://example.com/avatar.jpg"
        ///         }
        ///       ]
        ///     }
        ///   ],
        ///   "pageNumber": 1,
        ///   "pageSize": 10,
        ///   "totalPages": 1,
        ///   "totalRecords": 1
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of projects with design information and staff details</response>
        /// <response code="401">Unauthorized. User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("design")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectForDesignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<PagedApiResponse<GetAllProjectForDesignResponse>> GetsProjectForDesignAsync(
            [FromQuery] GetAllProjectFilterRequest filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            var advandcedFilter = new GetAllProjectByUserIdRequest();
            advandcedFilter.GetExpressionsV2(Guid.Parse(userId), role).And(filter.GetExpressions());
            advandcedFilter.PageNumber = filter.PageNumber;
            advandcedFilter.PageSize = filter.PageSize;
            advandcedFilter.Status = new List<string>()
            {
                EnumProjectStatus.DESIGNING.ToString()
            };
            
            // Get projects and count in a single query
            var (projects, count) = await service.GetAllProjectForDesignByUserIdAsync(advandcedFilter, userId, role);
            return new PagedApiResponse<GetAllProjectForDesignResponse>(
                projects,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: count);
        }

        /// <summary>
        /// Retrieve a project by id
        /// </summary>
        /// <param name="id">
        /// <para><see cref="Guid"/> request contains: </para>
        ///
        /// id: guid.
        /// </param>
        /// <remarks>
        /// <para>Retrieve a project by id.</para>
        /// Sample request:
        /// 
        ///     Get /api/projects{id}
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResult<ProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize]
        public async Task<ApiResult<ProjectResponse>> GetAsync(Guid id)
        {
            var project = await service.GetAsync(id);
            return project;
        }

        /// <summary>
        /// Create project
        /// </summary>
        /// <param name="request">
        /// <para><see cref="ProjectRequest"/> request object contains: </para>
        ///
        /// customerName: string.
        /// email: string.
        /// password: string.
        /// phone: string.
        /// address: string.
        /// area: float.
        /// depth: float.
        /// note: string.
        /// packageId: guid.
        /// templatedesignid: guid.
        ///</param>
        /// <returns>
        /// An Object with a JSON format.  <see cref="ApiResult"/>
        /// </returns>
        /// <remarks>
        /// <para>Customer request a new project, this mean customer create a project.</para>  
        /// Sample request:
        /// 
        ///     POST /api/projects
        ///     {
        ///         "customerName": "Customer 1",
        ///         "address": "HCM",
        ///         "phone": "0123456789",
        ///         "email": "",
        ///         "area": 100,
        ///         "depth": 100,
        ///         "packageId": "5ca78687-26db-40ed-99d0-685dff2b7e3e",
        ///         "note": "Note 1",
        ///         "templatedesignid": "5ca78687-26db-40ed-99d0-685dff2b7e3e"
        ///      }
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [CustomAuthorize("CUSTOMER")]
        public async Task<ApiResult> CreateAsync(ProjectRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await service.CreateAsync(request, Guid.Parse(userId));
            return Ok();
        }

        /// <summary>
        /// Assign staff to project based on project status
        /// </summary>
        /// <param name="request">
        /// <para><see cref="StaffAssignRequest"/> request object contains: </para>
        /// 
        /// staffId: guid (UserId from Staff table).
        ///</param>
        /// <param name="id">Project ID</param>
        /// <returns>An Object with a JSON format. <see cref="ApiResult"/></returns>
        /// <remarks>
        /// <para>Only admin can do this.</para>
        /// <para>Assigns staff to project following the status chain:</para>
        /// <para>REQUESTING -> Assign Consultant -> PROCESSING</para>
        /// <para>DESIGNING -> Assign Manager -> DESIGNING</para>
        /// <para>DESIGNING -> Assign Designer -> DESIGNING</para>
        /// <para>CONSTRUCTING -> Assign Constructor -> CONSTRUCTING</para>
        /// </remarks>
        [HttpPost("{id}/assignconsultant")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        // [CustomAuthorize("ADMINISTRATOR")]
        public async Task<ApiResult> AssignStaffAsync(Guid id, StaffAssignRequest request)
        {
            await service.AssignStaffAsync(id, request.StaffId);
            return Ok();
        }
        
        /// <summary>
        /// Get quotation by project
        /// </summary>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        ///
        /// pageNumber: int.
        /// pageSize: int.
        /// </param>
        /// <param name="id">Project ID</param>
        /// <remarks>
        /// <para>Retrieve a paginated list of quotation by project.</para>
        /// Sample request:
        /// 
        ///     Get /api/projects/{id}/quotation
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="500">Error</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("{id}/quotation")]
        public async Task<PagedApiResponse<QuotationForProjectResponse>> GetQuotationsByProjectAsync(Guid id, [FromQuery] GetAllQuotationFilterRequest filter)
        {
            var quotations = await service.GetQuotationsByProjectAsync(id, filter);
            return new PagedApiResponse<QuotationForProjectResponse>(quotations.data,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                totalRecords: quotations.total);
        }
        
        /// <summary>
        /// Get all contracts for a specific project
        /// </summary>
        /// <param name="id">The project ID to get contracts for</param>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        /// pageNumber: int - The page number to retrieve
        /// pageSize: int - The number of items per page
        /// </param>
        /// <returns>Paginated list of contracts with their associated quotation total prices</returns>
        /// <remarks>
        /// <para>Retrieves a paginated list of contracts for a specific project.</para>
        /// <para>Each contract includes:</para>
        /// <list type="bullet">
        ///     <item><description>Contract details (ID, name, status, etc.)</description></item>
        ///     <item><description>Contract value from the contract itself</description></item>
        /// </list>
        /// Sample request:
        /// 
        ///     GET /api/projects/{id}/contract?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of contracts</response>
        /// <response code="404">Project not found</response>
        /// <response code="400">Project is inactive</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/contract")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllContractResponse>> GetAllContractByProjectAsync(
            Guid id, 
            [FromQuery]
            GetAllContractFilterRequest filter)
        {
            filter.ProjectId = id;
            var contract = await service.GetContractByProjectAsync(filter);
            return new PagedApiResponse<GetAllContractResponse>(
                contract.data, 
                filter.PageNumber, 
                filter.PageSize, 
                contract.total);
        }
        
        /// <summary>
        /// Get all designs for a specific project
        /// </summary>
        /// <param name="id">The project ID to get designs for</param>
        /// <param name="filter">
        /// <para><see cref="PaginationFilter"/> request object contains: </para>
        /// pageNumber: int - The page number to retrieve
        /// pageSize: int - The number of items per page
        /// </param>
        /// <returns>Paginated list of designs with their associated images and staff information</returns>
        /// <remarks>
        /// <para>Retrieves a paginated list of designs for a specific project.</para>
        /// <para>Each design includes:</para>
        /// <list type="bullet">
        ///     <item><description>Basic design information (ID, version, status, etc.)</description></item>
        ///     <item><description>Design images associated with the design</description></item>
        ///     <item><description>Staff information who created the design</description></item>
        /// </list>
        /// Sample request:
        /// 
        ///     GET /api/projects/{id}/design?PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Success. Returns paginated list of designs</response>
        /// <response code="404">Project not found</response>
        /// <response code="400">Project is inactive</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/design")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllDesignResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllDesignResponse>> GetAllDesignByProjectAsync(Guid id, [FromQuery]GetAllDesignFilterRequest filter)
        {
            var design = await service.GetAllDesignByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllDesignResponse>(design.data, filter.PageNumber, filter.PageSize, design.total);
        }
        
        /// <summary>
        /// Gets a paginated list of construction items for a specific project
        /// </summary>
        /// <param name="id">The ID of the project</param>
        /// <param name="filter">Filter criteria for construction items including:
        /// - Search: Filters by name or description containing the search term
        /// - IsActive: Filters by active status (true/false)
        /// - Status: Filters by construction item status (OPENING, PROCESSING, DONE)
        /// - IsPayment: Filters by payment status (true/false)
        /// - IsChild: If true, returns only child items; if false, returns only parent items
        /// - PageNumber: Page number for pagination (1-based)
        /// - PageSize: Number of items per page
        /// - SortColumn: Column to sort by (default: CreatedAt)
        /// - SortDir: Sort direction (Asc or Desc, default: Desc)
        /// </param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/projects/{id}/construction?Search=foundation&amp;IsActive=true&amp;Status=OPENING&amp;IsPayment=true&amp;PageNumber=1&amp;PageSize=10
        /// 
        /// Available status values:
        /// - OPENING: Initial status for new construction items
        /// - PROCESSING: Construction items that are currently in progress
        /// - DONE: Completed construction items
        /// 
        /// IsChild filter behavior:
        /// - When IsChild=true: Returns only child items (items with a parent)
        /// - When IsChild=false: Returns only parent items (items without a parent) with their children
        /// - When IsChild is not specified: Returns parent items with their children (default behavior)
        /// </remarks>
        /// <returns>A paginated list of construction items with their children</returns>
        /// <response code="200">Returns the paginated list of construction items</response>
        [HttpGet("{id}/construction")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionItemResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of construction items for a specific project",
            Description = "Retrieves construction items for the specified project based on the provided filter criteria. Parent items are returned with their child items populated in the Childs property.",
            OperationId = "GetAllConstructionItemsByProject",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllConstructionItemResponse>> GetAllConstructionItemsByProjectAsync(Guid id,
            [FromQuery] 
            [SwaggerParameter(
                Description = "Filter criteria for construction items including Search, IsActive, Status (OPENING, PROCESSING, DONE), IsPayment, IsChild, PageNumber, PageSize, SortColumn, and SortDir",
                Required = false
            )]
            GetAllConstructionItemFilterRequest filter)
        {
            var (data, total) = await constructionService.GetAllConstructionItemsAsync(filter, id);
            return new PagedApiResponse<GetAllConstructionItemResponse>(data, filter.PageNumber, filter.PageSize, total);
        }

        [HttpGet("{id}/design/3d-confirmed")]
        [ProducesResponseType(typeof(ApiResult<IsDesignExitByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any confirmed 3D designs",
            Description = "Determines whether the specified project has any designs with type '3D' and status 'CONFIRMED'",
            OperationId = "IsDesign3DConfirmedAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsDesignExitByProjectResponse>> IsDesign3DConfirmedAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for confirmed 3D designs",
                Required = true
            )]
            Guid id)
        {
            var designs = await service.IsDesign3DConfirmedAsync(id);
            return Ok(designs);
        }

        [HttpGet("{id}/quotation/approved")]
        [ProducesResponseType(typeof(ApiResult<IsQuotationApprovedByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any approved quotations",
            Description = "Determines whether the specified project has any quotations with status 'APPROVED'",
            OperationId = "IsQuotationApprovedByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsQuotationApprovedByProjectResponse>> IsQuotationApprovedByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for approved quotations",
                Required = true
            )]
            Guid id)
        {
            var isQuotationApproved = await service.IsQuotationApprovedByProjectAsync(id);
            return Ok(isQuotationApproved);
        }

        [HttpGet("{id}/contract/active")]
        [ProducesResponseType(typeof(ApiResult<IsContractApprovedByProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Checks if a project has any active contracts",
            Description = "Determines whether the specified project has any contracts with status 'ACTIVE'",
            OperationId = "IsContractApprovedByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult<IsContractApprovedByProjectResponse>> IsContractApprovedByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to check for active contracts",
                Required = true
            )]
            Guid id)
        {
            var isContractApproved = await service.IsContractApprovedByProjectAsync(id);
            return Ok(isContractApproved);
        }

        /// <summary>
        /// Get all staff assigned to a specific project with filtering options
        /// </summary>
        /// <param name="id">The project ID to get staff for</param>
        /// <param name="filter">Filter parameters including Position, IsIdle, and pagination</param>
        /// <returns>A paginated list of staff assigned to the project</returns>
        /// <remarks>
        /// <para>This endpoint returns staff assigned to a project with filtering options:</para>
        /// <list type="bullet">
        ///     <item><description>Position: Filter by staff position (CONSTRUCTOR, MANAGER, CONSULTANT, etc.)</description></item>
        ///     <item><description>IsIdle: For constructors, returns only those not assigned to any active construction tasks</description></item>
        /// </list>
        /// <para>Sample request:</para>
        /// 
        ///     GET /api/projects/{id}/staff?Position=CONSTRUCTOR&amp;IsIdle=true&amp;PageNumber=1&amp;PageSize=10
        /// </remarks>
        /// <response code="200">Returns the list of staff</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}/staff")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllStaffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all staff assigned to a specific project with filtering options",
            Description = "Returns staff assigned to a project with filtering by position and idle status",
            OperationId = "GetAllStaffByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllStaffResponse>> GetAllStaffByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get staff for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for staff including Position, IsIdle, and pagination parameters",
                Required = false
            )]
            GetAllStaffRequest filter)
        {
            var staffs = await service.GetAllStaffByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllStaffResponse>(staffs.data, filter.PageNumber, filter.PageSize, staffs.total);
        }

        [HttpGet("{id}/construction-task")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllConstructionTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of construction tasks for a specific project",
            Description = "Retrieves construction tasks for the specified project based on the provided filter criteria. If the logged-in user is a CONSTRUCTOR, only returns tasks assigned to them. All other roles see all tasks for the project.",
            OperationId = "GetAllConstructionTaskByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllConstructionTaskResponse>> GetAllConstructionTaskByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get construction tasks for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for construction tasks including Search, IsActive, Status, IsOverdue, and ConstructionItemId",
                Required = false
            )]
            KPCOS.BusinessLayer.DTOs.Request.Constructions.GetAllConstructionTaskFilterRequest filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            (IEnumerable<GetAllConstructionTaskResponse> data, int total) constructionTasks;
            if (userIdClaim != null && roleClaim != null && roleClaim == RoleEnum.CONSTRUCTOR.ToString())
            {
                var userId = Guid.Parse(userIdClaim);
                filter.StaffId = userId;
            }
            filter.ProjectId = id;
            // constructionTasks = await constructionService.GetAllConstructionTaskAsync(filter);
            constructionTasks = await service.GetAllConstructionTaskByProjectAsync(filter);
            return new PagedApiResponse<GetAllConstructionTaskResponse>(constructionTasks.data, filter.PageNumber, filter.PageSize, constructionTasks.total);
        }

        [HttpGet("{id}/project-issue")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllProjectIssueResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Gets a paginated list of project issues for a specific project",
            Description = "Retrieves project issues for the specified project based on the provided filter criteria. If the logged-in user is a CONSTRUCTOR, only returns issues assigned to them. All other roles see all issues for the project.",
            OperationId = "GetAllProjectIssueByProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetAllProjectIssueResponse>> GetAllProjectIssueByProjectAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get issues for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for project issues including Search, Status, IssueTypeId, ConstructionItemId, and UserId",
                Required = false
            )]
            GetAllProjectIssueFilterRequest filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllProjectIssueResponse> data, int total) projectIssues;
            
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                projectIssues = await service.GetAllProjectIssueByProjectAsync(id, filter, userId);
                return new PagedApiResponse<GetAllProjectIssueResponse>(projectIssues.data, filter.PageNumber, filter.PageSize, projectIssues.total);
            }
            
            projectIssues = await service.GetAllProjectIssueByProjectAsync(id, filter);
            return new PagedApiResponse<GetAllProjectIssueResponse>(projectIssues.data, filter.PageNumber, filter.PageSize, projectIssues.total);
        }

        [HttpPut("{id}/finish")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Hoàn thành dự án và tạo yêu cầu bảo trì tự động",
            Description = "Chuyển trạng thái dự án sang FINISHED và tạo yêu cầu bảo trì tự động nếu có MaintenancePackageId.\n" +
                         "Lưu ý: Chỉ cần cung cấp MaintenancePackageId, các trường khác sẽ được hệ thống tự động điền.",
            OperationId = "FinishProjectAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<ApiResult> FinishProjectAsync(
            [SwaggerParameter(
                Description = "ID của dự án cần hoàn thành",
                Required = true
            )]
            Guid id,
            [FromBody]
            [SwaggerParameter(
                Description = "Thông tin yêu cầu bảo trì (tùy chọn).\n" +
                            "Chỉ cần cung cấp MaintenancePackageId, các trường khác sẽ được hệ thống tự động điền:\n" +
                            "- Tên: 'Bảo dưỡng/bảo trì' + tên dự án\n" +
                            "- Địa chỉ: Địa chỉ của dự án\n" +
                            "- Diện tích: Diện tích của dự án\n" +
                            "- Độ sâu: Độ sâu của dự án\n" +
                            "- Thời gian: 3 tháng\n" +
                            "- Tổng giá trị: 0\n" +
                            "- Loại: SCHEDULED\n" +
                            "- Ngày dự kiến: Ngày hiện tại + 1 ngày\n\n" +
                            "Ví dụ request:\n" +
                            "```json\n" +
                            "{\n" +
                            "  \"maintenancePackageId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"\n" +
                            "}\n" +
                            "```",
                Required = false
            )]
            CommandMaintenanceRequest maintenanceOptionalRequest)
        {
            await service.FinishProjectAsync(id, maintenanceOptionalRequest);
            return Ok();
        }

        [HttpGet("{id}/docs")]
        [ProducesResponseType(typeof(PagedApiResponse<GetAllDocResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<PagedApiResponse<GetAllDocResponse>> GetAllDocAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get docs for",
                Required = true
            )]
            Guid id,
            [FromQuery]
            GetAllDocFilterRequest filter)
        {
            filter.ProjectId = id;
            var docs = await service.GetAllDocAsync(filter);
            return new PagedApiResponse<GetAllDocResponse>(docs.data, filter.PageNumber, filter.PageSize, docs.total);
        }

        [HttpGet("{id}/payment")]
        [ProducesResponseType(typeof(PagedApiResponse<GetTransactionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Gets all payment transactions for a specific project",
            Description = "Retrieves a paginated list of payment transactions associated with the specified project. By default, returns transactions related to payment batches unless another 'Related' type is specified in the filter.",
            OperationId = "GetAllPaymentAsync",
            Tags = new[] { "Projects" }
        )]
        public async Task<PagedApiResponse<GetTransactionDetailResponse>> GetAllPaymentAsync(
            [SwaggerParameter(
                Description = "The ID of the project to get payments for",
                Required = true
            )]
            Guid id, 
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for transactions including AmountMin, AmountMax, Type, Status, and Related. The Related parameter can be 'batch', 'maintenance', or 'doc' to filter by transaction type.",
                Required = false
            )]
            GetAllTransactionFilterRequest filter)
        {
            filter.Type = EnumTransactionType.PAYMENT_BATCH.ToString() + "," + EnumTransactionType.DOC.ToString();
            
            var payments = await paymentService.GetTransactionsAsync(filter, projectId: id);
            return new PagedApiResponse<GetTransactionDetailResponse>(
                payments.data, 
                filter.PageNumber, 
                filter.PageSize, 
                payments.total);
        }
    }
}