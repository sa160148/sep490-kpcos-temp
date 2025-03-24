using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;

public class GetAllMaintenanceItemFilterRequest : PaginationRequest<MaintenanceItem>
{
    public string? Name { get; set; }
    
    public bool? IsActive { get; set; }
    
    public override Expression<Func<MaintenanceItem, bool>> GetExpressions()
    {
        var expression = PredicateBuilder.New<MaintenanceItem>(true);

        if (!string.IsNullOrEmpty(Name))
        {
            expression = expression.And(x => x.Name.Contains(Name));
        }
        if (IsActive.HasValue)
        {
            expression = expression.And(x => x.IsActive == IsActive.Value);
        }
        return expression;
    }
}
