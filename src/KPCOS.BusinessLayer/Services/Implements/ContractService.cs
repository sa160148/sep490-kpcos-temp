using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ContractService : IContractService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageService> _logger;
    
    public ContractService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task CreateContractAsync(ContractCreateRequest request)
    {
        IRepository<Contract> contractRepo = _unitOfWork.Repository<Contract>();
        IRepository<Quotation> quotationRepo = _unitOfWork.Repository<Quotation>();
        IRepository<Project> projectRepo = _unitOfWork.Repository<Project>();
        IRepository<ConstructionTemplate> constructionTemplateRepo = _unitOfWork.Repository<ConstructionTemplate>();
        IRepository<ConstructionTemplateItem > constructionTemplateItemRepo = _unitOfWork.Repository<ConstructionTemplateItem>();
        IRepository<ConstructionItem > constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        
        var quotationRaw = await quotationRepo.FindAsync(request.QuotationId);
        if (quotationRaw == null)
        {
            throw new NotFoundException("Báo giá không tồn tại");
        }
        
        var projectRaw = await projectRepo.FindAsync(request.ProjectId);
        
        if (projectRaw == null)
        {
            throw new NotFoundException("Dự án không tồn tại");
        }

        // var listTemplateItem = await constructionTemplateItemRepo.Get(
        //     x => x.Idtemplate == quotationRaw.Idtemplate
        // ).ToList();
        
      
        
        var contract = new Contract
        {
            Name = request.Name,
            ContractValue = request.ContractValue,
            CustomerName = request.CustomerName,
            Url = request.Url,
            Note = request.Note,
            QuotationId = request.QuotationId,
            ProjectId = request.ProjectId,
            Status = EnumContract.PROCESS.ToString()
        };
        await contractRepo.AddAsync(contract, false);
        
        await _unitOfWork.SaveChangesAsync();
    }
}