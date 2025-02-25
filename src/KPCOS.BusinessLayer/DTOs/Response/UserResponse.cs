using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace KPCOS.BusinessLayer.DTOs.Response;

[FirestoreData]
public class UserResponse
{
    /// <summary>
    /// Id here is from table User that get userId
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [FirestoreProperty]
    public string? FullName{ get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [FirestoreProperty]
    public string? Email { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [FirestoreProperty]
    public string? Position { get; set; }
}

public class StaffResponse : UserResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsActive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Avatar { get; set; }
}