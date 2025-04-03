using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
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
    public bool? IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional status to filter tasks (e.g., "OPENING", "PROCESSING", "PREVIEWING", "DONE")
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Optional flag to filter tasks by overdue status, true will return tasks with deadlines in the past that are not marked as DONE, false will return tasks that are not overdue or are marked as DONE.
    /// <para>When true, returns tasks with deadlines in the past that are not marked as DONE.</para>
    /// When false, returns tasks that are not overdue or are marked as DONE.
    /// </summary>
    public bool? IsOverdue { get; set; }
    
    /// <summary>
    /// Optional construction item ID to filter tasks by their associated construction item
    /// </summary>
    public Guid? ConstructionItemId { get; set; }

    /// <summary>
    /// Optional project ID to filter tasks by their associated project, if not provided, all tasks will be returned.
    /// <para>If this filter is use in api/projects/{id}/construction-task, it will filter tasks by their associated project.</para>
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Optional staff ID to filter tasks by their associated staff.
    /// <para>**IGNORE THIS FILTER**, it will auto set when login user is constructor, other case will return all tasks.</para>
    /// </summary>
    public Guid? StaffId { get; set; }

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
            // Handle comma-separated status values
            var statuses = Status.Split(',').ToList();
            predicate = predicate.And(task => statuses.Contains(task.Status));
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

        if (ProjectId.HasValue)
        {
            predicate = predicate.And(task => task.ConstructionItem.ProjectId == ProjectId);
        }

        if (StaffId.HasValue)
        {
            // Only filter by staff if they are a constructor
            predicate = predicate.And(task => 
                task.Staff.UserId == StaffId);
        }
        
        return predicate;
    }
}
