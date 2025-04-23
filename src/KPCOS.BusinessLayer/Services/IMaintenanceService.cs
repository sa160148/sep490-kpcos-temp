using System;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Request.MaintenanceRequestIssues;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.MaintenanceRequestIssues;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Pagination;

namespace KPCOS.BusinessLayer.Services;

public interface IMaintenanceService
{
    Task CreateMaintenanceRequestAsync(CommandMaintenanceRequest request, Guid customerId);
    
    Task<(IEnumerable<GetAllMaintenanceRequestResponse> data, int total)> GetMaintenanceRequestsAsync(GetAllMaintenanceRequestFilterRequest request);
    
    Task CreateMaintenancePackageItemAsync(CommandMaintenanceItemRequest request);
    
    Task<(IEnumerable<GetAllMaintenanceItemResponse> data, int total)> GetAllMaintenanceItemAsync(GetAllMaintenanceItemFilterRequest request);
    
    Task CreateMaintenancePackageAsync(CommandMaintenancePackageRequest request);
    
    Task<(IEnumerable<GetAllMaintenancePackageResponse> data, int total)> GetAllMaintenancePackageAsync(GetAllMaintenancePackageFilterRequest request);
    
    Task UpdateMaintenanceTaskStatusAsync(Guid id, CommandMaintenanceRequestTaskRequest request);
    
    Task ConfirmMaintenanceTaskAsync(Guid id);
    
    Task<GetAllMaintenanceRequestTaskResponse> GetMaintenanceTaskAsync(Guid id);
    
    Task<(IEnumerable<GetAllMaintenanceRequestTaskResponse> data, int total)> GetAllMaintenanceRequestTasksAsync(GetAllMaintenanceRequestTaskFilterRequest request, Guid? userId = null);
    
    Task<(IEnumerable<GetAllStaffResponse> data, int total)> GetStaffsAsync(GetAllStaffRequest request, Guid maintenanceRequestId);
    
    Task AssignStaffsAsync(Guid maintenanceRequestId, CommandMaintenanceRequestTaskRequest request);

    /// <summary>
    /// Lấy thông tin chi tiết của một yêu cầu bảo trì theo ID
    /// </summary>
    /// <param name="id">ID của yêu cầu bảo trì cần lấy thông tin</param>
    /// <returns>Thông tin chi tiết của yêu cầu bảo trì</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy yêu cầu bảo trì với ID được cung cấp</exception>
    Task<GetAllMaintenanceRequestResponse> GetDetailMaintenanceRequestAsync(Guid id);

    /// <summary>
    /// Lấy thông tin chi tiết của một gói bảo trì theo ID
    /// </summary>
    /// <param name="id">ID của gói bảo trì cần lấy thông tin</param>
    /// <returns>Thông tin chi tiết của gói bảo trì</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy gói bảo trì với ID được cung cấp</exception>
    Task<GetAllMaintenancePackageResponse> GetDetailMaintenancePackageByIdAsync(Guid id);
    
    /// <summary>
    /// Xóa một mục bảo trì khỏi gói bảo trì
    /// </summary>
    /// <param name="maintenancePackageId">ID của gói bảo trì</param>
    /// <param name="maintenanceItemId">ID của mục bảo trì cần xóa</param>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy mục bảo trì trong gói bảo trì</exception>
    Task DeleteMaintenancePackageItemAsync(Guid maintenancePackageId, Guid maintenanceItemId);
    
    /// <summary>
    /// Cập nhật thông tin của một gói bảo trì
    /// </summary>
    /// <param name="id">ID của gói bảo trì cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật của gói bảo trì</param>
    /// <remarks>
    /// Hàm này sẽ:
    /// 1. Cập nhật các thuộc tính cơ bản của gói bảo trì (tên, mô tả, giá, tỷ lệ)
    /// 2. Thêm các mục bảo trì mới vào gói (nếu có)
    /// 3. Không xóa các mục bảo trì hiện có (sử dụng DeleteMaintenancePackageItemAsync để xóa)
    /// </remarks>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy gói bảo trì hoặc mục bảo trì mới không tồn tại</exception>
    Task UpdateMaintenancePackageAsync(Guid id, CommandMaintenancePackageRequest request);

    /// <summary>
    /// Tạo vấn đề mới cho yêu cầu bảo trì
    /// </summary>
    /// <param name="request">Thông tin vấn đề bảo trì cần tạo</param>
    /// <returns>ID của vấn đề bảo trì đã tạo</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy yêu cầu bảo trì hoặc nhân viên với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi dữ liệu đầu vào không hợp lệ</exception>
    Task CreateMaintenanceRequestIssueAsync(CommandMaintenanceRequestIssueRequest request);
    
    /// <summary>
    /// Cập nhật thông tin vấn đề bảo trì
    /// </summary>
    /// <param name="request">Thông tin cập nhật của vấn đề bảo trì</param>
    /// <returns>ID của vấn đề bảo trì đã cập nhật</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề bảo trì với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi dữ liệu đầu vào không hợp lệ hoặc trạng thái chuyển đổi không hợp lệ</exception>
    /// <remarks>
    /// Hàm này sẽ xử lý nhiều trường hợp cập nhật khác nhau dựa trên dữ liệu đầu vào:
    /// 1. Phân công nhân viên: Khi staffId có giá trị thì chuyển trạng thái từ OPENING thành PROCESSING. Bắt buộc phải cung cấp EstimateAt khi phân công nhân viên.
    ///    EstimateAt sẽ được kiểm tra và không được: rơi vào ngày cuối tuần, trước ngày công việc bảo trì đầu tiên, sau ngày công việc bảo trì cuối cùng, 
    ///    hoặc trùng với ngày của công việc bảo trì khác.
    /// 2. Tải lên ảnh xác nhận: Khi confirmImage có giá trị thì chuyển trạng thái từ PROCESSING thành PREVIEWING.
    /// 3. Từ chối ảnh xác nhận: Khi reason có giá trị thì chuyển trạng thái từ PREVIEWING thành PROCESSING.
    /// 4. Giải quyết nhanh: Khi solution có giá trị và các trường khác không có thì chuyển trạng thái thành DONE.
    /// 5. Cập nhật thông thường: Chỉ cập nhật các thông tin cơ bản mà không thay đổi trạng thái.
    /// 6. Hủy vấn đề: Khi status có giá trị CANCELLED thì cập nhật trạng thái thành CANCELLED.
    /// 7. Xác nhận hoàn thành: Khi status có giá trị DONE và trạng thái hiện tại là PREVIEWING thì chuyển trạng thái thành DONE.
    /// </remarks>
    Task UpdateMaintenanceRequestIssueAsync(CommandMaintenanceRequestIssueRequest request);
    
    /// <summary>
    /// Lấy danh sách vấn đề bảo trì theo bộ lọc
    /// </summary>
    /// <param name="request">Bộ lọc để tìm kiếm vấn đề bảo trì</param>
    /// <returns>Danh sách vấn đề bảo trì phù hợp với bộ lọc</returns>
    Task<(IEnumerable<GetAllMaintenanceRequestIssueResponse> data, int total)> GetMaintenanceRequestIssuesAsync(GetAllMaintenanceRequestIssueFilterRequest request);
    
    /// <summary>
    /// Lấy thông tin chi tiết của một vấn đề bảo trì theo ID
    /// </summary>
    /// <param name="id">ID của vấn đề bảo trì cần lấy thông tin</param>
    /// <returns>Thông tin chi tiết của vấn đề bảo trì</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề bảo trì với ID được cung cấp</exception>
    Task<GetAllMaintenanceRequestIssueResponse> GetMaintenanceRequestIssueAsync(Guid id);
}
