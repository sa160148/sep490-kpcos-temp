using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.DataAccessLayer.DTOs.Request;

public class ServiceRequest
{
}
public class ServiceCreateRequest
{
    [DefaultValue("")]
    public string Name { get; set; }

    [DefaultValue("")]
    public string Description { get; set; }

    [Required(ErrorMessage = "can not let price null")]
    [DefaultValue(0)]
    public int Price { get; set; }

    [DefaultValue(0)]
    public string Unit { get; set; }

    [DefaultValue(EnumService.Unit)]
    public string Type { get; set; }
}

