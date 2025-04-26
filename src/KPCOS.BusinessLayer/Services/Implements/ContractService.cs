using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ContractService : IContractService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IBackgroundService _backgroundService;
    private readonly IFirebaseService _firebaseService;
    private readonly IMapper _mapper;

    public ContractService(IUnitOfWork unitOfWork, IEmailService emailService, IBackgroundService backgroundService, IFirebaseService firebaseService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _backgroundService = backgroundService;
        _firebaseService = firebaseService;
        _mapper = mapper;
    }

    /// <summary>
    /// Accept a contract, set status to Active, and set contract otp field IsActive to true.
    /// <para>Need contract id and opt code that have do by verifying the accept action from customer.</para>
    /// This function will set IsActive to true on contract otp from firestore and contract status to ACTIVE after put the opt code.
    /// </summary>
    /// <param name="contractId"></param>
    /// <param name="otpCode"></param>
    /// <exception cref="NotFoundException">Hợp đồng không tồn tại, Mã OTP không hợp lệ hoặc đã hết hạn</exception>
    public async Task AcceptContract(Guid contractId, string otpCode)
    {
        var repo = _unitOfWork.Repository<Contract>();
        var otpVerify = await _firebaseService.GetContractOtpAsync(contractId.ToString());
        if (otpVerify == null || otpVerify.OtpCode != otpCode)
        {
            throw new NotFoundException("Mã OTP không hợp lệ hoặc đã hết hạn");
        }
        
        var contract = await repo.FindAsync(contractId) ?? throw new NotFoundException("Hợp đồng không tồn tại");
        await _firebaseService.UpdateContractOtpAsync(contractId.ToString());
        contract.Status = EnumContractStatus.ACTIVE.ToString();

        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.FindAsync(contract.ProjectId) ?? throw new NotFoundException("Dự án không tồn tại");
        project.Status = EnumProjectStatus.DESIGNING.ToString();

        var quotationRepo = _unitOfWork.Repository<Quotation>();
        var quotation = await quotationRepo.FindAsync(contract.QuotationId) ?? throw new NotFoundException("Báo giá không tồn tại");
        quotation.Status = EnumQuotationStatus.CONFIRMED.ToString();
        
        await repo.UpdateAsync(contract, false);
        await projectRepo.UpdateAsync(project, false);
        await quotationRepo.UpdateAsync(quotation, false);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Reject a contract, set status to Cancelled and remove related payment batches and construction items
    /// <para>Can not reject a contract if the otp exited in firestore have IsActive = true or contract status is ACTIVE</para>
    /// </summary>
    /// <param name="contractId">guid</param>
    /// <exception cref="NotFoundException">Hợp đồng không tồn tại</exception>
    /// <exception cref="BadRequestException">Hợp đồng đã được xác nhận, không thể hủy</exception>
    public async Task RejectContract(Guid contractId)
    {
        var repo = _unitOfWork.Repository<Contract>();
        var contract = await repo.FindAsync(contractId) ?? throw new NotFoundException("Hợp đồng không tồn tại");
        
        // Check if contract is already active - can't reject an active contract
        if (contract.Status == EnumContractStatus.ACTIVE.ToString())
        {
            throw new BadRequestException("Hợp đồng đã được kích hoạt, không thể hủy");
        }
        
        // Check if contract is already cancelled
        if (contract.Status == EnumContractStatus.CANCELLED.ToString())
        {
            throw new BadRequestException("Hợp đồng đã bị hủy trước đó");
        }
        
        // Check if contract OTP is active in Firebase
        var isContractOtpInFirestore = await _firebaseService.IsContractOtpInFirestore(contractId.ToString());
        if (isContractOtpInFirestore)
        {
            var otpVerify = await _firebaseService.GetContractOtpAsync(contractId.ToString());
            if (otpVerify.IsActive)
            {
                throw new BadRequestException("Hợp đồng đã được xác nhận, không thể hủy");
            }
        }
        
        // Get related payment batches and delete them
        var paymentBatchRepo = _unitOfWork.Repository<PaymentBatch>();
        var paymentBatches = paymentBatchRepo.Get(
            filter: pb => pb.ContractId == contractId,
            includeProperties: "")
            .ToList();
            
        paymentBatchRepo.RemoveRange(paymentBatches);
        
        // Get all construction items for the project and delete them
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var constructionItems = constructionItemRepo.Get(
            filter: item => item.ProjectId == contract.ProjectId,
            includeProperties: "")
            .ToList();
        
        constructionItemRepo.RemoveRange(constructionItems);
        
        // Set contract status to CANCELLED
        contract.Status = EnumContractStatus.CANCELLED.ToString();
        await repo.UpdateAsync(contract, false);
        
        // Save all changes
        await _unitOfWork.SaveChangesAsync();
    }
    
    //cài lên firebase
    //gửi mail
    //delay job 5 phút, nếu không xác nhận thì hủy(xóa otp trên firestore)
    public async Task VerifyingContract(Guid contractId, Guid userId)
    {
        var contractRepo = _unitOfWork.Repository<Contract>();
        var contract = await contractRepo.FindAsync(contractId) ?? throw new NotFoundException("Hợp đồng không tồn tại");
        
        // Check if any other contract from the same project already has ACTIVE status
        var existingActiveContract = contractRepo.Get(
            filter: c => c.ProjectId == contract.ProjectId && 
                         c.Status == EnumContractStatus.ACTIVE.ToString() &&
                         c.Id != contractId,
            includeProperties: "")
            .FirstOrDefault();
            
        if (existingActiveContract != null)
        {
            throw new BadRequestException("Dự án này đã có hợp đồng đang hoạt động. Chỉ được phép có một hợp đồng hoạt động cho mỗi dự án.");
        }
        
        var user = await _unitOfWork.Repository<User>().FindAsync(userId);
        int otpCode = new Random().Next(1000, 9999);
        
        var OtpVerify = new OtpResponse
        {
            ContractId = contractId.ToString(),
            OtpCode = otpCode.ToString()
        };
        await _firebaseService.SaveContractOtpAsync(OtpVerify);
        await _emailService.SendVerifyEmailContractOtpAsync(user.Email, otpCode, OtpVerify.ExpiresAt);
        _backgroundService.DelayedCancelOtpJob(5, contractId.ToString());
    }

    public async Task CreateContractAsync(ContractRequest request)
    {
        if (!(await IsProjectExitAsync(request.ProjectId)))
        {
            throw new NotFoundException("Dự án không tồn tại");
        }
        if (!(await IsQuotationExitAsync(request.QuotationId)))
        {
            throw new NotFoundException("Báo giá không tồn tại");
        }
        var isConstructionExited = _unitOfWork.Repository<ConstructionItem>()
            .Get(filter: item => item.ProjectId == request.ProjectId && item.IsActive == true)
            .Any();
        if(!isConstructionExited)
        {
            throw new BadRequestException("Dự án này chưa có hạng mục xây dựng. Vui lòng tạo hạng mục xây dựng trước khi tạo hợp đồng.");
        }

        var quotation = await _unitOfWork.Repository<Quotation>().FindAsync(request.QuotationId);
        if(quotation.Status != EnumQuotationStatus.APPROVED.ToString())
        {
            throw new BadRequestException("Báo giá chưa được duyệt");
        }
        
        // Check if any other contract for this project has ACTIVE or PROCESSING status
        var contractRepo = _unitOfWork.Repository<Contract>();
        var existingActiveContract = contractRepo.Get(
            filter: c => c.ProjectId == request.ProjectId && 
                        (c.Status == EnumContractStatus.ACTIVE.ToString() || 
                         c.Status == EnumContractStatus.PROCESSING.ToString()),
            includeProperties: "")
            .FirstOrDefault();
            
        if (existingActiveContract != null)
        {
            var statusName = existingActiveContract.Status == EnumContractStatus.ACTIVE.ToString() ? "hoạt động" : "đang xử lý";
            throw new BadRequestException($"Dự án này đã có hợp đồng {statusName}. Chỉ được phép có một hợp đồng hoạt động hoặc đang xử lý cho mỗi dự án.");
        }
        
        var repo = _unitOfWork.Repository<Contract>();
        var contract = _mapper.Map<Contract>(request);
        
        // Calculate contract value from quotation price
        decimal contractValue = quotation.TotalPrice;
        string discountNote = "";
        
        // Apply promotion discount if available
        if (quotation.PromotionId.HasValue)
        {
            var promotion = await _unitOfWork.Repository<Promotion>().FindAsync(quotation.PromotionId.Value);
            if (promotion != null && promotion.IsActive == true)
            {
                // Apply the discount percentage to the contract value
                decimal discountAmount = contractValue * (promotion.Discount / 100m);
                contractValue -= discountAmount;
                
                // Add promotion info to discount note
                discountNote = $"Áp dụng khuyến mãi: {promotion.Name}, giảm {promotion.Discount}% tổng giá trị hợp đồng.";
            }
        }
        
        // Use provided contract value if specified, otherwise use calculated value
        contract.ContractValue = request.ContractValue ?? (int)Math.Round(contractValue);
        
        // Add promotion info to note if no note was provided
        if (string.IsNullOrEmpty(request.Note) && !string.IsNullOrEmpty(discountNote))
        {
            contract.Note = discountNote;
        }
        else if (!string.IsNullOrEmpty(discountNote))
        {
            // Append promotion info to existing note
            contract.Note = string.IsNullOrEmpty(request.Note) ? discountNote : $"{request.Note}\n{discountNote}";
        }
        
        contract.Id = Guid.NewGuid();
        // Set initial status to PROCESSING
        contract.Status = EnumContractStatus.PROCESSING.ToString();
        
        // Get level 1 construction items with IsPayment=true for the project, ordered by EstimateAt
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var paymentItems = constructionItemRepo.Get(
            filter: item => item.ProjectId == request.ProjectId && 
                           item.IsPayment == true && 
                           item.ParentId == null && 
                           item.IsActive == true,
            orderBy: query => query.OrderBy(item => item.EstimateAt),
            includeProperties: "",
            pageSize:3)
            .ToList();
        
        // Calculate the value for each payment batch (25% of contract value for each)
        decimal batchValue = contract.ContractValue * 0.25m;
        
        // Define the payment phases for all batches
        EnumPaymentStatus[] phases = new EnumPaymentStatus[]
        {
            EnumPaymentStatus.DEPOSIT,
            EnumPaymentStatus.PRE_CONSTRUCTING,
            EnumPaymentStatus.CONSTRUCTING,
            EnumPaymentStatus.ACCEPTANCE
        };
        
        // Create all payment batches in a single loop
        for (int i = 0; i < 4; i++)
        {
            var paymentBatch = new PaymentBatch
            {
                Id = Guid.NewGuid(),
                Name = i == 0 ? "Thanh toán đặt cọc" : $"Đợt thanh toán {i}",
                TotalValue = batchValue,
                IsPaid = false,
                ContractId = contract.Id,
                // Only assign construction items to non-deposit batches (i > 0)
                ConstructionItemId = (i > 0 && (i - 1) < paymentItems.Count) ? paymentItems[i - 1].Id : null,
                Status = phases[i].ToString(),
                Percents = 25,
                IsActive = true
            };
            
            // Add to contract's PaymentBatches collection
            contract.PaymentBatches.Add(paymentBatch);
        }
        
        // Add contract to repository and save changes
        await repo.AddAsync(contract);
    }

    /// <summary>
    /// Gets detailed information about a contract including its payment batches and their estimated payment dates
    /// </summary>
    /// <param name="id">The contract ID to retrieve</param>
    /// <returns>Contract details with payment batches</returns>
    /// <exception cref="NotFoundException">Thrown when contract is not found - "Hợp đồng không tồn tại"</exception>
    /// <remarks>
    /// This method:
    /// 1. Retrieves the contract with its payment batches
    /// 2. Gets all payment-related construction items for the project
    /// 3. Maps payment batches to response DTOs, including:
    ///    - Basic payment batch information
    ///    - Estimated payment dates from linked construction items
    /// 4. Orders payment batches by creation date
    /// </remarks>
    public async Task<GetContractDetailResponse> GetContractDetailAsync(Guid id)
    {
        var repo = _unitOfWork.Repository<Contract>();
        var contract = repo.Get(
            filter: contract => contract.Id == id,
            includeProperties: "PaymentBatches,Project")
            .SingleOrDefault() ?? throw new NotFoundException("Hợp đồng không tồn tại");

        var response = _mapper.Map<GetContractDetailResponse>(contract);
        
        // Get all payment construction items for the project
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var paymentItems = constructionItemRepo.Get(
            filter: item => item.ProjectId == contract.ProjectId && 
                           item.IsPayment == true && 
                           item.ParentId == null && 
                           item.IsActive == true,
            orderBy: query => query.OrderBy(item => item.EstimateAt))
            .ToList();

        // Ensure payment batches are included in response
        if (contract.PaymentBatches != null && contract.PaymentBatches.Any())
        {
            var paymentBatchResponses = new List<GetAllPaymentBatchesResponse>();
            var activeBatches = contract.PaymentBatches
                .Where(pb => pb.IsActive == true)
                .OrderBy(pb => pb.PaymentAt ?? DateTime.MaxValue); // Sort by PaymentAt, with null values at the end

            foreach (var batch in activeBatches)
            {
                var batchResponse = _mapper.Map<GetAllPaymentBatchesResponse>(batch);
                
                // If batch has a linked construction item, get its estimate date
                if (batch.ConstructionItemId.HasValue)
                {
                    var constructionItem = paymentItems.FirstOrDefault(ci => ci.Id == batch.ConstructionItemId.Value);
                    if (constructionItem != null)
                    {
                        batchResponse.PaymentEstimateAt = constructionItem.EstimateAt.ToDateTime(TimeOnly.MinValue);
                    }
                }
                
                paymentBatchResponses.Add(batchResponse);
            }

            response.PaymentBatches = paymentBatchResponses;
        }

        return response;
    }

    private async Task<bool> IsProjectExitAsync(Guid projectId)
    {
        var project = await _unitOfWork.Repository<Project>().SingleOrDefaultAsync(project => project.Id == projectId);
        if (project == null)
        {
            return false;
        }
        return true;
    }
    private async Task<bool> IsQuotationExitAsync(Guid quotationId)
    {
        var quotation = await _unitOfWork.Repository<Quotation>().SingleOrDefaultAsync(quotation => quotation.Id == quotationId);
        if (quotation == null)
        {
            return false;
        }
        return true;
    }
    

    public async Task InvalidContractOtpAsync(string contractId)
    {
        await _firebaseService.DeleteContractOtpAsync(contractId);
    }
}