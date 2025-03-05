using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Quotations;

public class GetAllQuotationFilterRequest : PaginationRequest<Quotation>
{
    public string? Status { get; set; }
    public override Expression<Func<Quotation, bool>> GetExpressions()
    {
        var quotationQueryExpression = PredicateBuilder.New<Quotation>(true);
        if (!string.IsNullOrEmpty(Status)){
            quotationQueryExpression.And(q => q.Status == Status);
        }
        return quotationQueryExpression;
    }
}