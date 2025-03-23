using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Docs;

public class GetAllDocFilterRequest : PaginationRequest<Doc>
{
    public string? Search { get; set; }
    public List<Guid>? DocTypeIds { get; set; }
    public Guid? ProjectId { get; set; }
    
    public override Expression<Func<Doc, bool>> GetExpressions()
    {
        var query = PredicateBuilder.New<Doc>(true);
        if (!string.IsNullOrEmpty(Search))
        {
            query = query.And(x => x.Name.Contains(Search));
        }
        if (DocTypeIds != null && DocTypeIds.Any())
        {
            query = query.And(x => DocTypeIds.Contains(x.DocTypeId));
        }
        if (ProjectId.HasValue)
        {
            query = query.And(x => x.ProjectId == ProjectId.Value);
        }
        return query;
    }
}
