using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;
using KPCOS.Common.Utilities;

namespace KPCOS.BusinessLayer.DTOs.Request.Promotions;

/// <summary>
/// Yêu cầu lọc và phân trang cho danh sách khuyến mãi
/// </summary>
public class GetAllPromotionFilterRequest : PaginationRequest<Promotion>
{
    /// <summary>
    /// Từ khóa tìm kiếm (tìm theo tên khuyến mãi)
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Lọc theo trạng thái khuyến mãi (PENDING, ACTIVE, EXPIRED)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Lọc theo phần trăm giảm giá
    /// </summary>
    public int? Discount { get; set; }
    
    /// <summary>
    /// Lọc theo thời gian bắt đầu (từ ngày này trở đi)
    /// </summary>
    public DateTime? StartAtFrom { get; set; }

    /// <summary>
    /// Lọc theo thời gian bắt đầu (từ ngày này trở đi)
    /// </summary>
    public DateTime? StartAtTo { get; set; }
    
    /// <summary>
    /// Lọc theo thời gian kết thúc (đến ngày này)
    /// </summary>
    public DateTime? ExpiredAtFrom { get; set; }

    /// <summary>
    /// Lọc theo thời gian kết thúc (đến ngày này)
    /// </summary>
    public DateTime? ExpiredAtTo { get; set; }

    /// <summary>
    /// Lọc theo ngày giới hạn sử dụng khuyến mãi
    /// </summary>
    public DateTime? DeadlineAtFrom { get; set; }

    /// <summary>
    /// Lọc theo ngày giới hạn sử dụng khuyến mãi
    /// </summary>
    public DateTime? DeadlineAtTo { get; set; }
    
    /// <summary>
    /// Lọc theo trạng thái hoạt động
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Tạo biểu thức điều kiện lọc dựa trên các tham số đã cung cấp
    /// </summary>
    /// <returns>Biểu thức điều kiện lọc</returns>
    public override Expression<Func<Promotion, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Promotion>(true);
        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search));
        }
        if (!string.IsNullOrEmpty(Status))
        {
            var status = Status.Trim(',').ToLower();
            predicate = predicate.And(x => x.Status == status);
        }
        if (Discount != null)
        {
            predicate = predicate.And(x => x.Discount == Discount);
        }
        if (StartAtFrom != null)
        {
            DateTime? startAtFrom = GlobalUtility.NormalizeDateTime(StartAtFrom);
            predicate = predicate.And(x => x.StartAt >= startAtFrom);
        }
        if (StartAtTo != null)
        {
            DateTime? startAtTo = GlobalUtility.NormalizeDateTime(StartAtTo);
            predicate = predicate.And(x => x.StartAt <= startAtTo);
        }
        if (ExpiredAtFrom != null)
        {
            DateTime? expiredAtFrom = GlobalUtility.NormalizeDateTime(ExpiredAtFrom);
            predicate = predicate.And(x => x.ExpiredAt >= expiredAtFrom);
        }
        if (ExpiredAtTo != null)
        {
            DateTime? expiredAtTo = GlobalUtility.NormalizeDateTime(ExpiredAtTo);
            predicate = predicate.And(x => x.ExpiredAt <= expiredAtTo);
        }
        if (DeadlineAtFrom != null)
        {
            DateTime? deadlineAtFrom = GlobalUtility.NormalizeDateTime(DeadlineAtFrom);
            predicate = predicate.And(x => x.DeadlineAt >= deadlineAtFrom);
        }
        if (DeadlineAtTo != null)
        {
            DateTime? deadlineAtTo = GlobalUtility.NormalizeDateTime(DeadlineAtTo);
            predicate = predicate.And(x => x.DeadlineAt <= deadlineAtTo);
        }
        if (IsActive.HasValue)
        {
            predicate = predicate.And(x => x.IsActive == IsActive);
        }
        return predicate;
    }

    /*
    /// <summary>
    /// Áp dụng sắp xếp cho truy vấn dựa trên cột sắp xếp được chỉ định
    /// </summary>
    /// <param name="query">Truy vấn cần sắp xếp</param>
    /// <returns>Truy vấn đã được sắp xếp</returns>
    public IOrderedQueryable<Promotion> ApplySort(IQueryable<Promotion> query)
    {
        // Sắp xếp mặc định theo thời gian tạo giảm dần (mới nhất đầu tiên)
        if (string.IsNullOrEmpty(SortColumn))
        {
            return query.OrderByDescending(p => p.CreatedAt);
        }

        // Xử lý SortColumn và SortDir
        var isAscending = SortDir == SortDirection.Asc;
        
        // Xử lý các trường sắp xếp khác nhau
        return isAscending
            ? SortColumn.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "code" => query.OrderBy(p => p.Code),
                "discount" => query.OrderBy(p => p.Discount),
                "startat" => query.OrderBy(p => p.StartAt),
                "expiredat" => query.OrderBy(p => p.ExpiredAt),
                "status" => query.OrderBy(p => p.Status),
                "createdat" => query.OrderBy(p => p.CreatedAt),
                "updatedat" => query.OrderBy(p => p.UpdatedAt),
                _ => query.OrderBy(p => p.CreatedAt)
            }
            : SortColumn.ToLower() switch
            {
                "name" => query.OrderByDescending(p => p.Name),
                "code" => query.OrderByDescending(p => p.Code),
                "discount" => query.OrderByDescending(p => p.Discount),
                "startat" => query.OrderByDescending(p => p.StartAt),
                "expiredat" => query.OrderByDescending(p => p.ExpiredAt),
                "status" => query.OrderByDescending(p => p.Status),
                "createdat" => query.OrderByDescending(p => p.CreatedAt),
                "updatedat" => query.OrderByDescending(p => p.UpdatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
    }
    */
}
