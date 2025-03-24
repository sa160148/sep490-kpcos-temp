using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Payments;

public class GetAllTransactionFilterRequest : PaginationRequest<Transaction>
{
    public int? AmountMin { get; set; }
    public int? AmountMax { get; set; }
    public string? Type { get; set; }
    public string? Related { get; set; }
    public string? Status { get; set; }
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
            expression = expression.And(x => x.Type == Type);
        }
        if (!string.IsNullOrEmpty(Related))
        {
            // We don't filter here, because we can't easily join tables in LINQ expressions
            // The filtering by related type is handled in the service layer
        }
        if (!string.IsNullOrEmpty(Status))
        {
            expression = expression.And(x => x.Status == Status);
        }
        return expression;
    }
}
