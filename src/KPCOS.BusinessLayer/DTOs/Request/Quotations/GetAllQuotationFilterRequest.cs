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
        throw new NotImplementedException();
    }

    public Expression<Func<Quotation, bool>> GetExpressionsV2()
    {
        var quotationQueryExpression = PredicateBuilder.New<Quotation>(true);
        if (!string.IsNullOrEmpty(Status)){
            quotationQueryExpression.And(x => x.Status == Status);
        }
        return quotationQueryExpression;
    }
}