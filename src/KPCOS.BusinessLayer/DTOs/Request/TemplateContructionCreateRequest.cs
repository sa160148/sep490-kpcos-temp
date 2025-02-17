using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class TemplateContructionCreateRequest
{
    [Required(ErrorMessage = "không được để trống")]
    [DefaultValue("Template Thi Công Hồ Koi Chuẩn")]
    public string Name { get; set; }
    
    [DefaultValue("Template Thi Công Hồ Koi Chuẩn")]
    public string Description { get; set; }
}

public class TemplateContructionItemCreateRequest
{
    [Required(ErrorMessage = "không được để trống")]
    [DefaultValue("Đào đất")]
    public string Name { get; set; }
    
    [DefaultValue("Đào đất")]
    public string Description { get; set; }
    
    public Guid IdTemplateContruction { get; set; }

    public Guid? IdParent { get; set; } = null;


}