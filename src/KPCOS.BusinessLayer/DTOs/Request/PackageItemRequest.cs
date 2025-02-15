using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class PackageItemRequest
{
}
public class PackageItemCreateRequest
{
    [Required(ErrorMessage = "không được để trống")]
    [DefaultValue("Hệ thống lọc")]
    public string Name { get; set; }
}