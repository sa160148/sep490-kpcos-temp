using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Equipments;

public class GetAllEquipmentResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }        
    public decimal? Price { get; set; }
    public string? Note { get; set; }
    public string? Category { get; set; }
}
