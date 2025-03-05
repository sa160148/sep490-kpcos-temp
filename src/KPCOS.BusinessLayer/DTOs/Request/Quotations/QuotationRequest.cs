namespace KPCOS.BusinessLayer.DTOs.Request;

public class QuotationRequest
{
    
}

public class QuotationCreateRequest
{
    public Guid ProjectId { get; set; }
    public Guid TemplateConstructionId { get; set; }
    public List<Service> Services { get; set; }
    public List<Equipment> Equipments { get; set; }
    
    public class Service 
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
    } 
    public class Equipment 
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string Category { get; set; }
    }
}

public class QuotationRejectOrAcceptRequest
{
    public bool IsAccept { get; set; }
    public string? Reason { get; set; }
}


public class QuotationApproveOrEditRequest
{
    public bool IsApprove { get; set; }
    public string? Reason { get; set; }
}