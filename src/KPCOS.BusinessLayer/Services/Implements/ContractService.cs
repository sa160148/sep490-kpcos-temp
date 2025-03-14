using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
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
        await repo.UpdateAsync(contract);
    }

    /// <summary>
    /// Reject a contract, set status to Cancelled
    /// <para>Can not reject a contract if the otp exited in firestore have IsActive = true</para>
    /// </summary>
    /// <param name="contractId">guid</param>
    /// <exception cref="NotFoundException">Hợp đồng không tồn tại</exception>
    /// <exception cref="BadRequestException">Hợp đồng đã được xác nhận, không thể hủy</exception>
    public async Task RejectContract(Guid contractId)
    {
        var repo = _unitOfWork.Repository<Contract>();
        var contract = await repo.FindAsync(contractId) ?? throw new NotFoundException("Hợp đồng không tồn tại");
        var isContractOtpInFirestore = await _firebaseService.IsContractOtpInFirestore(contractId.ToString());
        if (isContractOtpInFirestore)
        {
            var otpVerify = await _firebaseService.GetContractOtpAsync(contractId.ToString());
            if (otpVerify.IsActive)
            {
                throw new BadRequestException("Hợp đồng đã được xác nhận, không thể hủy");
            }
        }
        contract.Status = EnumContractStatus.CANCELLED.ToString();
        await repo.UpdateAsync(contract);
    }
    
    //cài lên firebase
    //gửi mail
    //delay job 5 phút, nếu không xác nhận thì hủy(xóa otp trên firestore)
    public async Task VerifyingContract(Guid contractId, Guid userId)
    {
        var contractRepo = _unitOfWork.Repository<Contract>();
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
        var quotation = await _unitOfWork.Repository<Quotation>().FindAsync(request.QuotationId);
        if(quotation.Status != EnumQuotationStatus.APPROVED.ToString())
        {
            throw new BadRequestException("Báo giá chưa được duyệt");
        }
        
        var repo = _unitOfWork.Repository<Contract>();
        var contract = _mapper.Map<Contract>(request);
        contract.ContractValue = request.ContractValue is null ? contract.ContractValue = quotation.TotalPrice : contract.ContractValue = request.ContractValue.Value;
        contract.Id = Guid.NewGuid();
        
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

    public async Task<GetContractDetailResponse> GetContractDetailAsync(Guid id)
    {
        var repo = _unitOfWork.Repository<Contract>();
        var contract = repo.Get(
            filter: contract => contract.Id == id,
            includeProperties: "PaymentBatches,Project")
            .SingleOrDefault() ?? throw new NotFoundException("Hợp đồng không tồn tại");
        var response = _mapper.Map<GetContractDetailResponse>(contract);
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