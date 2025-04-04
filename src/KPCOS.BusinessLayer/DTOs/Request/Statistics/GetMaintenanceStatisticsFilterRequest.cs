using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Statistics;

public class GetMaintenanceStatisticsFilterRequest : PaginationRequest<MaintenanceRequest>
{
    public string? Year { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? FromTotal { get; set; }
    public int? ToTotal { get; set; }

    public override Expression<Func<MaintenanceRequest, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<MaintenanceRequest>(true);

        if (!string.IsNullOrEmpty(Year))
        {
            var years = Year.Split(',').ToList();
            predicate = predicate.And(x => years.Contains(
                x.CreatedAt.Value.Year.ToString()) && 
                x.Status == EnumMaintenanceRequestStatus.DONE.ToString() &&
                x.IsPaid == true
            );
        }

        if (FromDate != null)
        {
            predicate = predicate.And(
                x => x.CreatedAt.Value >= FromDate.Value.ToDateTime(TimeOnly.MinValue) &&
                x.Status == EnumMaintenanceRequestStatus.DONE.ToString() &&
                x.IsPaid == true
            );
        }

        if (ToDate != null)
        {
            predicate = predicate.And(
                x => x.CreatedAt.Value <= ToDate.Value.ToDateTime(TimeOnly.MaxValue) &&
                x.Status == EnumMaintenanceRequestStatus.DONE.ToString() &&
                x.IsPaid == true
            );
        }

        if (FromTotal != null)
        {
            predicate = predicate.And(
                x => x.TotalValue >= FromTotal.Value &&
                x.Status == EnumMaintenanceRequestStatus.DONE.ToString() &&
                x.IsPaid == true
            );
        }

        if (ToTotal != null)
        {
            predicate = predicate.And(
                x => x.TotalValue <= ToTotal.Value &&
                x.Status == EnumMaintenanceRequestStatus.DONE.ToString() &&
                x.IsPaid == true
            );
        }

        return predicate;
    }
}
