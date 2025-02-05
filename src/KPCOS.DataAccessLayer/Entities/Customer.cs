using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;


[Table("customer")]
public class Customer : BaseEntity
{
    [Column("point", TypeName = "integer")]
    public int Point { get; set; } = 0;

    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}