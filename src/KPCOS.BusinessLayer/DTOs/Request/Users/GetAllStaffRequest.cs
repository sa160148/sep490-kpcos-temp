using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Users;

public class GetAllStaffRequest : PaginationRequest<Staff>
{
    public string? Search { get; set; }
    public string? Position { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsIdle { get; set; }

    public override Expression<Func<Staff, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Staff>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(s => s.User.FullName.Contains(Search) || 
                                          s.User.Email.Contains(Search) ||
                                          s.User.Phone.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Position))
        {
            predicate = predicate.And(s => s.Position == Position);
        }

        if (IsActive.HasValue)
        {
            predicate = predicate.And(s => s.User.IsActive == IsActive.Value);
        }

        return predicate;
    }
}
