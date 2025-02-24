using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response;

public class QuotationResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TemplateConstructionId { get; set; }
    public int Version { get; set; }
    public string CreatedDate { get; set; }
    public string UpdatedDate { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    
    public List<Service> Services { get; set; }
    public List<Equipment> Equipments { get; set; }
    
    
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
    }
    
    public class Equipment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
    }
    
}