using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Notifications;

public class CommandNotificationRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Link { get; set; }
    public string? Type { get; set; }
    public Guid? No { get; set; }
    public Guid? RecipientId { get; set; }
}
