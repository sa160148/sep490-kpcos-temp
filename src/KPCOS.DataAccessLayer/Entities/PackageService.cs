using System.ComponentModel.DataAnnotations.Schema;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.DataAccessLayer.Entities;


[Table("package_service")]
public class PackageService : BaseEntity
{
    [Column("price", TypeName = "integer")]
    public int Price { get; set; }

    [Column("category", TypeName = "enumCategory")]
    public EnumCategory Category { get; set; }

    [Column("quantity", TypeName = "integer")]
    public int Quantity { get; set; }

    [Column("amount", TypeName = "integer")]
    public int Amount { get; set; }

    [Column("package_id", TypeName = "uuid")]
    public Guid PackageId { get; set; }

    [ForeignKey("PackageId")]
    public virtual Package Package { get; set; }

    [Column("service_id", TypeName = "uuid")]
    public Guid ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public virtual Service Service { get; set; }
}