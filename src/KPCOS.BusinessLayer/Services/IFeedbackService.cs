using System;
using KPCOS.BusinessLayer.DTOs.Request.Feedbacks;
using KPCOS.BusinessLayer.DTOs.Response.Feedbacks;

namespace KPCOS.BusinessLayer.Services;

public interface IFeedbackService
{
    Task CreateFeedbackAsync(CommandFeedbackRequest request, Guid userId);
    Task UpdateFeedbackAsync(Guid id, CommandFeedbackRequest request, Guid userId);
    Task<(IEnumerable<GetAllFeedbackResponse> data, int total)> GetAllFeedbackAsync(GetAllFeedbackFilterRequest request, Guid? userId = null);
}
