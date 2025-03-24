using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Maintenances;

public class GetAllMaintenanceRequestFilterRequest : PaginationRequest<MaintenanceRequest>
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }

    public double? Area { get; set; }
    public double? Depth { get; set; }
    public double? Volume { get; set; }
    public int? TotalValue { get; set; }
    public bool? IsPaid { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public int? Duration { get; set; }
    
    public override Expression<Func<MaintenanceRequest, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<MaintenanceRequest>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Status))
        {
            predicate = predicate.And(x => x.Status == Status);
        }
        
        if (!string.IsNullOrEmpty(Type))
        {
            predicate = predicate.And(x => x.Type == Type);
        }

        if (Area != null)
        {
            predicate = predicate.And(x => x.Area == Area);
        }   

        if (Depth != null)
        {
            predicate = predicate.And(x => x.Depth == Depth);
        }   

        if (Volume != null)
        {
            predicate = predicate.And(x => x.Depth * x.Area == Volume);
        }

        if (TotalValue != null)
        {
            predicate = predicate.And(x => x.TotalValue == TotalValue);
        }

        if (IsPaid != null)
        {
            predicate = predicate.And(x => x.IsPaid == IsPaid);
        }

        if (Duration != null)
        {
            predicate = predicate.And(x => x.MaintenanceRequestTasks.Count() == Duration);
        }

        return predicate;
    }
}
