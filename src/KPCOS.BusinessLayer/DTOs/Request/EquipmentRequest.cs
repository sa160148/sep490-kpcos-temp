using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class EquipmentRequest
{
    
}

public class EquipmentCreateRequest
{
    [Required(ErrorMessage = "không được để trống")]
    [DefaultValue("Thùng lọc nước hồ cá Koi MPF-5000")]
    public string Name { get; set; }
    
    [DefaultValue("Thùng lọc nước hồ cá Koi MPF-5000")]
    public string Description { get; set; }
}