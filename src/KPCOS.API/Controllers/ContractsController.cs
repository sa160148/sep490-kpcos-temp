using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;

/// <summary>
/// Controller for managing contract operations including acceptance and verification workflow
/// </summary>
[Route("api/[controller]")]
public class ContractsController : BaseController
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    /// <summary>
    /// Creates a new contract with automatic payment batches
    /// </summary>
    /// <param name="request">Contract creation request</param>
    /// <remarks>
    /// Creates a new contract based on an approved quotation and automatically generates payment batches.
    /// 
    /// The contract creation process:
    /// 1. Validates that the project exists
    /// 2. Validates that the quotation exists and has APPROVED status
    /// 3. Creates a new contract with the provided details
    /// 4. Applies any promotion discount from the quotation to the contract value
    /// 5. Adds promotion information to the contract notes if a promotion is applied
    /// 6. Automatically creates 4 payment batches (each 25% of contract value):
    ///    * Deposit payment batch
    ///    * Pre-constructing payment batch
    ///    * Constructing payment batch
    ///    * Acceptance payment batch
    /// 7. Links payment batches to construction items with IsPayment=true, ordered by EstimateAt date
    /// 8. Validate if the project has construction items, if not, throw an error
    /// 
    /// Sample request:
    /// ```json
    /// {
    ///   "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "quotationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "Contract for Project X",
    ///   "customerName": "Customer Name",
    ///   "contractValue": 100000000,
    ///   "url": "https://example.com/contract.pdf",
    ///   "note": "Additional notes about the contract"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Contract created successfully with payment batches</response>
    /// <response code="400">Invalid request or quotation not approved</response>
    /// <response code="404">Project or quotation not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    public async Task<ApiResult> CreateContract(ContractRequest request)
    {
        await _contractService.CreateContractAsync(request);
        return Ok();
    }
    
    /// <summary>
    /// Get contract detail by id
    /// </summary>
    /// <param name="id">Contract ID to get</param>
    /// <remarks>
    /// Get contract detail by id, including payment batches with their estimated payment dates.
    /// Each payment batch may include:
    /// - Basic information (id, name, status, etc.)
    /// - Payment estimate date from its linked construction item (if available)
    /// - Current payment status and value
    /// 
    /// Payment batches are ordered by creation date and only active batches are included.
    /// The payment estimate date for each batch comes from its linked construction item's estimate date.
    /// </remarks>
    /// <returns>Contract details including payment batches with their estimate dates</returns>
    /// <response code="200">Returns the contract details with payment batches</response>
    /// <response code="404">Hợp đồng không tồn tại</response>
    [HttpGet("{id}")]
    public async Task<ApiResult<GetContractDetailResponse>> GetContractDetail(Guid id)
    {
        var contract = await _contractService.GetContractDetailAsync(id);
        return contract;
    }
    
    /// <summary>
    /// Step 1: Initiates the contract acceptance process by generating an OTP
    /// </summary>
    /// <param name="id">Contract ID to accept</param>
    /// <remarks>
    /// This is the first step in the contract acceptance workflow:
    /// 1. Generates an OTP and saves it to Firebase
    /// 2. Sends the OTP to user's email
    /// 3. OTP expires in 5 minutes if not used
    /// </remarks>
    /// <returns>Success response if OTP is generated and sent successfully</returns>
    [HttpGet("{id}/accept")]
    public async Task<ApiResult> AcceptingContract(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với customer");
        }
        var userIdParsed = Guid.Parse(userId);
        await _contractService.VerifyingContract(id, userIdParsed);
        return Ok();
    }
    
    /// <summary>
    /// Rejects and cancels a contract
    /// </summary>
    /// <param name="id">Contract ID to reject</param>
    /// <returns>Success response if contract is rejected successfully</returns>
    [HttpGet("{id}/reject")]
    public async Task<ApiResult> RejectContract(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với customer");
        }
        await _contractService.RejectContract(id);
        return Ok();
    }
    
    /// <summary>
    /// Step 2: Verifies the contract using the OTP received in email
    /// </summary>
    /// <param name="id">Contract ID to verify</param>
    /// <param name="request">Request containing the OTP code, require 4 numbers</param>
    /// <remarks>
    /// This is the second and final step in the contract acceptance workflow:
    /// 1. Must be called after /accept endpoint
    /// 2. Validates the OTP received in email
    /// 3. If valid, marks the contract as active
    /// 4. OTP must be used within 5 minutes of generation
    /// </remarks>
    /// <returns>Success response if contract is verified successfully</returns>
    [HttpPost("{id}/verify")]
    public async Task<ApiResult> VerifyContract(Guid id, OtpRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với customer");
        }
        await _contractService.AcceptContract(id, request.OtpCode);
        return Ok();
    }
    
    
}