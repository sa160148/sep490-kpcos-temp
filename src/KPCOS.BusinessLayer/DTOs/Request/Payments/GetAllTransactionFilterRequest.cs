using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Payments;

/// <summary>
/// Yêu cầu lọc danh sách giao dịch thanh toán
/// </summary>
public class GetAllTransactionFilterRequest : PaginationRequest<Transaction>
{
    /// <summary>
    /// Số tiền tối thiểu của giao dịch
    /// </summary>
    public int? AmountMin { get; set; }

    /// <summary>
    /// Số tiền tối đa của giao dịch
    /// </summary>
    public int? AmountMax { get; set; }

    /// <summary>
    /// Loại giao dịch (PAYMENT_BATCH, MAINTENANCE_REQUEST, DOC)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// [DEPRECATED] Loại giao dịch (PAYMENT_BATCH, MAINTENANCE_REQUEST, DOC)
    /// </summary>
    [Obsolete("Use Type instead")]
    public string? Related { get; set; }

    /// <summary>
    /// Trạng thái giao dịch (SUCCESSFUL, FAILED)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Số tiền bắt đầu của khoảng tìm kiếm
    /// </summary>
    public int? FromAmount { get; set; }

    /// <summary>
    /// Số tiền kết thúc của khoảng tìm kiếm
    /// </summary>
    public int? ToAmount { get; set; }

    /// <summary>
    /// Vai trò của người dùng (CUSTOMER, ADMIN, STAFF)
    /// Lưu ý: Trường này **được tự động thiết lập bởi hệ thống** dựa trên người dùng đang đăng nhập
    /// - ADMINISTRATOR: Có thể xem tất cả giao dịch
    /// - CUSTOMER: Chỉ xem được các giao dịch của mình
    /// - STAFF: Có thể xem tất cả giao dịch, nhưng nó là lỗi
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// ID của người dùng
    /// Lưu ý: Trường này **được tự động thiết lập bởi hệ thống** dựa trên người dùng đang đăng nhập
    /// **Không cần nhập giá trị cho trường này**
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Tạo biểu thức lọc cho truy vấn
    /// </summary>
    /// <returns>Biểu thức lọc</returns>
    public override Expression<Func<Transaction, bool>> GetExpressions()
    {
        var expression = PredicateBuilder.New<Transaction>(true);
        if (AmountMin.HasValue)
        {
            expression = expression.And(x => x.Amount >= AmountMin.Value);
        }
        if (AmountMax.HasValue)
        {
            expression = expression.And(x => x.Amount <= AmountMax.Value);
        }
        if (!string.IsNullOrEmpty(Type))
        {
            var types = Type.Split(',').ToList();
            expression = expression.And(x => types.Contains(x.Type));
        }
        if (!string.IsNullOrEmpty(Status))
        {
            expression = expression.And(x => x.Status == Status);
        }
        if (FromAmount.HasValue)
        {
            expression = expression.And(x => x.Amount >= FromAmount.Value);
        }
        if (ToAmount.HasValue)
        {
            expression = expression.And(x => x.Amount <= ToAmount.Value);
        }
        if (UserId.HasValue)
        {
            if (Role == "CUSTOMER")
            {
                expression = expression.And(x => x.Customer.UserId == UserId.Value);
            }
        }
        return expression;
    }
}
