using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.DataAccessLayer.Entities;


[Table("service")]
public class Service : BaseEntity
{
    [Column("name", TypeName = "character varying(255)")]
    [Required]
    public string Name { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("price", TypeName = "integer")]
    public int Price { get; set; }

    [Column("unit", TypeName = "character varying(255)")]
    public string Unit { get; set; }

    [Column("type", TypeName = "integer")]
    public int Type { get; set; }
}