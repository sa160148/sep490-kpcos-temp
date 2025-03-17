using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.Helpers;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Constants;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace KPCOS.BusinessLayer.Services.Implements;

public class PaymentService : IPaymentService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VnpaySetting _vnpaySetting;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public PaymentService(
        IHttpContextAccessor httpContextAccessor, 
        VnpaySetting vnpaySetting, 
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _vnpaySetting = vnpaySetting;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    /// <summary>
    /// Creates a transaction payment request to VNPAY payment gateway
    /// </summary>
    /// <param name="request">Payment request containing batch payment ID and return URL</param>
    /// <returns>Response containing the VNPAY payment URL to redirect the user to</returns>
    public async Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request)
    {
        // Use SEA time instead of local time to ensure consistency
        var time = GlobalUtility.GetCurrentSEATime();
        
        // Validate that BatchPaymentId is provided
        if (!request.BatchPaymentId.HasValue)
        {
            throw new Exception("BatchPaymentId must be provided");
        }
        
        // Payment for a batch payment
        var paymentBatch = await GetPaymentBatchAsync(request.BatchPaymentId.Value);
        var paymentInfo = GeneratePaymentInfo(paymentBatch);
        var amount = paymentBatch.TotalValue;
        var customerId = paymentBatch.Contract.Project.CustomerId;
        var returnUrl = GenerateReturnUrl(request.ReturnUrl, customerId, request.BatchPaymentId.Value);

        var vnpayLibrary = new VnpayLibrary();
        vnpayLibrary.AddRequestData("vnp_Version", _vnpaySetting.Version);
        vnpayLibrary.AddRequestData("vnp_Command", _vnpaySetting.Command);
        vnpayLibrary.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode);
        vnpayLibrary.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
        vnpayLibrary.AddRequestData("vnp_CurrCode", _vnpaySetting.CurrCode);
        vnpayLibrary.AddRequestData("vnp_Locale", _vnpaySetting.Locale);
        vnpayLibrary.AddRequestData("vnp_CreateDate", time.ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_IpAddr", GlobalUtility.GetIpAddress());
        vnpayLibrary.AddRequestData("vnp_OrderInfo", paymentInfo);
        vnpayLibrary.AddRequestData("vnp_OrderType", EnumBillType.HOA_DON_THANH_TOAN + "");
        // Use SEA time for expire date to ensure consistency with the create date
        vnpayLibrary.AddRequestData("vnp_ExpireDate", time.AddSeconds(300).ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString());
        
        // Generate the VnpayUrl
        vnpayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
        var vnpayUrl = vnpayLibrary
            .CreateRequestUrl(_vnpaySetting.PaymentEndpoint, _vnpaySetting.HashSecret);
        
        return new CreateTransactionPaymentResponse(vnpayUrl);
    }

    private string GenerateReturnUrl(string returnUrl, Guid customerId, Guid batchPaymentId)
    {
        var serverUrl = GlobalUtility.GetSecureServerUrl(_httpContextAccessor);
        var callbackUrl = _vnpaySetting.CallbackUrl;
        return  $"{serverUrl}/{callbackUrl}?returnUrl={returnUrl}&customerId={customerId}&batchPaymentId={batchPaymentId}";
    }

    private async Task<PaymentBatch> GetPaymentBatchAsync(Guid batchPaymentId)  
    {
        var paymentBatch = _unitOfWork.Repository<PaymentBatch>()
            .Get(
                filter: x => x.Id == batchPaymentId,
                includeProperties: "Contract.Project.Customer"
            )
            .FirstOrDefault();
            
        if (paymentBatch == null)
        {
            throw new Exception("Payment batch not found");
        }
        return paymentBatch;
    }

    private string GeneratePaymentInfo(PaymentBatch paymentBatch)
    {
        // Try to parse the string to enum
        if (Enum.TryParse<EnumPaymentStatus>(paymentBatch.Status, out var paymentStatusEnum))
        {
            string paymentInfo = paymentStatusEnum switch 
            {
                EnumPaymentStatus.DEPOSIT => "Thanh toan coc ",
                EnumPaymentStatus.PRE_CONSTRUCTING => "Thanh toan 1 ",
                EnumPaymentStatus.CONSTRUCTING => "Thanh toan 2 ",
                EnumPaymentStatus.ACCEPTANCE => "Thanh toan 3 ",
                _ => "Thanh toan "
            };
            
            return paymentInfo + paymentBatch.Id.ToString();
        }
        
        // Fallback if parsing fails
        return "Thanh toan " + paymentBatch.Id.ToString();
    }

    /// <summary>
    /// Processes the callback from VNPAY payment gateway after payment completion
    /// </summary>
    /// <param name="request">Callback request containing payment result and transaction details</param>
    /// <returns>Redirect URL with success or failure parameters</returns>
    public async Task<string> PaymentVnpayCallback(VnpayCallbackRequest request)
    {
        string returnUrl = request.returnUrl;
        
        // Check payment success status first before doing any database operations
        if (!request.IsSuccess)
        {
            // Payment failed, just return failure URL
            return $"{returnUrl}?success=failed&code={request.vnp_ResponseCode}";
        }
        
        var customerId = Guid.Parse(request.customerId);
        
        // Validate that batchPaymentId is provided
        if (string.IsNullOrEmpty(request.batchPaymentId))
        {
            return $"{returnUrl}?success=failed&message=BatchPaymentIdRequired";
        }
        
        var batchPaymentId = Guid.Parse(request.batchPaymentId);
        var time = GlobalUtility.GetCurrentSEATime();
        
        // Get payment batch with all necessary related entities
        var paymentBatch = _unitOfWork.Repository<PaymentBatch>()
            .Get(
                filter: x => x.Id == batchPaymentId,
                includeProperties: "Contract.Project.Customer.User,Contract,Contract.Project"
            )
            .FirstOrDefault();
            
        if (paymentBatch == null)
        {
            // If payment batch not found, return failure URL
            return $"{returnUrl}?success=failed&message=PaymentBatchNotFound";
        }
        
        // Update payment batch status - keep the existing status (payment phase) and just mark as paid
        paymentBatch.IsPaid = true;
        paymentBatch.PaymentAt = time;
        
        // Create transaction record
        var transaction = new Transaction
        {
            Id = Guid.Parse(request.vnp_TxnRef!),
            IsActive = true,
            Amount = (int)((request.vnp_Amount ?? 0) / 100),
            CustomerId = customerId,
            No = batchPaymentId, // Link transaction to the payment batch
            Note = request.vnp_OrderInfo,
            Status = EnumTransactionStatus.SUCCESSFUL.ToString(),
            Type = EnumBillType.HOA_DON_THANH_TOAN.ToString()
        };
        
        // Add new transaction entity to context using repository pattern
        await _unitOfWork.Repository<Transaction>().AddAsync(transaction, false);
        
        // Update the payment batch entity using repository pattern
        await _unitOfWork.Repository<PaymentBatch>().UpdateAsync(paymentBatch, false);
        
        // Save all changes in a single call
        await _unitOfWork.SaveChangesAsync();
        
        // Return success URL
        return $"{returnUrl}?success=true&transactionId={transaction.Id}";
    }

    /// <summary>
    /// Gets the payment transaction details by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Payment transaction details with related payment batch, contract and project information</returns>
    public async Task<GetTransactionDetailResponse> GetPaymentDetailAsync(Guid id)
    {
        // Get transaction with related customer
        var transaction = _unitOfWork.Repository<Transaction>()
            .Get(
                filter: x => x.Id == id,
                includeProperties: "Customer.User"
            )
            .FirstOrDefault();

        if (transaction == null)
        {
            throw new NotFoundException("Không tìm thấy giao dịch thanh toán với ID: " + id);
        }

        // Map transaction to response DTO using AutoMapper
        var response = _mapper.Map<GetTransactionDetailResponse>(transaction);

        // The No field in Transaction can reference either a PaymentBatch or a Doc
        // First, try to find a PaymentBatch with this ID
        var paymentBatch = _unitOfWork.Repository<PaymentBatch>()
            .Get(
                filter: x => x.Id == transaction.No,
                includeProperties: "Contract.Project"
            )
            .FirstOrDefault();

        if (paymentBatch != null)
        {
            // This transaction is related to a payment batch
            // Map payment batch to GetPaymentForTransactionResponse
            var paymentBatchResponse = _mapper.Map<GetPaymentForTransactionResponse>(paymentBatch);
            
            // Map contract to GetContractForPaymentBatchResponse
            var contractResponse = _mapper.Map<GetContractForPaymentBatchResponse>(paymentBatch.Contract);
            
            // Map project to GetProjectForTransactionResponse
            var projectResponse = _mapper.Map<GetProjectForTransactionResponse>(paymentBatch.Contract.Project);
            
            // Link everything together
            contractResponse.Project = projectResponse;
            paymentBatchResponse.Contract = contractResponse;
            response.PaymentBatch = paymentBatchResponse;
        }
        else
        {
            // If not a payment batch, try to find a Doc with this ID
            var doc = _unitOfWork.Repository<Doc>()
                .Get(
                    filter: x => x.Id == transaction.No,
                    includeProperties: "Project"
                )
                .FirstOrDefault();

            if (doc != null)
            {
                // This transaction is related to a document
                var docResponse = _mapper.Map<GetDocResponse>(doc);
                
                // Map project information if available
                if (doc.Project != null)
                {
                    var projectResponse = _mapper.Map<GetProjectForTransactionResponse>(doc.Project);
                    docResponse.Project = projectResponse;
                }
                
                response.Doc = docResponse;
            }
        }

        return response;
    }
}