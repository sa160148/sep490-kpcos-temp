using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;

namespace KPCOS.BusinessLayer.Services;

public interface IPaymentService
{
    /// <summary>
    /// Creates a transaction payment request to VNPAY payment gateway
    /// </summary>
    /// <param name="request">Payment request containing batch payment ID or maintenance request ID and return URL</param>
    /// <returns>Response containing the VNPAY payment URL to redirect the user to</returns>
    Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request);

    /// <summary>
    /// Processes the callback from VNPAY payment gateway after payment completion
    /// </summary>
    /// <param name="request">Callback request containing payment result and transaction details</param>
    /// <returns>Redirect URL with success or failure parameters</returns>
    Task<string> PaymentVnpayCallback(VnpayCallbackRequest request);

    /// <summary>
    /// Gets the payment transaction details by ID with all related entities
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Payment transaction details with related payment batch, contract, project, maintenance request, or document information</returns>
    Task<GetTransactionDetailResponse> GetPaymentDetailAsync(Guid id);
    
    /// <summary>
    /// Gets transactions based on filter criteria with optional filtering by customer ID and project ID
    /// </summary>
    /// <param name="request">Filter criteria for transactions</param>
    /// <param name="customerId">Optional customer ID to filter transactions by customer</param>
    /// <param name="projectId">Optional project ID to filter transactions by project</param>
    /// <returns>List of transactions with pagination information and fully populated related entities</returns>
    Task<(IEnumerable<GetTransactionDetailResponse> data, int total)> GetTransactionsAsync(
        GetAllTransactionFilterRequest request, 
        Guid? customerId = null, 
        Guid? projectId = null);
}
