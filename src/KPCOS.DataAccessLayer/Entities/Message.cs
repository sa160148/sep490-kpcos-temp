using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;

[Table("Message")]
public class Message : BaseEntity
{
    [Column("conversation_id", TypeName = "uuid")]
    public Guid ConversationId { get; set; }
    
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    [Column("content", TypeName = "character varying(1024)")]
    public string Content { get; set; }
    
}