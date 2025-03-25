using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.DTOs.Request.DocsType;

public class GetAllDocsTypeFilterRequest : PaginationRequest<DocType>
{
    public string? Search { get; set; }
    
    // Constructor to initialize default SortColumn value
    public GetAllDocsTypeFilterRequest()
    {
        // Set default sort column to Name instead of CreatedAt
        SortColumn = "Name";
    }
    
    public override Expression<Func<DocType, bool>> GetExpressions()
    {
        return docType =>
            (string.IsNullOrEmpty(Search) ||
             (docType.Name != null && docType.Name.Contains(Search)));
    }
}