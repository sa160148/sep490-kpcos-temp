using System;
using System.Linq.Expressions;
using KPCOS.BusinessLayer.DTOs.Notifications;
using KPCOS.Common.Pagination;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Notifications;

public class GetAllNotificationFilterRequest : PaginationRequest<Notification>
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public Guid? No { get; set; }

    public override Expression<Func<Notification, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Notification>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search)
                                || x.Description.Contains(Search)
                                || x.Link.Contains(Search));
        }
        if (!string.IsNullOrEmpty(Type))
        {
            var types = Type.Split(',').ToList();
            predicate = predicate.And(x => types.Contains(x.Type));
        }
        if (IsRead.HasValue)
        {
            predicate = predicate.And(x => x.IsRead == IsRead);
        }
        if (IsActive.HasValue)
        {
            predicate = predicate.And(x => x.IsActive == IsActive);
        }
        if (CreatedAtFrom.HasValue)
        {
            predicate = predicate.And(x => x.CreatedAt >= CreatedAtFrom);
        }
        if (CreatedAtTo.HasValue)
        {
            predicate = predicate.And(x => x.CreatedAt <= CreatedAtTo);
        }
        if (No.HasValue)
        {
            predicate = predicate.And(x => x.No == No.ToString());
        }
        return predicate;
    }
}
