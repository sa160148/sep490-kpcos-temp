using System;
using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Promotions;

/// <summary>
/// Phản hồi chứa thông tin chi tiết về khuyến mãi
/// </summary>
public class GetAllPromotionResponse
{
    /// <summary>
    /// Định danh duy nhất của khuyến mãi
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tên khuyến mãi
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
    
    /// <summary>
    /// Mã khuyến mãi
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
    
    /// <summary>
    /// Phần trăm giảm giá (0-100)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Discount { get; set; }
    
    /// <summary>
    /// Thời gian bắt đầu khuyến mãi
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? StartAt { get; set; }
    
    /// <summary>
    /// Thời gian kết thúc khuyến mãi
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ExpiredAt { get; set; }
    
    /// <summary>
    /// Trạng thái khuyến mãi (PENDING, ACTIVE, EXPIRED)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    
    /// <summary>
    /// Thời gian tạo khuyến mãi
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Thời gian cập nhật khuyến mãi gần nhất
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Trạng thái hoạt động (true: đang hoạt động, false: đã bị xóa)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsActive { get; set; }
}
