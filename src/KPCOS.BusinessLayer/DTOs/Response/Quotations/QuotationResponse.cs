using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.Equipments;
using KPCOS.BusinessLayer.DTOs.Response.Services;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.Quotations;

public class QuotationForProjectResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ProjectId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? TemplateConstructionId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }
    
    public int? TotalPrice { get; set; }
    
    public IEnumerable<GetAllStaffResponse>? Staffs { get; set; }
}

public class QuotationResponse : QuotationForProjectResponse
{
    public IEnumerable<GetAllServiceResponse> Services { get; set; } = new List<GetAllServiceResponse>();
    public IEnumerable<GetAllEquipmentResponse> Equipments { get; set; } = new List<GetAllEquipmentResponse>();
    
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public IEnumerable<GetAllStaffResponse>? Staffs { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Name { get; set; }
    
    public class Service
    {
        public Guid Id { get; set; }
        
        public int Quantity { get; set; }
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