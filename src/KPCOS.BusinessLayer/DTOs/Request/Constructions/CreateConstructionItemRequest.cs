namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class CreateConstructionItemRequest
{
    public DateOnly EstDate { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsPayment { get; set; }
    public Guid? TemplateItemId { get; set; }
    public List<CreateConstructionItemRequest>? Childs { get; set; }
}