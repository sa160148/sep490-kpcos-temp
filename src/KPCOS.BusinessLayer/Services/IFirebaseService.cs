using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Docs;

namespace KPCOS.BusinessLayer.Services;

public interface IFirebaseService
{
    /// <summary>
    /// Save contract otp.
    /// <para>Set a new of otp document</para>
    /// </summary>
    /// <param name="otpResponse"></param>
    /// <exception cref="AppException">Lỗi xảy ra khi lưu mã OTP xác nhận lên hệ thống</exception>
    Task SaveContractOtpAsync(OtpResponse otpResponse);
    
    /// <summary>
    /// Delete contract otp.
    /// <para>Delete contract when IsActive not true</para>
    /// </summary>
    /// <param name="contractId">string</param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi xóa mã OTP xác nhận</exception>
    Task DeleteContractOtpAsync(string contractId);
    
    /// <summary>
    /// Update contract otp.
    /// <para>Update the contract otp with the value IsActive = true.</para>
    /// If the contract otp have IsActive is already true, it will return.
    /// </summary>
    /// <param name="contractId">string</param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi cập nhật mã OTP xác nhận</exception>
    Task UpdateContractOtpAsync(string contractId);
    
    /// Get the contract otp from firestore.
    /// </summary>
    /// <param name="contractId">string</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Không tìm thấy mã OTP xác nhận trên hệ thống</exception>
    Task<OtpResponse> GetContractOtpAsync(string contractId);
    
    /// <summary>
    /// Check the contract otp is existed in firestore.
    /// <para>return true when exited.</para>
    /// </summary>
    /// <param name="contractId">string</param>
    /// <returns></returns>
    Task<bool> IsContractOtpInFirestore(string contractId);

    #region Docs
    /// <summary>
    /// Save doc otp.
    /// <para>Set a new of otp document</para>
    /// </summary>
    /// <param name="otpResponse"></param>
    /// <exception cref="AppException">Lỗi xảy ra khi lưu mã OTP xác nhận lên hệ thống</exception>
    Task SaveDocOtpAsync(DocOtpResponse otpResponse);
    /// <summary>
    /// Delete doc otp.
    /// <para>Delete doc when IsActive not true</para>
    /// </summary>
    /// <param name="docId">string</param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi xóa mã OTP xác nhận</exception>
    Task DeleteDocOtpAsync(string docId);
    /// <summary>
    /// Update doc otp.
    /// <para>Update the doc otp with the value IsActive = true.</para>
    /// If the doc otp have IsActive is already true, it will return.
    /// </summary>
    /// <param name="docId">string</param>
    /// <exception cref="BadRequestException">Lỗi xảy ra khi cập nhật mã OTP xác nhận</exception>
    Task UpdateDocOtpAsync(string docId);
    /// <summary>
    /// Get the doc otp from firestore.
    /// </summary>
    /// <param name="docId">string</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Không tìm thấy mã OTP xác nhận trên hệ thống</exception>
    Task<DocOtpResponse> GetDocOtpAsync(string docId);
    /// <summary>
    /// Check the doc otp is existed in firestore.
    /// <para>return true when exited.</para>
    /// </summary>
    /// <param name="docId">string</param>
    /// <returns></returns>
    Task<bool> IsDocOtpInFirestore(string docId);
    #endregion
}