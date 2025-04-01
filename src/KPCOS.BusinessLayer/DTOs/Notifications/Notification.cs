using System;
using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Notifications;

[FirestoreData]
public class Notification
{
    [FirestoreProperty]
    public string Id { get; set; }
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public string? Description { get; set; }
    [FirestoreProperty]
    public string? Link { get; set; }
    /// <summary>
    /// Type của thông báo, ví dụ: promotion, project, ...
    /// </summary>
    [FirestoreProperty]
    public string? Type { get; set; }
    /// <summary>
    /// Id của đối tượng được trích dẫn kèm theo trong thông báo, ví dụ: id của promotion
    /// </summary>
    [FirestoreProperty]
    public string? No { get; set; }
    /// <summary>
    /// Id của người nhận thông báo, userId
    /// </summary>
    [FirestoreProperty]
    public string? RecipientId { get; set; }
    [FirestoreProperty]
    public DateTime? CreatedAt { get; set; }
    [FirestoreProperty]
    public bool? IsActive { get; set; }
    [FirestoreProperty]
    public bool? IsRead { get; set; }
}
