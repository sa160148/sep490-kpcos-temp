using System;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

namespace KPCOS.BusinessLayer.Services;

/// <summary>
/// Dịch vụ quản lý vấn đề dự án hồ Koi
/// </summary>
public interface IProjectIssueService
{
    /// <summary>
    /// Tạo mới một vấn đề cho dự án hồ Koi
    /// </summary>
    /// <param name="constructionItemId">ID của hạng mục xây dựng liên quan đến vấn đề</param>
    /// <param name="request">Thông tin chi tiết của vấn đề cần tạo</param>
    /// <param name="userId">Tham số không được sử dụng, giữ lại để đảm bảo khả năng tương thích</param>
    /// <returns>Task hoàn thành khi tạo vấn đề thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy hạng mục xây dựng với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi hạng mục xây dựng không phải cấp 1, tên vấn đề trống, ID loại vấn đề không được cung cấp, hoặc vấn đề trùng tên đã tồn tại</exception>
    /// <remarks>
    /// Phương thức này chỉ chấp nhận hạng mục xây dựng cấp 1 (không có parentId).
    /// Hạng mục xây dựng cấp 2 không thể được sử dụng để tạo vấn đề.
    /// 
    /// Phương thức này sử dụng các trường sau từ đối tượng request:
    /// - Name (tên vấn đề) - bắt buộc
    /// - Description (mô tả vấn đề) - tùy chọn
    /// - Cause (nguyên nhân) - bắt buộc
    /// - Solution (giải pháp) - tùy chọn
    /// - IssueImage (hình ảnh vấn đề) - bắt buộc
    /// - IssueTypeId (ID loại vấn đề) - bắt buộc
    /// - EstimateAt (ngày dự kiến hoàn thành) - tùy chọn
    /// 
    /// Các trường khác nếu có trong request sẽ bị bỏ qua, bao gồm:
    /// - Reason (lý do)
    /// - ConfirmImage (hình ảnh xác nhận)
    /// - StaffId (ID nhân viên)
    /// - ActualAt (ngày thực tế hoàn thành)
    /// 
    /// Vấn đề mới luôn được tạo với trạng thái "OPENING" và các trường không được sử dụng sẽ được đặt là null.
    /// </remarks>
    Task CreateProjectIssueAsync(Guid constructionItemId, CommandProjectIssueRequest request, Guid userId);
    
    /// <summary>
    /// Cập nhật thông tin vấn đề dự án hồ Koi
    /// </summary>
    /// <param name="id">ID của vấn đề cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật cho vấn đề</param>
    /// <returns>Task hoàn thành khi cập nhật vấn đề thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi URL hình ảnh trống hoặc vấn đề trùng tên đã tồn tại</exception>
    /// <remarks>
    /// Trạng thái vấn đề được cập nhật tự động dựa trên các hành động:
    /// - Trường hợp 1: Cập nhật thông thường - Cho phép thay đổi trực tiếp trạng thái qua trường Status
    /// - Trường hợp 2: Chỉ cập nhật nhân viên - Nếu trạng thái hiện tại là "OPENING", trạng thái chuyển thành "PROCESSING"
    /// - Trường hợp 3: Chỉ cập nhật hình ảnh xác nhận - Trạng thái luôn chuyển thành "PREVIEWING"
    /// - Trường hợp 4: Chỉ cập nhật lý do - Nếu trạng thái hiện tại là "PREVIEWING", trạng thái chuyển thành "PROCESSING"
    /// 
    /// Khi gán nhân viên phụ trách, hệ thống thực hiện các kiểm tra:
    /// - Nhân viên phải tồn tại trong hệ thống
    /// - Nhân viên phải được phân công cho dự án liên quan
    /// - Nhân viên không được đang phụ trách công việc hoặc vấn đề khác chưa hoàn thành (status != "DONE")
    /// 
    /// Lưu ý: Trường `StaffId` trong request phải là ID của người dùng (User.Id), không phải ID của nhân viên (Staff.Id).
    /// Hệ thống sẽ tự động tìm nhân viên tương ứng với người dùng đó.
    /// </remarks>
    Task UpdateProjectIssueAsync(Guid id, CommandProjectIssueRequest request);
    
    /// <summary>
    /// Xóa hình ảnh của vấn đề dự án hồ Koi
    /// </summary>
    /// <param name="id">ID của vấn đề cần xóa hình ảnh</param>
    /// <returns>Task hoàn thành khi xóa hình ảnh thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề với ID được cung cấp</exception>
    /// <remarks>
    /// Phương thức này xóa trường hình ảnh của vấn đề dự án bằng cách đặt trường IssueImage thành null.
    /// </remarks>
    Task DeleteIssueImageAsync(Guid id);

    /// <summary>
    /// Xác nhận vấn đề dự án đã được giải quyết và đánh dấu hoàn thành
    /// </summary>
    /// <param name="id">ID của vấn đề cần xác nhận</param>
    /// <returns>Task hoàn thành khi xác nhận vấn đề thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi vấn đề không ở trạng thái PREVIEWING</exception>
    /// <remarks>
    /// Phương thức này chuyển trạng thái vấn đề từ PREVIEWING sang DONE.
    /// Đồng thời kiểm tra nếu tất cả các hạng mục xây dựng con (lv2) của hạng mục cha (lv1) đã DONE,
    /// thì cập nhật trạng thái của hạng mục cha thành DONE.
    /// </remarks>
    Task ConfirmProjectIssueAsync(Guid id);
}
