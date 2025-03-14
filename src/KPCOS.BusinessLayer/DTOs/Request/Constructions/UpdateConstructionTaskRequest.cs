using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class UpdateConstructionTaskRequest : CreateConstructionTaskRequest
{
    public Guid? StaffId { get; set; }
    public string? Reason { get; set; }
    public string? ImageUrl { get; set; }
}
