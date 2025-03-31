using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.Common.Exceptions;

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
    /// <param name="request">Contract creation request containing project ID, quotation ID, and other contract details</param>
    /// <remarks>
    /// <para>This method:</para>
    /// <list type="bullet">
    ///     <item><description>Validates that the project exists</description></item>
    ///     <item><description>Validates that the quotation exists and has APPROVED status</description></item>
    ///     <item><description>Creates a new contract with the provided details</description></item>
    ///     <item><description>Applies any promotional discount if the quotation has an associated promotion</description></item>
    ///     <item><description>Adds promotion information to the contract notes if a promotion was applied</description></item>
    ///     <item><description>Creates 4 payment batches automatically:</description>
    ///         <list type="bullet">
    ///             <item><description>Deposit payment batch (25% of contract value)</description></item>
    ///             <item><description>Pre-constructing payment batch (25% of contract value)</description></item>
    ///             <item><description>Constructing payment batch (25% of contract value)</description></item>
    ///             <item><description>Acceptance payment batch (25% of contract value)</description></item>
    ///         </list>
    ///     </item>
    ///     <item><description>Links payment batches to construction items with IsPayment=true, ordered by EstimateAt date</description></item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when project or quotation is not found</exception>
    /// <exception cref="BadRequestException">Thrown when quotation status is not APPROVED</exception>
    Task CreateContractAsync(ContractRequest request);

    Task<GetContractDetailResponse> GetContractDetailAsync(Guid id);
}