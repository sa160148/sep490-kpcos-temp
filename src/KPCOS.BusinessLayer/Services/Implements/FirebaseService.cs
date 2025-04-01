using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using KPCOS.BusinessLayer.DTOs.Notifications;
using LinqKit;
using System.Linq.Expressions;

namespace KPCOS.BusinessLayer.Services.Implements;

public class FirebaseService : IFirebaseService
{
    private readonly IConfiguration _config;
    private readonly FirestoreDb _dbFirestore;
    private static FirebaseApp? _firebaseApp;
    private readonly IMapper _mapper;

    public FirebaseService(IConfiguration config, IMapper mapper)
    {
        _config = config;
        _mapper = mapper;
        if (_firebaseApp == null)
        {
            try
            {
                string authJsonFile = _config["FirebaseSettings:ConfigFile"];
                var appOptions = new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(authJsonFile)
                };

                _firebaseApp = FirebaseApp.Create(appOptions);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("The default FirebaseApp already exists"))
            {
                // If the app is already initialized, just get the instance
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }
        string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase_app_settings.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        _dbFirestore = FirestoreDb.Create("roomspt-37b2f");
    }
    
    /// <summary>
    /// Sample method to communicate to firestore.
    /// </summary>
    /// <param name="saveUser"></param>
    /// <param name="id"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public async Task<string> SaveUser(DataAccessLayer.Entities.User saveUser, Guid id, string collectionName)
    {
        try
        {
            var saveUserResponse = _mapper.Map<UserResponse>(saveUser);
            DocumentReference docRef = _dbFirestore.Collection(collectionName).Document(id.ToString());
            Console.WriteLine("docRef: " + docRef);
            await docRef.SetAsync(saveUserResponse);
            return (await docRef.GetSnapshotAsync()).UpdateTime.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving document: {e.Message}");
            throw;
        }
    }

    #region Contract
    /// <summary>
    /// Save contract otp.
    /// <para>Set a new of otp document</para>
    /// </summary>
    /// <param name="otpResponse"></param>
    /// <exception cref="AppException"></exception>
    public async Task SaveContractOtpAsync(OtpResponse otpResponse)
    {
        try
        {
            DocumentReference docRef = _dbFirestore.Collection("otps").Document(otpResponse.ContractId);
            await docRef.SetAsync(otpResponse, SetOptions.Overwrite);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving document for otp: {e.Message}");
            throw new AppException("Lỗi xảy ra khi lưu mã OTP xác nhận lên hệ thống");
        }
    }

    /// <summary>
    /// Delete contract otp.
    /// <para>Delete contract when IsActive not true</para>
    /// </summary>
    /// <param name="contractId"></param>
    /// <exception cref="BadRequestException"></exception>
    public async Task DeleteContractOtpAsync(string contractId)
    {
        try
        {
            var snapshot = await GetContractOtpAsync(contractId);
            if (snapshot.IsActive == true)
            {
                return;
            }
            DocumentReference docRef = _dbFirestore.Collection("otps").Document(contractId);
            await docRef.DeleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi xảy ra khi xóa mã OTP xác nhận: {e.Message}");
            throw new BadRequestException("Lỗi xảy ra khi xóa mã OTP xác nhận");
        }
    }

    /// <summary>
    /// Update contract otp.
    /// <para>Update the contract otp with the value IsActive = true.</para>
    /// If the contract otp have IsActive is already true, it will return.
    /// </summary>
    /// <param name="contractId"></param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi cập nhật mã OTP xác nhận</exception>
    public async Task UpdateContractOtpAsync(string contractId)
    {
        try
        {
            var snapshot = await GetContractOtpAsync(contractId);
            if (snapshot.IsActive)
            {
                return;
            }
            DocumentReference docRef = _dbFirestore.Collection("otps").Document(contractId);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                {"IsActive", true}
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi xảy ra khi cập nhật mã OTP xác nhận: {e.Message}");
            throw new BadRequestException("Lỗi xảy ra khi cập nhật mã OTP xác nhận");
        }
    }

    /// <summary>
    /// Get the contract otp from firestore.
    /// </summary>
    /// <param name="contractId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Không tìm thấy mã OTP xác nhận trên hệ thống</exception>
    public async Task<OtpResponse> GetContractOtpAsync(string contractId)
    {
        var docRef = _dbFirestore.Collection("otps").Document(contractId);
        var snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<OtpResponse>();
        }
        throw new NotFoundException("Không tìm thấy mã OTP xác nhận trên hệ thống");
    }

    /// <summary>
    /// Check the contract otp is existed in firestore.
    /// <para>return true when exited.</para>
    /// </summary>
    /// <param name="contractId"></param>
    /// <returns></returns>
    public async Task<bool> IsContractOtpInFirestore(string contractId)
    {
        var docRef = _dbFirestore.Collection("otps").Document(contractId);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }
    #endregion

    #region Docs
    /// <summary>
    /// Save doc otp.
    /// <para>Set a new of otp document</para>
    /// </summary>
    /// <param name="otpResponse"></param>
    /// <exception cref="AppException"></exception>
    public async Task SaveDocOtpAsync(DocOtpResponse otpResponse)
    {
        try
        {
            DocumentReference docRef = _dbFirestore.Collection("docs").Document(otpResponse.DocId);
            await docRef.SetAsync(otpResponse, SetOptions.Overwrite);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving document for otp: {e.Message}");
            throw new AppException("Lỗi xảy ra khi lưu mã OTP xác nhận lên hệ thống");
        }
    }

    /// <summary>
    /// Delete doc otp.
    /// <para>Delete doc when IsActive not true</para>
    /// </summary>
    /// <param name="docId"></param>
    /// <exception cref="BadRequestException"></exception>
    public async Task DeleteDocOtpAsync(string docId)
    {
        try
        {
            var snapshot = await GetDocOtpAsync(docId);
            if (snapshot.IsActive == true)
            {
                return;
            }
            DocumentReference docRef = _dbFirestore.Collection("docs").Document(docId);
            await docRef.DeleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi xảy ra khi xóa mã OTP xác nhận: {e.Message}");
            throw new BadRequestException("Lỗi xảy ra khi xóa mã OTP xác nhận");
        }
    }

    /// <summary>
    /// Update doc otp.
    /// <para>Update the doc otp with the value IsActive = true.</para>
    /// If the doc otp have IsActive is already true, it will return.
    /// </summary>
    /// <param name="docId"></param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi cập nhật mã OTP xác nhận</exception>
    public async Task UpdateDocOtpAsync(string docId)
    {
        try
        {
            var snapshot = await GetDocOtpAsync(docId);
            if (snapshot.IsActive)
            {
                return;
            }
            DocumentReference docRef = _dbFirestore.Collection("docs").Document(docId);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                {"IsActive", true}
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi xảy ra khi cập nhật mã OTP xác nhận: {e.Message}");
            throw new BadRequestException("Lỗi xảy ra khi cập nhật mã OTP xác nhận");
        }
    }

    /// <summary>
    /// Get the doc otp from firestore.
    /// </summary>
    /// <param name="docId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Không tìm thấy mã OTP xác nhận trên hệ thống</exception>
    public async Task<DocOtpResponse> GetDocOtpAsync(string docId)
    {
        var docRef = _dbFirestore.Collection("docs").Document(docId);
        var snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<DocOtpResponse>();
        }
        throw new NotFoundException("Không tìm thấy mã OTP xác nhận trên hệ thống");
    }

    /// <summary>
    /// Check the doc otp is existed in firestore.
    /// <para>return true when exited.</para>
    /// </summary>
    /// <param name="docId"></param>
    /// <returns></returns>
    public async Task<bool> IsDocOtpInFirestore(string docId)
    {
        var docRef = _dbFirestore.Collection("docs").Document(docId);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }
    #endregion

    #region Notifications
    /// <summary>
    /// Create a new notification in Firestore
    /// </summary>
    /// <param name="notification">Notification to save</param>
    /// <exception cref="AppException">Error creating notification in Firestore</exception>
    public async Task CreateNotificationAsync(Notification notification)
    {
        try
        {
            DocumentReference docRef = _dbFirestore.Collection("notifications").Document(notification.Id);
            await docRef.SetAsync(notification);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating notification: {e.Message}");
            throw new AppException("Error creating notification in Firestore");
        }
    }
    
    /// <summary>
    /// Get a notification by ID
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification if found</returns>
    /// <exception cref="NotFoundException">Notification not found</exception>
    public async Task<Notification> GetNotificationByIdAsync(string id)
    {
        try
        {
            DocumentReference docRef = _dbFirestore.Collection("notifications").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                throw new NotFoundException($"Notification with ID {id} not found");
            }
            
            return snapshot.ConvertTo<Notification>();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting notification by ID: {e.Message}");
            throw new AppException($"Error retrieving notification {id} from Firestore");
        }
    }
    
    /// <summary>
    /// Get notifications with filtering
    /// </summary>
    /// <param name="filter">Expression to filter notifications</param>
    /// <returns>Collection of notifications and total count</returns>
    /// <exception cref="AppException">Error retrieving notifications</exception>
    public async Task<(IEnumerable<Notification> notifications, int total)> GetNotificationsAsync(
        Expression<Func<Notification, bool>> filter = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            // Reference to the notifications collection
            CollectionReference notificationsRef = _dbFirestore.Collection("notifications");
            
            // Execute query to get all notifications (Firestore doesn't support LINQ expressions directly)
            QuerySnapshot snapshot = await notificationsRef.OrderByDescending("CreatedAt").GetSnapshotAsync();
            
            // Convert to Notification objects
            var notifications = snapshot.Documents.Select(doc => doc.ConvertTo<Notification>()).ToList();
            
            // Apply filter in memory if provided
            if (filter != null)
            {
                var compiledFilter = filter.Compile();
                notifications = notifications.Where(compiledFilter).ToList();
            }
            
            // Get total count
            int totalCount = notifications.Count;
            
            // Apply pagination
            notifications = notifications
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (notifications, totalCount);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting notifications: {e.Message}");
            throw new AppException("Error retrieving notifications from Firestore");
        }
    }
    
    /// <summary>
    /// Update a notification's read status
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="isRead">Whether the notification is read</param>
    /// <exception cref="NotFoundException">Notification not found</exception>
    /// <exception cref="AppException">Error updating notification</exception>
    public async Task UpdateNotificationReadStatusAsync(string id, bool isRead)
    {
        try
        {
            DocumentReference docRef = _dbFirestore.Collection("notifications").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
            {
                throw new NotFoundException($"Notification with ID {id} not found");
            }
            
            await docRef.UpdateAsync("IsRead", isRead);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating notification read status: {e.Message}");
            throw new AppException($"Error updating notification {id} in Firestore");
        }
    }
    #endregion
}