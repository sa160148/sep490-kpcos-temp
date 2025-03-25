using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Projects;

public class GetAllProjectFilterRequest : PaginationRequest<Project>
{
    [Display(Name = "Trạng thái", Description = "Trạng thái của dự án")]
    public string? Status { get; set; }
    [Display(Name = "Tìm kiếm", Description = "Tìm kiếm dự án theo tên")]
    public string? Search { get; set; }
    [Display(Name = "Diện tích", Description = "Diện tích của dự án")]
    public double? Area { get; set; }
    [Display(Name = "Chiều sâu", Description = "Chiều sâu của dự án")]
    public double? Depth { get; set; }
    [Display(Name = "Giá tối thiểu", Description = "Dựa theo quotation confirmed")]
    public double? PriceMin { get; set; }
    [Display(Name = "Giá tối đa", Description = "Dựa theo quotation confirmed")]
    public double? PriceMax { get; set; }
    [Display(Name = "PackageIds", Description = "PackageIds")]
    public string? PackageIds { get; set; }
    [Display(Name = "Templatedesignids", Description = "Templatedesignids")]
    public string? Templatedesignids { get; set; }
    [Display(Name = "IsActive", Description = "IsActive")]
    public bool? IsActive { get; set; }

    public override Expression<Func<Project, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Project>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(p => p.Name.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Status))
        {
            predicate = predicate.And(p => p.Status == Status);
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

    public Expression<Func<Project, bool>> GetExpressionsV2(Guid userId, string role)
    {       
        var customerQueryExpression = PredicateBuilder.New<Project>(true);
        if (role == RoleEnum.ADMINISTRATOR.ToString())
        {
            return Expression = Expression.And(customerQueryExpression);
        }
        customerQueryExpression.Or(pro => pro.Customer.UserId == userId || 
                                          pro.ProjectStaffs.Any(ps => ps.Staff.UserId == userId) 
                                          // || pro.ProjectStaffs.Any(ps => ps.StaffId == userId && ps.Staff.Position == RoleEnum.ADMINISTRATOR.ToString())
                                          );
        return Expression = Expression.And(customerQueryExpression);
    }
}
