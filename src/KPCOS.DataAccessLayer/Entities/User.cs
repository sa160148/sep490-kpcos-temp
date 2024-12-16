namespace KPCOS.DataAccessLayer.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public Guid RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;
}