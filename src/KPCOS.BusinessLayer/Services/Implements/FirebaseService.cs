using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Exceptions;
using Microsoft.Extensions.Configuration;

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
            string authJsonFile = _config["FirebaseSettings:ConfigFile"];
            var appOptions = new AppOptions()
            {
                Credential = GoogleCredential.FromFile(authJsonFile)
            };

            _firebaseApp = FirebaseApp.Create(appOptions);
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

    public async Task<bool> IsContractOtpInFirestore(string contractId)
    {
        var docRef = _dbFirestore.Collection("otps").Document(contractId);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }
}