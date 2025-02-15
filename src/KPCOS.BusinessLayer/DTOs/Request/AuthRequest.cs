using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class AuthRequest
{
}

public class SigninRequest
{
    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("admin@mail.com")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "can not let password null")]
    [DefaultValue("123456")]
    public string Password { get; set; }
}

public class SignupRequest
{
    [Required(ErrorMessage = "can not let username null")]
    [Description("root")]
    public string FullName { get; set; }
    
    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("root@gmail.com")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "can not let password null")]
    public string Password { get; set; }

    [Required(ErrorMessage = "can not let confirm password null")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "can not let avatar null")]
    [DefaultValue("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTaotZTcu1CLMGOJMDl-f_LYBECs7tqwhgpXA&s")]
    public string Avatar { get; set; }

    [Required(ErrorMessage = "can not let phone null")]
    [DefaultValue("")]
    public string Phone { get; set; }
}