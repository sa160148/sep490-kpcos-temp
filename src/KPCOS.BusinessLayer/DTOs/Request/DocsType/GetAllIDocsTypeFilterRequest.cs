using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.DTOs.Request.DocsType;

public class GetAllDocsTypeFilterRequest : PaginationRequest<DocType>
{
    public string? Search { get; set; }
    public override Expression<Func<DocType, bool>> GetExpressions()
    {
        return docType =>
            (string.IsNullOrEmpty(Search) ||
             (docType.Name != null && docType.Name.Contains(Search)));
    }
}