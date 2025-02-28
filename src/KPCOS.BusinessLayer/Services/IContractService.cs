using KPCOS.BusinessLayer.DTOs.Request.Contracts;

namespace KPCOS.BusinessLayer.Services;

public interface IContractService
{
    /// <summary>
    /// Accept a contract, set status to Active, and set contract otp field IsActive to true.
    /// <para>Need contract id and opt code that already do by verifying the accept action from customer.</para>
    /// This function will set IsActive to true on contract otp from firestore and contract status to ACTIVE after put the opt code.
    /// </summary>
    /// <param name="contractId">guid</param>
    /// <param name="otpCode">string</param>
    /// <exception cref="NotFoundException">Hợp đồng không tồn tại, Mã OTP không hợp lệ hoặc đã hết hạn</exception>
    Task AcceptContract(Guid contractId, string otpCode);
    
    /// <summary>
    /// Reject a contract, set status to Cancelled
    /// <para>Can not reject a contract if the otp exited in firestore have IsActive = true</para>
    /// </summary>
    /// <param name="contractId">guid</param>
    /// <exception cref="NotFoundException">Hợp đồng không tồn tại</exception>
    /// <exception cref="BadRequestException">Hợp đồng đã được xác nhận, không thể hủy</exception>
    Task RejectContract(Guid contractId);
    
    /// <summary>
    /// Verifying contract by contract id and user id.
    /// <para>This function will create a new contract otp and save to firestore with the otp verifying time to 5 minutes after call this function.</para>
    /// Also this function will send email to the user with the otp code and the time that the otp will be expired.
    /// <para>After 5 minutes without calling AcceptContract function to verifying, the contract otp from firestore will be deleted.</para>
    /// </summary>
    /// <param name="contractId">guid</param>
    /// <param name="userId">guid</param>
    Task VerifyingContract(Guid contractId, Guid userId);

    /// <summary>
    /// Create a new contract.
    /// <para>Can do this when quotation that have status is APPROVED</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task CreateContractAsync(ContractRequest request);
}