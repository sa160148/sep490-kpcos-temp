using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("User")]
public class User : BaseEntity
{
    [Required, MaxLength(128), Column("username", TypeName = "character varying(256)")]
    public string Username { get; set; }

    [Required, MaxLength(256), Column("password", TypeName = "character varying(256)")]
    public string Password { get; set; }

    [Required, MaxLength(128), Column("email", TypeName = "character varying(256)")]
    public string Email { get; set; }

    [Required, Column("role_id", TypeName = "uuid")]
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}