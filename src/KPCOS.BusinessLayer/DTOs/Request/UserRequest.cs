using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class UserRequest
{
    [Required(ErrorMessage = "can not let username null")]
    [Description("root")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("root@gmail.com")]
    public string Email { get; set; }

    [Required(ErrorMessage = "can not let password null")]
    public string Password { get; set; }

    [Required(ErrorMessage = "can not let phone null")]
    [DefaultValue("0123456789")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "can not let position null")]
    [DefaultValue(RoleEnum.CONSTRUCTOR)]
    public RoleEnum Position{ get; set; }

}