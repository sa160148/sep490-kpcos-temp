using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Statistics;

public class GetStatisticFilterRequest : PaginationRequest<Transaction>
{
    public string? Year { get; set; }
    public string? Type { get; set; }
    public override Expression<Func<Transaction, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Transaction>(true);
        if (!string.IsNullOrEmpty(Year))
        {
            var years = Year.Split(',').ToList();
            predicate = predicate.And(x => years.Contains(x.CreatedAt.Value.Year.ToString()));
        }
        if (!string.IsNullOrEmpty(Type))
        {
            var types = Type.Split(',').ToList();
            predicate = predicate.And(x => types.Contains(x.Type.ToString()));
        }
        return predicate;
    }
}
