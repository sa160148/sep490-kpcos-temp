using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class ServiceRequest
{
}
public class ServiceCreateRequest
{
    [DefaultValue("")]
    public string name { get; set; }
    
    [DefaultValue("")]
    public string description { get; set; }
    
    [Required(ErrorMessage = "can not let price null")]
    [DefaultValue(0)]
    public int price { get; set; }
    
    [DefaultValue(0)]
    public string unit { get; set; }
    
    [DefaultValue(EnumService.Unit)]
    public string type { get; set; }
}

