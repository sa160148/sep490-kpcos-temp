using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Designs;

public class CreateDesignRequest
{
    [Required(ErrorMessage = "can not let Type null")]
    public Guid ProjectId { get; set; }
    [Required(ErrorMessage = "can not let Type null")]
    [DefaultValue("3D")]
    public string Type { get; set; } = null!;
    public List<DesignImageRequest> DesignImages { get; set; }
}