using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Users;

public class CommandUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Gender { get; set; }
    public string? Position { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public DateOnly? Dob { get; set; }
    public bool? IsActive { get; set; }
}
