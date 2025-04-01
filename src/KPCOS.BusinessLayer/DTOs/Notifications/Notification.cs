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
    /// <summary>
    /// Id của người nhận thông báo, userId
    /// </summary>
    [FirestoreProperty]
    public string? RecipientId { get; set; }
    [FirestoreProperty]
    public DateTime? CreatedAt { get; set; }
    [FirestoreProperty]
    public bool? IsRead { get; set; }
}
