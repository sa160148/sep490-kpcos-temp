using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
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

namespace KPCOS.BusinessLayer.Services.Implements;

public class PaymentService : IPaymentService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VnpaySetting _vnpaySetting;
    private readonly IUnitOfWork _unitOfWork;
    public PaymentService(IHttpContextAccessor httpContextAccessor, VnpaySetting vnpaySetting, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _vnpaySetting = vnpaySetting;
        _unitOfWork = unitOfWork;
    }
    /// <summary>
    /// Creates a transaction payment request to VNPAY payment gateway
    /// </summary>
    /// <param name="request">Payment request containing batch payment ID and return URL</param>
    /// <returns>Response containing the VNPAY payment URL to redirect the user to</returns>
    public async Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request)
    {
        var time = DateTime.Now;
        var paymentBatch = await GetPaymentBatchAsync(request.BatchPaymentId);
        var paymentInfo = GeneratePaymentInfo(paymentBatch);

        var vnpayLibrary = new VnpayLibrary();
        vnpayLibrary.AddRequestData("vnp_Version", _vnpaySetting.Version);
        vnpayLibrary.AddRequestData("vnp_Command", _vnpaySetting.Command);
        vnpayLibrary.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode);
        vnpayLibrary.AddRequestData("vnp_Amount", ((long)paymentBatch.TotalValue * 100).ToString());
        vnpayLibrary.AddRequestData("vnp_CurrCode", _vnpaySetting.CurrCode);
        vnpayLibrary.AddRequestData("vnp_Locale", _vnpaySetting.Locale);
        vnpayLibrary.AddRequestData("vnp_CreateDate", time.ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_IpAddr", GlobalUtility.GetIpAddress());
        vnpayLibrary.AddRequestData("vnp_OrderInfo", paymentInfo);
        vnpayLibrary.AddRequestData("vnp_OrderType", EnumBillType.HOA_DON_THANH_TOAN + "");
        vnpayLibrary.AddRequestData("vnp_ExpireDate", time.AddSeconds(180).ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString());
        
        // Generate the VnpayUrl
        var customerId = paymentBatch.Contract.Project.CustomerId;
        var returnUrl = GenerateReturnUrl(request.ReturnUrl, customerId, paymentBatch.Id);
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
        .SingleOrDefault();
        if (paymentBatch == null)
        {
            throw new Exception("Payment batch not found");
        }
        return paymentBatch;
    }

    private string GeneratePaymentInfo(PaymentBatch paymentBatch)
    {
        // Try to parse the string to enum
        if (Enum.TryParse<EnumPaymentPhase>(paymentBatch.PaymentPhase, out var paymentPhaseEnum))
        {
            string paymentInfo = paymentPhaseEnum switch 
            {
                EnumPaymentPhase.DEPOSIT => "Thanh toan coc ",
                EnumPaymentPhase.PRE_CONSTRUCTING => "Thanh toan 1 ",
                EnumPaymentPhase.CONSTRUCTING => "Thanh toan 2 ",
                EnumPaymentPhase.ACCEPTANCE => "Thanh toan 3 ",
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
        var batchPaymentId = Guid.Parse(request.batchPaymentId);
        
        // Get payment batch with all necessary related entities
        var paymentBatch = _unitOfWork.Repository<PaymentBatch>()
            .Get(
                filter: x => x.Id == batchPaymentId,
                includeProperties: "Contract.Project.Customer.User,Contract,Contract.Project"
            )
            .SingleOrDefault();
            
        if (paymentBatch == null)
        {
            // If payment batch not found, return failure URL
            return $"{returnUrl}?success=failed&message=PaymentBatchNotFound";
        }
        
        // Update payment batch status
        paymentBatch.IsPaid = true;
        paymentBatch.PaymentAt = DateTime.Now;
        paymentBatch.Status = "PAID";
        
        // Generate Vietnamese document name based on payment phase enum
        string docNamePrefix;
        if (Enum.TryParse<EnumPaymentPhase>(paymentBatch.PaymentPhase, out var paymentPhaseEnum))
        {
            docNamePrefix = paymentPhaseEnum switch
            {
                EnumPaymentPhase.DEPOSIT => "Biên lai thanh toán cọc",
                EnumPaymentPhase.PRE_CONSTRUCTING => "Biên lai thanh toán đợt 1",
                EnumPaymentPhase.CONSTRUCTING => "Biên lai thanh toán đợt 2",
                EnumPaymentPhase.ACCEPTANCE => "Biên lai thanh toán đợt 3",
                _ => "Biên lai thanh toán"
            };
        }
        else
        {
            docNamePrefix = "Biên lai thanh toán";
        }
        
        // Create document record
        var docId = Guid.NewGuid();
        var doc = new Doc
        {
            Id = docId,
            IsActive = true,
            Name = $"{docNamePrefix} - {paymentBatch.Name}",
            Url = $"payment-receipts/{docId}.pdf",
            Type = EnumBillType.HOA_DON_THANH_TOAN.ToString(),
            ProjectId = paymentBatch.Contract.ProjectId
        };
        
        // Create transaction record
        var transaction = new Transaction
        {
            Id = Guid.Parse(request.vnp_TxnRef!),
            IsActive = true,
            Amount = (int)((request.vnp_Amount ?? 0) / 100),
            CustomerId = customerId,
            No = batchPaymentId,
            Note = request.vnp_OrderInfo,
            IdDocs = docId,
            Status = EnumTransactionStatus.SUCCESSFUL.ToString(),
            Type = EnumBillType.HOA_DON_THANH_TOAN.ToString()
        };
        
        // Add new entities to context
        _unitOfWork.DbContext.Set<Doc>().Add(doc);
        _unitOfWork.DbContext.Set<Transaction>().Add(transaction);
        
        // Update the payment batch entity
        _unitOfWork.DbContext.Update(paymentBatch);
        
        // Save all changes in a single call
        await _unitOfWork.SaveChangesAsync();
        
        // Return success URL
        return $"{returnUrl}?success=true&transactionId={transaction.Id}";
    }
}