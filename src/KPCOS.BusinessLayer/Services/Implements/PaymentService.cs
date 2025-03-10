using System;
using System.Collections.Generic;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.Helpers;
using KPCOS.Common.Constants;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Http;

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
    public async Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request)
    {
        var time = DateTime.Now;
        var paymentBatch = await GetPaymentBatchAsync(request.BatchPaymentId);
        var paymentInfo = GeneratePaymentInfo(paymentBatch);

        var paymentMethod = 
        paymentBatch.PaymentPhase == EnumPaymentPhase.DEPOSIT.ToString() ?
         PaymentMethod.DEPOSIT.ToString() : PaymentMethod.PAYMENT.ToString();

        var vnpayLibrary = new VnpayLibrary();
        vnpayLibrary.AddRequestData("vnp_Version", _vnpaySetting.Version);
        vnpayLibrary.AddRequestData("vnp_Command", _vnpaySetting.Command);
        vnpayLibrary.AddRequestData("vnp_TmnCode", _vnpaySetting.TmnCode);
        vnpayLibrary.AddRequestData("vnp_Amount", ((long)paymentBatch.TotalValue).ToString());
        vnpayLibrary.AddRequestData("vnp_CurrCode", _vnpaySetting.CurrCode);
        vnpayLibrary.AddRequestData("vnp_Locale", _vnpaySetting.Locale);
        vnpayLibrary.AddRequestData("vnp_CreateDate", time.ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_IpAddr", GlobalUtility.GetIpAddress());
        vnpayLibrary.AddRequestData("vnp_OrderInfo", paymentInfo);
        vnpayLibrary.AddRequestData("vnp_OrderType", paymentMethod);
        vnpayLibrary.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString());
        
        // Generate the VnpayUrl
        var customerId = paymentBatch.Contract.Project.CustomerId;
        var returnUrl = GenerateReturnUrl(request.ReturnUrl, customerId);
        vnpayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
        var vnpayUrl = vnpayLibrary.CreateRequestUrl(_vnpaySetting.PaymentEndpoint, _vnpaySetting.HashSecret);
        
        return new CreateTransactionPaymentResponse(vnpayUrl);
    }

    private string GenerateReturnUrl(string returnUrl, Guid customerId)
    {
        var serverUrl = GlobalUtility.GetSecureServerUrl(_httpContextAccessor);
        var callbackUrl = _vnpaySetting.CallbackUrl;
        return  $"{serverUrl}/{callbackUrl}?returnUrl={returnUrl}";
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
}