using System.ComponentModel.DataAnnotations.Schema;

namespace KPCOS.DataAccessLayer.Entities;


[Table("Conversation_Participants")]
public class Conversation_Participants : BaseEntity
{
    [Column("conversation_id", TypeName = "uuid")]
    public Guid ConversationId { get; set; }

    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
}