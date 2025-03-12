using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class UpdateConstructionItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsPayment { get; set; }
    public bool? IsActive { get; set; }
    public DateOnly? ActualAt { get; set; }
    public string? Status { get; set; }
}
