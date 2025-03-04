using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.DTOs.Response.Users;

public class GetAllStaffForDesignResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Position { get; set; }
    public string Avatar { get; set; }
}