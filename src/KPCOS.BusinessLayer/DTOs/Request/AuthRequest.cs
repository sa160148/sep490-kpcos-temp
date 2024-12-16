using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class AuthRequest
{
}

public class SigninRequest
{
    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("root@gmail.com")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "can not let password null")]
    public string Password { get; set; }
}

public class SignupRequest
{
    [Required(ErrorMessage = "can not let username null")]
    [Description("root")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "can not let email null")]
    [DefaultValue("root@gmail.com")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "can not let password null")]
    public string Password { get; set; }
    
}