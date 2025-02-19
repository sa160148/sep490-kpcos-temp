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
    [DefaultValue("string")]
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

    [Required(ErrorMessage = "can not let phone null")]
    [DefaultValue("0123456789")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "can not let address null")]
    [DefaultValue("Hanoi")]
    public string Address { get; set; }

    [Required(ErrorMessage = "can not let gender null")]
    [DefaultValue("MALE")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "can not let birthday null")]
    [DefaultValue("2022-01-01")]
    public DateOnly Dob { get; set; }
}