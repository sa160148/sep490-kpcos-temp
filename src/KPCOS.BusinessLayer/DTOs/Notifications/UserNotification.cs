using System;
using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Notifications;

[FirestoreData]
public class UserNotification
{
    [FirestoreProperty]
    public int? TotalUnread { get; set; }
    [FirestoreProperty]
    public List<Notification> Notifications { get; set; }
}
