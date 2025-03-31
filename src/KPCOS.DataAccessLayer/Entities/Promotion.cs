namespace KPCOS.DataAccessLayer.Entities;

/// <summary>
/// Đối tượng khuyến mãi trong hệ thống xây dựng và bảo trì hồ cá Koi
/// </summary>
public partial class Promotion
{
    /// <summary>
    /// Định danh duy nhất của khuyến mãi
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Thời gian tạo khuyến mãi
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật khuyến mãi gần nhất
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Trạng thái hoạt động (true: đang hoạt động, false: đã bị xóa)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Tên khuyến mãi
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Mã khuyến mãi
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Phần trăm giảm giá (0-100)
    /// </summary>
    public int Discount { get; set; }

    /// <summary>
    /// Thời gian bắt đầu khuyến mãi
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// Thời gian kết thúc khuyến mãi
    /// </summary>
    public DateTime? ExpiredAt { get; set; }

    /// <summary>
    /// Ngày giới hạn sử dụng khuyến mãi
    /// </summary>
    public DateTime? DeadlineAt { get; set; }

    /// <summary>
    /// Trạng thái khuyến mãi (PENDING, ACTIVE, EXPIRED)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Danh sách báo giá sử dụng khuyến mãi này
    /// </summary>
    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
