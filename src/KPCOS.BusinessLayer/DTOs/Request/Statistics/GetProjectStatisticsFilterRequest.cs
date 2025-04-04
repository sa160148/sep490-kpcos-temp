using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Statistics;

public class GetProjectStatisticsFilterRequest : PaginationRequest<Contract>
{
    public string? Year { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? FromTotal { get; set; }
    public int? ToTotal { get; set; }

    public override Expression<Func<Contract, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Contract>(true);
        
        if (!string.IsNullOrEmpty(Year))
        {
            var years = Year.Split(',').ToList();
            predicate = predicate.And(x => years.Contains(
                x.CreatedAt.Value.Year.ToString()) && 
                x.Status == EnumContractStatus.ACTIVE.ToString() &&
                x.PaymentBatches.All(y => y.IsPaid == true) &&
                x.Project.Status == EnumProjectStatus.FINISHED.ToString()
                );
        }

        if (FromDate != null)
        {
            predicate = predicate.And(
                x => x.CreatedAt.Value >= FromDate.Value.ToDateTime(TimeOnly.MinValue)
                && x.Status == EnumContractStatus.ACTIVE.ToString()
                && x.PaymentBatches.All(y => y.IsPaid == true)
                && x.Project.Status == EnumProjectStatus.FINISHED.ToString()
            );
        }
        
        if (ToDate != null)
        {
            predicate = predicate.And(
                x => x.CreatedAt.Value <= ToDate.Value.ToDateTime(TimeOnly.MaxValue)
                && x.Status == EnumContractStatus.ACTIVE.ToString()
                && x.PaymentBatches.All(y => y.IsPaid == true)
                && x.Project.Status == EnumProjectStatus.FINISHED.ToString()
            );
        }

        if (FromTotal != null)
        {
            predicate = predicate.And(
                x => x.ContractValue >= FromTotal.Value &&
                x.Status == EnumContractStatus.ACTIVE.ToString() &&
                x.PaymentBatches.All(y => y.IsPaid == true)
                && x.Project.Status == EnumProjectStatus.FINISHED.ToString()
            );
        }
        
        if (ToTotal != null)
        {
            predicate = predicate.And(
                x => x.ContractValue <= ToTotal.Value &&
                x.Status == EnumContractStatus.ACTIVE.ToString() &&
                x.PaymentBatches.All(y => y.IsPaid == true)
                && x.Project.Status == EnumProjectStatus.FINISHED.ToString()
            );
        }

        return predicate;
    }
}
