using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("role")]
public class Role : BaseEntity
{
    [Column("name", TypeName = "character varying(128)")]
    public string Name { get; set; }

    [Column("description", TypeName = "character varying(256)")]
    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}