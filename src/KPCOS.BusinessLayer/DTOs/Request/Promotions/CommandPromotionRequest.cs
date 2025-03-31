using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Promotions;

/// <summary>
/// Yêu cầu tạo mới hoặc cập nhật khuyến mãi
/// </summary>
public class CommandPromotionRequest
{
    /// <summary>
    /// Tên khuyến mãi
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Mã khuyến mãi (tự động tạo nếu không cung cấp)
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Phần trăm giảm giá (0-100)
    /// </summary>
    public int? Discount { get; set; }
    
    /// <summary>
    /// Thời gian bắt đầu khuyến mãi
    /// </summary>
    public DateTime? StartAt { get; set; }
    
    /// <summary>
    /// Thời gian kết thúc khuyến mãi
    /// </summary>
    public DateTime? ExpiredAt { get; set; }
    
    /// <summary>
    /// Ngày giới hạn sử dụng khuyến mãi (báo giá sau ngày này không thể sử dụng khuyến mãi)
    /// </summary>
    public DateTime? DeadlineAt { get; set; }
    
    /// <summary>
    /// Trạng thái khuyến mãi (PENDING, ACTIVE, EXPIRED)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Trạng thái hoạt động (true: đang hoạt động, false: đã bị xóa)
    /// </summary>
    public bool? IsActive { get; set; }
}
