using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Feedbacks;

public class GetAllFeedbackFilterRequest : PaginationRequest<Feedback>
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public Guid? No { get; set; }
    public int? Rating { get; set; }
    public DateTime? FromCreatedAt { get; set; }
    public DateTime? ToCreatedAt { get; set; }

    public override Expression<Func<Feedback, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Feedback>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(feedback => feedback.Name.Contains(Search));
        }
        
        if (!string.IsNullOrEmpty(Type))
        {
            predicate = predicate.And(feedback => feedback.Type == Type);
        }
        
        if (No.HasValue)
        {
            predicate = predicate.And(feedback => feedback.No == No.Value);
        }
        
        if (Rating.HasValue)
        {
            predicate = predicate.And(feedback => feedback.Rating == Rating.Value);
        }

        if (FromCreatedAt.HasValue)
        {
            predicate = predicate.And(feedback => feedback.CreatedAt >= FromCreatedAt.Value);
        }

        if (ToCreatedAt.HasValue)
        {
            predicate = predicate.And(feedback => feedback.CreatedAt <= ToCreatedAt.Value);
        }

        return predicate;
    }
}
