using System;
using System.Threading.Tasks;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;

namespace KPCOS.BusinessLayer.Services;

public interface IPaymentService
{
    /// <summary>
    /// Creates a transaction payment request to VNPAY payment gateway
    /// </summary>
    /// <param name="request">Payment request containing batch payment ID and return URL</param>
    /// <returns>Response containing the VNPAY payment URL to redirect the user to</returns>
    Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request);

    /// <summary>
    /// Processes the callback from VNPAY payment gateway after payment completion
    /// </summary>
    /// <param name="request">Callback request containing payment result and transaction details</param>
    /// <returns>Redirect URL with success or failure parameters</returns>
    Task<string> PaymentVnpayCallback(VnpayCallbackRequest request);
}
