using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.MaintenanceRequestIssues
{
    public class GetAllMaintenanceRequestIssueFilterRequest : PaginationRequest<MaintenanceRequestIssue>
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public Guid? MaintenanceRequestId { get; set; }
        public Guid? Id { get; set; }
        public override Expression<Func<MaintenanceRequestIssue, bool>> GetExpressions()
        {
            var predicate = PredicateBuilder.New<MaintenanceRequestIssue>(true);
            
            if (!string.IsNullOrEmpty(Search))
            {
                predicate = predicate.And(x => x.Name.Contains(Search) ||
                                               x.Description.Contains(Search));
            }
            if (MaintenanceRequestId.HasValue)
            {
                predicate = predicate.And(x => x.MaintenanceRequestId == MaintenanceRequestId.Value);
            }
            if (Id.HasValue)
            {
                predicate = predicate.And(x => x.Id == Id.Value);
            }
            if (!string.IsNullOrEmpty(Status))
            {
                var statuses = Status.Split(',').ToList();
                predicate = predicate.And(x => x.Status == Status);
            }
            if (IsActive.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == IsActive.Value);
            }
            return predicate;
        }
    }
}