using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Maintenances;

public class GetAllMaintenanceRequestTaskFilterRequest : PaginationRequest<MaintenanceRequestTask>
{
    public string? Search { get; set; }
    public Guid? MaintenanceRequestId { get; set; }
    public string? Status { get; set; }
    public string? MaintenanceItemIds { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public override Expression<Func<MaintenanceRequestTask, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<MaintenanceRequestTask>(true);
        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search));
        }
        if (MaintenanceRequestId.HasValue)
        {
            predicate = predicate.And(x => x.MaintenanceRequestId == MaintenanceRequestId.Value);
        }
        if (!string.IsNullOrEmpty(Status))
        {
            predicate = predicate.And(x => x.Status == Status);
        }
        if (!string.IsNullOrEmpty(MaintenanceItemIds))
        {
            var maintenanceItemIds = MaintenanceItemIds.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(x => maintenanceItemIds.Contains(x.MaintenanceItemId.Value));
        }
        if (From.HasValue)
        {
            predicate = predicate.And(x => x.EstimateAt >= From.Value);
        }
        if (To.HasValue)
        {
            predicate = predicate.And(x => x.EstimateAt <= To.Value);
        }
        return predicate;
    }
}
