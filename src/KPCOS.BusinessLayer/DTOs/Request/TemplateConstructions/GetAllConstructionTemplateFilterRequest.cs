using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class GetAllConstructionTemplateFilterRequest : PaginationRequest<ConstructionTemplate>
{
    /// <summary>
    /// Optional search term to filter tasks by name
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Optional flag to filter tasks by active status
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Optional status to filter tasks (e.g., "OPENING", "PROCESSING", "PREVIEWING", "DONE")
    /// </summary>
  
   
    public override Expression<Func<ConstructionTemplate, bool>> GetExpressions()
    {

        return templateConstructions =>
            (string.IsNullOrEmpty(Search) || templateConstructions.Name.Contains(Search)) &&
            (!IsActive.HasValue || templateConstructions.IsActive == IsActive);

    }
}