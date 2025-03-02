using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Designs;

public class DesignImageRequest
{
    [Required(ErrorMessage = "can not let ImageUrl null")]
    [DefaultValue("https://upload.wikimedia.org/wikipedia/commons/a/a7/Blank_image.jpg")]
    public string ImageUrl { get; set; } = null!;
}