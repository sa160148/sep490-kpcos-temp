using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Blogs;

public class GetAllBlogFilterRequest : PaginationRequest<Blog>
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public override Expression<Func<Blog, bool>> GetExpressions()
    {
        var expression = PredicateBuilder.New<Blog>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            expression = expression.And(x => x.Name.Contains(Search));
            expression = expression.Or(x => x.Description.Contains(Search));
            expression = expression.Or(x => x.Type.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Type))
        {
            List<string> types = Type.Split(',').ToList();
            expression = expression.And(x => types.Contains(x.Type));
        }

        if (From != null)
        {
            expression = expression.And(x => x.CreatedAt >= From);
        }

        if (To != null)
        {
            expression = expression.And(x => x.CreatedAt <= To);
        }

        return expression;
    }
}
