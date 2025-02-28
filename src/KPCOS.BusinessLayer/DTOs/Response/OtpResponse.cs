using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Response;

[FirestoreData]
public class OtpResponse
{
    [FirestoreProperty]
    public string? ContractId { get; set; }
    
    [FirestoreProperty]
    public string? OtpCode { get; set; }
    
    [FirestoreProperty]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    [FirestoreProperty]
    public bool IsActive { get; set; } = false;
}