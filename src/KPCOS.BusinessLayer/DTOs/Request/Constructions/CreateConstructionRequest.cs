namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class CreateConstructionRequest
{
    public Guid ProjectId { get; set; }
    public List<CreateConstructionItemRequest> Items { get; set; }
}