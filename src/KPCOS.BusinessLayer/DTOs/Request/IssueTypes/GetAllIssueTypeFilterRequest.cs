using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.DTOs.Request.IssueTypes;

public class GetAllIssueTypeFilterRequest : PaginationRequest<IssueType>
{
    
    public string? Search { get; set; }
    public override Expression<Func<IssueType, bool>> GetExpressions()
    {
        return issueType => 
            (string.IsNullOrEmpty(Search) || 
                (issueType.Name != null && issueType.Name.Contains(Search))) &&
            (issueType.IsActive == true);
    }
}