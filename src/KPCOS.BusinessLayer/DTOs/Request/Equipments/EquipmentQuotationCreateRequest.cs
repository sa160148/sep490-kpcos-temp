namespace KPCOS.BusinessLayer.DTOs.Request.Equipments;

public class EquipmentQuotationCreateRequest
{
    public Guid Id { get; set; }
    public string Note { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public string Category { get; set; }
}