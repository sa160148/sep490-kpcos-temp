using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Quotations;

public class GetAllQuotationFilterRequest : PaginationRequest<Quotation>
{
    public string? Status { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public override Expression<Func<Quotation, bool>> GetExpressions()
    {
        var quotationQueryExpression = PredicateBuilder.New<Quotation>(true);
        if (!string.IsNullOrEmpty(Status)){
            quotationQueryExpression.And(q => q.Status == Status);
        }
        if (MinPrice.HasValue){
            quotationQueryExpression.And(q => q.TotalPrice >= MinPrice.Value);
        }
        if (MaxPrice.HasValue){
            quotationQueryExpression.And(q => q.TotalPrice <= MaxPrice.Value);
        }
        if (UserId.HasValue && !string.IsNullOrEmpty(Role))
        {
            if (Role == RoleEnum.CUSTOMER.ToString())
            {
                quotationQueryExpression.And(q => 
                q.Status == EnumQuotationStatus.APPROVED.ToString() ||
                q.Status == EnumQuotationStatus.PREVIEW.ToString() ||
                q.Status == EnumQuotationStatus.CONFIRMED.ToString() ||
                q.Status == EnumQuotationStatus.UPDATING.ToString()
                );
            }
        }
        return quotationQueryExpression;
    }
}