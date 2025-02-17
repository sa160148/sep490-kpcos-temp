namespace KPCOS.BusinessLayer.DTOs.Response;

public class TemplateContructionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public bool? IsActive { get; set; }
}
public class TemplateContructionDetailResponse : TemplateContructionResponse
{
    public List<TemplateContructionItemResponse> TemplateContructionItems { get; set; }
}
public class TemplateContructionItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public List<TemplateContructionItemResponse>? Child { get; set; }
}