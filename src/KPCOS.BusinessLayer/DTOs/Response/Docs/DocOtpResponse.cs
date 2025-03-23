using System;
using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Response.Docs;

[FirestoreData]
public class DocOtpResponse
{
    [FirestoreProperty]
    public string? DocId { get; set; }
    
    [FirestoreProperty]
    public string? OtpCode { get; set; }
    
    [FirestoreProperty]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    [FirestoreProperty]
    public bool IsActive { get; set; } = false;
}
