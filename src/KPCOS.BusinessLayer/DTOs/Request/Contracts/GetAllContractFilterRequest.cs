using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Contracts;

public class GetAllContractFilterRequest : PaginationRequest<Contract>
{
    public string? Status { get; set; }
    public override Expression<Func<Contract, bool>> GetExpressions()
    {
        var contractQueryExpression = PredicateBuilder.New<Contract>(true);
        if (!string.IsNullOrEmpty(Status))
        {
            contractQueryExpression.And(c => c.Status == Status);
        }
        return contractQueryExpression;
    }
}