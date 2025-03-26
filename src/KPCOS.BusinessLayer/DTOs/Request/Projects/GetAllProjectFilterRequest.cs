using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Projects;

/// <summary>
/// Filter criteria for retrieving projects
/// </summary>
public class GetAllProjectFilterRequest : PaginationRequest<Project>
{
    /// <summary>
    /// Filter by project status(comma-separated): OPENING,REQUESTING,PROCESSING,DESIGNING,CONSTRUCTING,FINISHED
    /// </summary>
    /// <remarks>
    /// Multiple statuses can be specified as a comma-separated list
    /// </remarks>
    [Display(Name = "Trạng thái", Description = "Filter by project status (comma-separated): REQUESTING, PROCESSING, DESIGNING, CONSTRUCTING, FINISHED")]
    public string? Status { get; set; }
    
    /// <summary>
    /// Search term to filter projects by name
    /// </summary>
    [Display(Name = "Tìm kiếm", Description = "Search term to filter projects by name")]
    public string? Search { get; set; }
    
    /// <summary>
    /// Filter by minimum project area in square meters
    /// </summary>
    [Display(Name = "Diện tích", Description = "Filter by minimum project area in square meters (e.g., 100)")]
    public double? Area { get; set; }
    
    /// <summary>
    /// Filter by minimum project depth in meters
    /// </summary>
    [Display(Name = "Chiều sâu", Description = "Filter by minimum project depth in meters (e.g., 3.5)")]
    public double? Depth { get; set; }
    
    /// <summary>
    /// Filter by minimum confirmed quotation price
    /// </summary>
    [Display(Name = "Giá tối thiểu", Description = "Filter by minimum confirmed quotation price (e.g., 10000)")]
    public double? PriceMin { get; set; }
    
    /// <summary>
    /// Filter by maximum confirmed quotation price
    /// </summary>
    [Display(Name = "Giá tối đa", Description = "Filter by maximum confirmed quotation price (e.g., 50000)")]
    public double? PriceMax { get; set; }
    
    /// <summary>
    /// Filter by specific package IDs (comma-separated GUIDs)
    /// </summary>
    [Display(Name = "PackageIds", Description = "Filter by comma-separated package GUIDs")]
    public string? PackageIds { get; set; }
    
    /// <summary>
    /// Filter by specific template design IDs (comma-separated GUIDs)
    /// </summary>
    [Display(Name = "Templatedesignids", Description = "Filter by comma-separated template design GUIDs")]
    public string? Templatedesignids { get; set; }
    
    /// <summary>
    /// Filter by active status
    /// </summary>
    [Display(Name = "IsActive", Description = "Filter by active status (true/false)")]
    public bool? IsActive { get; set; }

    /// <summary>
    /// Builds the filter expression based on the provided criteria
    /// </summary>
    /// <returns>A LINQ expression for filtering projects</returns>
    public override Expression<Func<Project, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Project>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(p => p.Name.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Status))
        {
            var statuses = Status.Split(',').ToList();
            predicate = predicate.And(p => statuses.Contains(p.Status));
        }

        if (IsActive.HasValue)
        {
            predicate = predicate.And(p => p.IsActive == IsActive.Value);
        }

        if (Area.HasValue)
        {
            predicate = predicate.And(p => p.Area >= Area.Value);
        }
        if (Depth.HasValue)
        {
            predicate = predicate.And(p => p.Depth >= Depth.Value);
        }
        if (PriceMin.HasValue)
        {
            predicate = predicate.And(p => 
            p.Quotations.Any(q => q.TotalPrice >= PriceMin.Value && q.Status == EnumQuotationStatus.CONFIRMED.ToString()));
        }
        if (PriceMax.HasValue)
        {
            predicate = predicate.And(p => 
            p.Quotations.Any(q => q.TotalPrice <= PriceMax.Value && q.Status == EnumQuotationStatus.CONFIRMED.ToString()));
        }
        if (!string.IsNullOrEmpty(PackageIds))
        {
            var packageIds = PackageIds.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(p => packageIds.Contains(p.PackageId));
        }
        if (!string.IsNullOrEmpty(Templatedesignids))
        {
            var templatedesignids = Templatedesignids.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(p => p.Templatedesignid.HasValue && templatedesignids.Contains(p.Templatedesignid.Value));
        }
        return predicate;
    }

    /// <summary>
    /// Builds a filter expression based on user ID and role for role-based access control
    /// </summary>
    /// <param name="userId">The user ID to filter by</param>
    /// <param name="role">The user role (ADMINISTRATOR, CUSTOMER, or staff role)</param>
    /// <returns>A LINQ expression for filtering projects based on user access rights</returns>
    /// <remarks>
    /// - ADMINISTRATOR: Can see all projects (no additional filtering)
    /// - CUSTOMER: Can only see their own projects
    /// - Staff roles: Can only see projects they are specifically assigned to with their role
    /// </remarks>
    public Expression<Func<Project, bool>> GetExpressionsV2(Guid userId, string role)
    {       
        var predicate = PredicateBuilder.New<Project>(true);
        
        if (role == RoleEnum.ADMINISTRATOR.ToString())
        {
            return predicate; // Administrator can see all projects
        }

        if (role == RoleEnum.CUSTOMER.ToString())
        {
            predicate = predicate.And(pro => pro.Customer.UserId == userId);
        }
        else
        {
            // For staff roles, only show projects they are specifically assigned to
            predicate = predicate.And(pro => 
                pro.ProjectStaffs.Any(ps => 
                    ps.Staff.UserId == userId && 
                    ps.Staff.Position == role
                )
            );
        }
        
        return predicate;
    }
}
