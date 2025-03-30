using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

/// <summary>
/// Enum trạng thái của khuyến mãi trong hệ thống xây dựng và bảo trì hồ cá Koi
/// </summary>
[DataContract]
public enum EnumPromotionStatus
{
    /// <summary>
    /// Khuyến mãi đang hoạt động (trong thời gian hiệu lực)
    /// </summary>
    [EnumMember(Value = "ACTIVE")]
    ACTIVE,
    
    /// <summary>
    /// Khuyến mãi đang chờ (chưa đến thời gian bắt đầu)
    /// </summary>
    [EnumMember(Value = "PENDING")]
    PENDING,
    
    /// <summary>
    /// Khuyến mãi đã hết hạn (đã qua thời gian kết thúc)
    /// </summary>
    [EnumMember(Value = "EXPIRED")]
    EXPIRED
}
