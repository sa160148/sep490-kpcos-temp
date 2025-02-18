using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class ProjectRequest
{
    [Required(ErrorMessage = "can not let name null")]
    [DefaultValue("project")]
    public string Name { get; set; }

    [Required(ErrorMessage = "can not let customer name null")]
    [DefaultValue("customer")]
    public string CustomerName { get; set; }

    [Required(ErrorMessage = "can not let note null")]
    [DefaultValue("note")]
    public string Note { get; set; }

    [Required(ErrorMessage = "can not let address null")]
    [DefaultValue("address")]
    public string Address { get; set; }

    [Required(ErrorMessage = "can not let phone null")]
    [DefaultValue("0123456789")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("root@gmail.com")]
    public string Email { get; set; }

    [Required(ErrorMessage = "can not let area null")]
    [DefaultValue(0)]
    public float Area { get; set; }

    [Required(ErrorMessage = "can not let depth null")]
    [DefaultValue(0)]
    public float Depth { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? PackageId { get; set; }

    public Guid? Templatedesignid{ get; set; }
}