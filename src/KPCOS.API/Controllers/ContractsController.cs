using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;
using ContractRequest = KPCOS.BusinessLayer.DTOs.Request.Contracts.ContractRequest;

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

    [HttpPost("")]
    public async Task<ApiResult> CreateContract(ContractRequest request)
    {
        await _contractService.CreateContractAsync(request);
        return Ok();
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