using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Designs;

public class GetAllDesignFilterRequest : PaginationRequest<Design>
{
    public string? Status { get; set; }
    public bool? IsPublic { get; set; }
    public string? Type { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// Applies all filter conditions to the provided expression builder
    /// </summary>
    /// <param name="builder">Expression builder to apply conditions to</param>
    private void ApplyFilterConditions(ExpressionStarter<Design> builder)
    {
        if (!string.IsNullOrWhiteSpace(Status))
        {
            builder.And(d => d.Status == Status.ToUpper());
        }

        if (IsPublic.HasValue)
        {
            builder.And(d => d.IsPublic == IsPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(Type))
        {
            builder.And(d => d.Type == Type.ToUpper());
        }

        if (IsActive.HasValue)
        {
            builder.And(d => d.IsActive == IsActive.Value);
        }
    }

    /// <summary>
    /// Gets the combined filter expression for all conditions
    /// </summary>
    /// <returns>Expression containing all filter conditions</returns>
    public override Expression<Func<Design, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Design>(true);
        ApplyFilterConditions(predicate);
        return predicate;
    }
}