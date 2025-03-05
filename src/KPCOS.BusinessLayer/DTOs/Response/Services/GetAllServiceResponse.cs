using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Services;

public class GetAllServiceResponse
{
    public Guid Id { get; set; }
    public int? Quantity { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Price { get; set; }
    public string? Unit { get; set; }
    public string? Type { get; set; }
    public string? Note { get; set; }
    public string? Category { get; set; }
}
