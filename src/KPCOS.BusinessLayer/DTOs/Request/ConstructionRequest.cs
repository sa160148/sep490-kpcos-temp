namespace KPCOS.BusinessLayer.DTOs.Request;

public class ConstructionRequest
{
    
    public List<Item> Items { get; set; }
    public Guid ProjectId { get; set; }
    public class Item 
    {
        public Guid TemplateItemId { get; set; }
    
        public DateOnly EstDate { get; set; }
    
        public bool IsPayment { get; set; }
        
        public List<Item>? Child { get; set; }
    }
    
}

