using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KPCOS.DataAccessLayer.Entities;

public class BaseEntity
{
    [Required, Key, Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required, Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Required, Column("updated_at", TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at", TypeName = "timestamp")]
    public DateTime? DeletedAt { get; set; }

    [Required, Column("is_active", TypeName = "boolean")]
    public bool IsActive { get; set; }

    /*[Column("created_by", TypeName = "uuid")]
    public Guid? ModifiedBy { get; set; }*/
}