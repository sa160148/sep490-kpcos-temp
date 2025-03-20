using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

/// <summary>
/// Filter criteria for retrieving construction items
/// </summary>
public class GetAllConstructionItemFilterRequest : PaginationRequest<ConstructionItem>
{
    /// <summary>
    /// Search term to filter by name or description
    /// </summary>
    [Display(Name = "Search", Description = "Search term to filter by name or description")]
    public string? Search { get; set; }
    
    /// <summary>
    /// Filter by category
    /// </summary>
    [Display(Name = "Category", Description = "Filter by category")]
    public string? Category { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    [Display(Name = "IsActive", Description = "Filter by active status (true/false)")]
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Filter by construction item status: OPENING, PROCESSING, or DONE
    /// </summary>
    [Display(Name = "Status", Description = "Filter by construction item status: OPENING, PROCESSING, or DONE")]
    public string? Status { get; set; }
    
    /// <summary>
    /// Filter by payment status
    /// </summary>
    [Display(Name = "IsPayment", Description = "Filter by payment status (true/false)")]
    public bool? IsPayment { get; set; }
    
    /// <summary>
    /// If true, returns only child items; if false, returns only parent items
    /// </summary>
    /// <remarks>
    /// - When IsChild=true: Returns only child items (items with a parent)
    /// - When IsChild=false: Returns only parent items (items without a parent) with their children
    /// - When IsChild is not specified: Returns parent items with their children (default behavior)
    /// </remarks>
    [Display(Name = "IsChild", Description = "If true, returns only child items; if false, returns only parent items")]
    public bool? IsChild { get; set; }
    
    /// <summary>
    /// Builds the filter expression based on the provided criteria
    /// </summary>
    /// <returns>A LINQ expression for filtering construction items</returns>
    public override Expression<Func<ConstructionItem, bool>> GetExpressions()
    {
        Expression = PredicateBuilder.New<ConstructionItem>(true);
        
        // Filter by search term (name or description)
        if (!string.IsNullOrWhiteSpace(Search))
        {
            Expression = Expression.And(x => 
                (x.Name != null && x.Name.Contains(Search)) || 
                (x.Description != null && x.Description.Contains(Search))
            );
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(Category))
        {
            Expression = Expression.And(x => x.Category == Category);
        }
        
        // Filter by active status
        if (IsActive.HasValue)
        {
            Expression = Expression.And(x => x.IsActive == IsActive.Value);
        }
        
        // Filter by status
        if (!string.IsNullOrWhiteSpace(Status))
        {
            // Validate that the status is a valid enum value
            if (Enum.TryParse<EnumConstructionItemStatus>(Status, out var statusEnum))
            {
                Expression = Expression.And(x => x.Status == statusEnum.ToString());
            }
            else
            {
                // If not a valid enum value, use the string directly
                Expression = Expression.And(x => x.Status == Status);
            }
        }
        
        // Filter by payment status
        if (IsPayment.HasValue)
        {
            Expression = Expression.And(x => x.IsPayment == IsPayment.Value);
        }
        
        // Filter by parent/child status
        if (IsChild.HasValue)
        {
            if (IsChild.Value)
            {
                // If IsChild is true, get items with a ParentId (child items)
                Expression = Expression.And(x => x.ParentId != null);
            }
            else
            {
                // If IsChild is false, get items without a ParentId (parent items)
                Expression = Expression.And(x => x.ParentId == null);
            }
        }
        
        return Expression;
    }
}
