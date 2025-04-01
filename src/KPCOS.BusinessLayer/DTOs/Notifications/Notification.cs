using System;
using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Notifications;

[FirestoreData]
public class Notification
{
    [FirestoreProperty]
    public Guid Id { get; set; }
    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public string? Description { get; set; }
    [FirestoreProperty]
    public string? Link { get; set; }
    [FirestoreProperty]
    public string? Type { get; set; }
    [FirestoreProperty]
    public string? Status { get; set; }
    [FirestoreProperty]
    public Guid? No { get; set; }
    [FirestoreProperty]
    public DateTime? CreatedAt { get; set; }
    [FirestoreProperty]
    public DateTime? UpdatedAt { get; set; }
    [FirestoreProperty]
    public bool? IsRead { get; set; }
}
