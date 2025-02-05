using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("User")]
public class User : BaseEntity
{
    [Column("fullname", TypeName = "character varying(255)")]
    [Required]
    public string Fullname { get; set; }

    [Column("birthdate", TypeName = "date")]
    public DateTime? Birthdate { get; set; }

    [Column("address", TypeName = "text")]
    public string? Address { get; set; }

    [Column("gender", TypeName = "character varying(10)")]
    public string? Gender { get; set; }

    [Column("avatar", TypeName = "text")]
    public string Avatar { get; set; } = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTaotZTcu1CLMGOJMDl-f_LYBECs7tqwhgpXA&s";

    [Column("password", TypeName = "character varying(255)")]
    [Required]
    public string Password { get; set; }

    [Column("email", TypeName = "character varying(255)")]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Column("role_id", TypeName = "uuid")]
    public Guid RoleId { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }
        
    public virtual Customer Customer { get; set; }
}