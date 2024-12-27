using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("Conversation")]
public class Conversation : BaseEntity
{
    [Column("name", TypeName = "character varying(128)")]
    public string Name { get; set; }
 //   type VARCHAR(50) NOT NULL CHECK (type IN ('PRIVATE', 'GROUP')), 
    [Column("type", TypeName = "character varying(256)")]
    public string Type { get; set; }
}