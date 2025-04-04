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
using LinqKit;

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
    /// <param name="request">Payment request containing batch payment ID or maintenance request ID and return URL</param>
    /// <returns>Response containing the VNPAY payment URL to redirect the user to</returns>
    public async Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request)
    {
        // Use SEA time instead of local time to ensure consistency
        var time = GlobalUtility.GetCurrentSEATime();
        decimal amount = 0;
        string paymentInfo = "";
        Guid customerId = Guid.Empty;
        string orderType = EnumTransactionType.PAYMENT_BATCH.ToString();
        
        // Check if we have a batch payment ID or maintenance request ID
        if (request.BatchPaymentId.HasValue)
        {
            // Payment for a batch payment
            var paymentBatch = await GetPaymentBatchAsync(request.BatchPaymentId.Value);
            paymentInfo = GeneratePaymentInfo(paymentBatch);
            amount = paymentBatch.TotalValue;
            customerId = paymentBatch.Contract.Project.CustomerId;
        }
        else if (request.MaintenanceRequestId.HasValue)
        {
            // Payment for a maintenance request
            var maintenanceRequest = await GetMaintenanceRequestAsync(request.MaintenanceRequestId.Value);
            paymentInfo = $"Thanh toan dich vu bao tri {maintenanceRequest.Id}";
            amount = maintenanceRequest.TotalValue;
            customerId = maintenanceRequest.CustomerId;
            orderType = EnumTransactionType.MAINTENANCE_REQUEST.ToString();
        }
        else
        {
            throw new Exception("Either BatchPaymentId or MaintenanceRequestId must be provided");
        }
        
        // Generate return URL with appropriate parameters
        var returnUrl = GenerateReturnUrl(
            request.ReturnUrl, 
            customerId, 
            request.BatchPaymentId, 
            request.MaintenanceRequestId
        );

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
        vnpayLibrary.AddRequestData("vnp_OrderType", orderType);
        // Use SEA time for expire date to ensure consistency with the create date
        vnpayLibrary.AddRequestData("vnp_ExpireDate", time.AddSeconds(300).ToString("yyyyMMddHHmmss"));
        vnpayLibrary.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString());
        
        // Generate the VnpayUrl
        vnpayLibrary.AddRequestData("vnp_ReturnUrl", returnUrl);
        var vnpayUrl = vnpayLibrary
            .CreateRequestUrl(_vnpaySetting.PaymentEndpoint, _vnpaySetting.HashSecret);
        
        return new CreateTransactionPaymentResponse(vnpayUrl);
    }

    private string GenerateReturnUrl(
        string returnUrl, 
        Guid customerId, 
        Guid? batchPaymentId = null, 
        Guid? maintenanceRequestId = null)
    {
        var serverUrl = GlobalUtility.GetSecureServerUrl(_httpContextAccessor);
        var callbackUrl = _vnpaySetting.CallbackUrl;
        string url = $"{serverUrl}/{callbackUrl}?returnUrl={returnUrl}&customerId={customerId}";
        
        if (batchPaymentId.HasValue)
        {
            url += $"&batchPaymentId={batchPaymentId.Value}";
        }
        
        if (maintenanceRequestId.HasValue)
        {
            url += $"&maintenanceRequestId={maintenanceRequestId.Value}";
        }
        
        return url;
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
    
    private async Task<MaintenanceRequest> GetMaintenanceRequestAsync(Guid maintenanceRequestId)
    {
        var maintenanceRequest = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: x => x.Id == maintenanceRequestId,
                includeProperties: "Customer"
            )
            .FirstOrDefault();
            
        if (maintenanceRequest == null)
        {
            throw new Exception("Maintenance request not found");
        }
        return maintenanceRequest;
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
        var time = GlobalUtility.GetCurrentSEATime();
        
        // Check which type of payment we're handling
        if (!string.IsNullOrEmpty(request.batchPaymentId))
        {
            return await ProcessBatchPaymentCallback(request, customerId, time, returnUrl);
        }
        else if (!string.IsNullOrEmpty(request.maintenanceRequestId))
        {
            return await ProcessMaintenanceRequestCallback(request, customerId, time, returnUrl);
        }
        else
        {
            return $"{returnUrl}?success=failed&message=NoPaymentTypeSpecified";
        }
    }
    
    private async Task<string> ProcessBatchPaymentCallback(
        VnpayCallbackRequest request, 
        Guid customerId, 
        DateTime time, 
        string returnUrl)
    {
        var batchPaymentId = Guid.Parse(request.batchPaymentId!);
        
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
            Type = EnumTransactionType.PAYMENT_BATCH.ToString()
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
    
    private async Task<string> ProcessMaintenanceRequestCallback(
        VnpayCallbackRequest request, 
        Guid customerId, 
        DateTime time, 
        string returnUrl)
    {
        var maintenanceRequestId = Guid.Parse(request.maintenanceRequestId!);
        
        // Get maintenance request with all necessary related entities
        var maintenanceRequest = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: x => x.Id == maintenanceRequestId,
                includeProperties: "Customer.User,MaintenancePackage"
            )
            .FirstOrDefault();
            
        if (maintenanceRequest == null)
        {
            // If maintenance request not found, return failure URL
            return $"{returnUrl}?success=failed&message=MaintenanceRequestNotFound";
        }
        
        // Update maintenance request status to paid
        maintenanceRequest.IsPaid = true;
        maintenanceRequest.Status = EnumMaintenanceRequestStatus.REQUESTING.ToString();
        
        // Create transaction record
        var transaction = new Transaction
        {
            Id = Guid.Parse(request.vnp_TxnRef!),
            IsActive = true,
            Amount = (int)((request.vnp_Amount ?? 0) / 100),
            CustomerId = customerId,
            No = maintenanceRequestId, // Link transaction to the maintenance request
            Note = request.vnp_OrderInfo,
            Status = EnumTransactionStatus.SUCCESSFUL.ToString(),
            Type = EnumTransactionType.MAINTENANCE_REQUEST.ToString()
        };
        
        // Add new transaction entity to context using repository pattern
        await _unitOfWork.Repository<Transaction>().AddAsync(transaction, false);
        
        // Update the maintenance request entity using repository pattern
        await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
        
        // Save all changes in a single call
        await _unitOfWork.SaveChangesAsync();
        
        // Return success URL
        return $"{returnUrl}?success=true&transactionId={transaction.Id}";
    }

    /// <summary>
    /// Gets the payment transaction details by ID with all related entities
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Payment transaction details with related payment batch, contract, project, maintenance request, or document information</returns>
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

        // The No field in Transaction can reference either a PaymentBatch, a MaintenanceRequest, or a Doc
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
            response.PaymentBatch = _mapper.Map<GetPaymentForTransactionResponse>(paymentBatch);
            
            if (paymentBatch.Contract != null)
            {
                var contractResponse = _mapper.Map<GetContractForPaymentBatchResponse>(paymentBatch.Contract);
                
                if (paymentBatch.Contract.Project != null)
                {
                    contractResponse.Project = _mapper.Map<GetProjectForTransactionResponse>(paymentBatch.Contract.Project);
                }
                
                response.PaymentBatch.Contract = contractResponse;
            }
            
            return response;
        }

        // If not a payment batch, check if it's a maintenance request
        var maintenanceRequest = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: x => x.Id == transaction.No,
                includeProperties: "MaintenancePackage"
            )
            .FirstOrDefault();
            
        if (maintenanceRequest != null)
        {
            // This transaction is related to a maintenance request
            response.MaintenanceRequest = _mapper.Map<GetMaintenanceRequestForTransactionResponse>(maintenanceRequest);
            return response;
        }

        // If not a maintenance request, check if it's a doc
        var doc = _unitOfWork.Repository<Doc>()
            .Get(
                filter: x => x.Id == transaction.No,
                includeProperties: "Project"
            )
            .FirstOrDefault();

        if (doc != null)
        {
            // This transaction is related to a document
            response.Doc = _mapper.Map<GetDocResponse>(doc);
            
            if (doc.Project != null)
            {
                response.Doc.Project = _mapper.Map<GetProjectForTransactionResponse>(doc.Project);
            }
        }

        return response;
    }

    /// <summary>
    /// Gets transactions based on filter criteria with optional filtering by customer ID and project ID
    /// </summary>
    /// <param name="request">Filter criteria for transactions</param>
    /// <param name="customerId">Optional customer ID to filter transactions by customer</param>
    /// <param name="projectId">Optional project ID to filter transactions by project</param>
    /// <returns>List of transactions with pagination information and fully populated related entities</returns>
    public async Task<(IEnumerable<GetTransactionDetailResponse> data, int total)> GetTransactionsAsync(
        GetAllTransactionFilterRequest request,
        Guid? customerId = null,
        Guid? projectId = null)
    {
        var repository = _unitOfWork.Repository<Transaction>();
        var expression = request.GetExpressions();
        
        // Add customer ID filter if provided
        if (customerId.HasValue)
        {
            var customerId1 = await _unitOfWork.Repository<Customer>().SingleOrDefaultAsync(x => x.UserId == customerId.Value);
            expression = expression.And(t => t.CustomerId == customerId1.Id);
        }
        
        // Get all transactions based on filters
        var (transactions, total) = repository.GetWithCount(
            filter: expression,
            includeProperties: "Customer.User",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );
        
        // Early return if no transactions found
        if (!transactions.Any())
            return (Enumerable.Empty<GetTransactionDetailResponse>(), 0);
        
        // Map transactions to response DTOs
        var responses = _mapper.Map<IEnumerable<GetTransactionDetailResponse>>(transactions).ToList();
        
        // Get all transaction IDs to use in subsequent queries for related entities
        var transactionIds = transactions.Select(t => t.No).ToList();
        
        // Check if we need to filter by related entity type
        bool shouldFilterByRelated = !string.IsNullOrEmpty(request.Related);
        string relatedType = shouldFilterByRelated ? request.Related.ToLower() : string.Empty;
        
        // Always process related data - but apply filters if Related is specified
        // This ensures all transactions have their related data loaded
        
        // Process payment batch related transactions
        if (!shouldFilterByRelated || relatedType == "batch")
        {
            ProcessBatchRelatedTransactions(transactions, responses, transactionIds, projectId, ref total);
        }
        
        // Process maintenance request related transactions
        if (!shouldFilterByRelated || relatedType == "maintenance")
        {
            ProcessMaintenanceRelatedTransactions(transactions, responses, transactionIds);
        }
        
        // Process doc related transactions
        if (!shouldFilterByRelated || relatedType == "doc")
        {
            ProcessDocRelatedTransactions(transactions, responses, transactionIds, projectId, ref total);
        }
        
        // If filtering by related type is requested, remove responses without the specified related entity
        if (shouldFilterByRelated)
        {
            switch (relatedType)
            {
                case "batch":
                    responses.RemoveAll(r => r.PaymentBatch == null);
                    break;
                case "maintenance":
                    responses.RemoveAll(r => r.MaintenanceRequest == null);
                    break;
                case "doc":
                    responses.RemoveAll(r => r.Doc == null);
                    break;
            }
            
            // Update total count after filtering
            total = responses.Count;
        }
        
        return (responses, total);
    }
    
    private void ProcessBatchRelatedTransactions(
        IEnumerable<Transaction> transactions,
        List<GetTransactionDetailResponse> responses,
        List<Guid> transactionIds,
        Guid? projectId,
        ref int total)
    {
        // Get all payment batches related to these transactions with their related entities
        var paymentBatches = _unitOfWork.Repository<PaymentBatch>()
            .Get(
                filter: pb => transactionIds.Contains(pb.Id),
                includeProperties: "Contract.Project"
            )
            .ToList();
        
        // Additional filter by project ID if provided
        if (projectId.HasValue)
        {
            var projectIdValue = projectId.Value; // Capture variable for lambda expression
            
            // Filter batches by project
            paymentBatches = paymentBatches
                .Where(pb => pb.Contract?.ProjectId == projectIdValue)
                .ToList();
            
            // Get transaction IDs that match filtered batches
            var filteredBatchIds = paymentBatches.Select(pb => pb.Id).ToList();
            var filteredTransactionIds = transactions
                .Where(t => filteredBatchIds.Contains(t.No))
                .Select(t => t.Id)
                .ToList();
            
            // Filter responses to only include matching transactions
            responses.RemoveAll(r => r.Id.HasValue && !filteredTransactionIds.Contains(r.Id.Value));
            
            // Update total count
            total = responses.Count;
        }
        
        // Create lookup by ID for faster access
        var batchLookup = paymentBatches.ToDictionary(b => b.Id);
        var transactionsById = transactions.ToDictionary(t => t.Id);
        
        // Match transactions with their payment batches and populate response
        foreach (var response in responses.Where(r => r.Id.HasValue))
        {
            if (transactionsById.TryGetValue(response.Id.Value, out var transaction) && 
                batchLookup.TryGetValue(transaction.No, out var matchingBatch))
            {
                // Map payment batch to response
                response.PaymentBatch = _mapper.Map<GetPaymentForTransactionResponse>(matchingBatch);
                
                // Map contract if available
                if (matchingBatch.Contract != null)
                {
                    var contractResponse = _mapper.Map<GetContractForPaymentBatchResponse>(matchingBatch.Contract);
                    
                    // Map project if available
                    if (matchingBatch.Contract.Project != null)
                    {
                        contractResponse.Project = _mapper.Map<GetProjectForTransactionResponse>(matchingBatch.Contract.Project);
                    }
                    
                    response.PaymentBatch.Contract = contractResponse;
                }
            }
        }
    }
    
    private void ProcessMaintenanceRelatedTransactions(
        IEnumerable<Transaction> transactions,
        List<GetTransactionDetailResponse> responses,
        List<Guid> transactionIds)
    {
        // Get all maintenance requests related to these transactions
        var maintenanceRequests = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: mr => transactionIds.Contains(mr.Id),
                includeProperties: "MaintenancePackage"
            )
            .ToList();
        
        // Create lookup by ID for faster access
        var requestLookup = maintenanceRequests.ToDictionary(mr => mr.Id);
        var transactionsById = transactions.ToDictionary(t => t.Id);
        
        // Match transactions with their maintenance requests and populate response
        foreach (var response in responses.Where(r => r.Id.HasValue))
        {
            if (transactionsById.TryGetValue(response.Id.Value, out var transaction) && 
                requestLookup.TryGetValue(transaction.No, out var matchingRequest))
            {
                response.MaintenanceRequest = _mapper.Map<GetMaintenanceRequestForTransactionResponse>(matchingRequest);
            }
        }
    }
    
    private void ProcessDocRelatedTransactions(
        IEnumerable<Transaction> transactions,
        List<GetTransactionDetailResponse> responses,
        List<Guid> transactionIds,
        Guid? projectId,
        ref int total)
    {
        // Get all docs related to these transactions
        var docs = _unitOfWork.Repository<Doc>()
            .Get(
                filter: d => transactionIds.Contains(d.Id),
                includeProperties: "Project"
            )
            .ToList();
        
        // Additional filter by project ID if provided
        if (projectId.HasValue)
        {
            var projectIdValue = projectId.Value; // Capture variable for lambda expression
            
            // Filter docs by project
            docs = docs
                .Where(d => d.ProjectId == projectIdValue)
                .ToList();
            
            // Get transaction IDs that match filtered docs
            var filteredDocIds = docs.Select(d => d.Id).ToList();
            var filteredTransactionIds = transactions
                .Where(t => filteredDocIds.Contains(t.No))
                .Select(t => t.Id)
                .ToList();
            
            // Filter responses to only include matching transactions
            responses.RemoveAll(r => r.Id.HasValue && !filteredTransactionIds.Contains(r.Id.Value));
            
            // Update total count
            total = responses.Count;
        }
        
        // Create lookup by ID for faster access
        var docLookup = docs.ToDictionary(d => d.Id);
        var transactionsById = transactions.ToDictionary(t => t.Id);
        
        // Match transactions with their docs and populate response
        foreach (var response in responses.Where(r => r.Id.HasValue))
        {
            if (transactionsById.TryGetValue(response.Id.Value, out var transaction) && 
                docLookup.TryGetValue(transaction.No, out var matchingDoc))
            {
                // Map doc to response
                response.Doc = _mapper.Map<GetDocResponse>(matchingDoc);
                
                // Map project if available
                if (matchingDoc.Project != null)
                {
                    response.Doc.Project = _mapper.Map<GetProjectForTransactionResponse>(matchingDoc.Project);
                }
            }
        }
    }
}