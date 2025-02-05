using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("package")]
public class Package : BaseEntity
{
    [Column("name", TypeName = "character varying(255)")]
    [Required]
    public string Name { get; set; }

    [Column("rate", TypeName = "integer")]
    [Range(0, 10)]
    public int Rate { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("price", TypeName = "integer")]
    public int Price { get; set; }

    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
}