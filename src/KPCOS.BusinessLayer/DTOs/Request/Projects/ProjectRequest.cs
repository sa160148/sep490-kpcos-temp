using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Projects;

public class ProjectRequest
{
    [DefaultValue("customer")]
    public string? CustomerName { get; set; }

    [DefaultValue("note")]
    public string? Note { get; set; }

    [DefaultValue("address")]
    public string? Address { get; set; }

    [DefaultValue("0123456789")]
    public string? Phone { get; set; }

    [DefaultValue("root@gmail.com")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "can not let area null")]
    [DefaultValue(100)]
    public double Area { get; set; }

    [Required(ErrorMessage = "can not let depth null")]
    [DefaultValue(100)]
    public double Depth { get; set; }

    [DefaultValue("5ca78687-26db-40ed-99d0-685dff2b7e3e")]
    public Guid? PackageId { get; set; }

    [DefaultValue("5ca78687-26db-40ed-99d0-685dff2b7e3e")]
    public Guid? Templatedesignid{ get; set; }
}