using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Feedbacks;

public class CommandFeedbackRequest
{
    public Guid? No { get; set; }
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? Rating { get; set; }

    public string? Type { get; set; }
}
