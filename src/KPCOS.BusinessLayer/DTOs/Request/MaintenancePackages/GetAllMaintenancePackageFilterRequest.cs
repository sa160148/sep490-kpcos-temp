using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;

public class GetAllMaintenancePackageFilterRequest : PaginationRequest<MaintenancePackage>
{
    public string? Name { get; set; }
    
    public int? PriceMin { get; set; }

    public int? PriceMax { get; set; }
    
    public bool? IsActive { get; set; }
    
    public string? Status { get; set; }

    public override Expression<Func<MaintenancePackage, bool>> GetExpressions()
    {
        var expression = PredicateBuilder.New<MaintenancePackage>(true);

        if (!string.IsNullOrEmpty(Name))
        {
            expression = expression.And(x => x.Name.Contains(Name));
        }
        
        if (PriceMin.HasValue)
        {
            expression = expression.And(x => x.Price >= PriceMin.Value);
        }
        
        if (PriceMax.HasValue)
        {
            expression = expression.And(x => x.Price <= PriceMax.Value);
        }
        
        if (IsActive.HasValue)
        {
            expression = expression.And(x => x.IsActive == IsActive);
        }
        
        if (!string.IsNullOrEmpty(Status))
        {
            expression = expression.And(x => x.Status == Status);
        }
        
        return expression;
    }
}
