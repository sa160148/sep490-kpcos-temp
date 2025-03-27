using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

/// <summary>
/// Request model for filtering construction tasks with pagination support
/// </summary>
public class GetAllConstructionTaskFilterRequest : PaginationRequest<ConstructionTask>
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
    public string? Status { get; set; }
    
    /// <summary>
    /// Optional flag to filter tasks by overdue status.
    /// <para>When true, returns tasks with deadlines in the past that are not marked as DONE.</para>
    /// When false, returns tasks that are not overdue or are marked as DONE.
    /// </summary>
    public bool? IsOverdue { get; set; }
    
    /// <summary>
    /// Optional construction item ID to filter tasks by their associated construction item
    /// </summary>
    public Guid? ConstructionItemId { get; set; }
    
    /// <summary>
    /// Builds the filter expression based on the provided filter criteria
    /// </summary>
    /// <returns>Expression to filter construction tasks</returns>
    public override Expression<Func<ConstructionTask, bool>> GetExpressions()
    {
        // Get current SEA time for deadline comparison
        var currentSEATime = GlobalUtility.GetCurrentSEATime();
        var predicate = PredicateBuilder.New<ConstructionTask>();

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(task => task.Name.Contains(Search));
        }
        
        if (IsActive.HasValue)
        {
            predicate = predicate.And(task => task.IsActive == IsActive);
        }
        
        if (!string.IsNullOrEmpty(Status))
        {
            predicate = predicate.And(task => task.Status == Status);
        }
        
        if (IsOverdue.HasValue)
        {
            predicate = predicate.And(task =>
                (IsOverdue.Value && task.DeadlineAt.HasValue && task.DeadlineAt.Value < currentSEATime && task.Status != "DONE") ||
                (!IsOverdue.Value && (!task.DeadlineAt.HasValue || task.DeadlineAt.Value >= currentSEATime || task.Status == "DONE")));
        }
        
        if (ConstructionItemId.HasValue)
        {
            predicate = predicate.And(task => task.ConstructionItemId == ConstructionItemId);
        }
        
        if (!string.IsNullOrEmpty(Status))
        {
            var statuses = Status.Split(',').ToList();
            predicate = predicate.And(task => statuses.Contains(task.Status));
        }
        
        return predicate;
        /*
        return task => 
            (string.IsNullOrEmpty(Search) || task.Name.Contains(Search)) &&
            (!IsActive.HasValue || task.IsActive == IsActive) &&
            (string.IsNullOrEmpty(Status) || task.Status == Status) &&
            (!IsOverdue.HasValue || 
                (IsOverdue.Value && task.DeadlineAt.HasValue && task.DeadlineAt.Value < currentSEATime && task.Status != "DONE") || 
                (!IsOverdue.Value && (!task.DeadlineAt.HasValue || task.DeadlineAt.Value >= currentSEATime || task.Status == "DONE"))) &&
            (!ConstructionItemId.HasValue || task.ConstructionItemId == ConstructionItemId);
        */
    }
}
