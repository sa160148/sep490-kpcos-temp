using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class QuotationService : IQuotationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageItemService> _logger;
    private readonly IMapper _mapper;
    
    public QuotationService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageItemService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _mapper = mapper;
    }


    public async Task CreateQuotationAsync(QuotationCreateRequest request)
    {

       
        var repoService = _unitOfWork.Repository<Service>();
        var repoEquiment = _unitOfWork.Repository<Equipment>();
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var repoQuotationService = _unitOfWork.Repository<QuotationDetail>();
        var repoQuotationEquiment = _unitOfWork.Repository<QuotationEquipment>();
        var repoTemplateConstruction = _unitOfWork.Repository<ConstructionTemplate>();
        var repoProject = _unitOfWork.Repository<Project>();
        
        var project = await repoProject.Get().Where(p => p.Id == request.ProjectId).FirstOrDefaultAsync();
        
        if (project == null) {
            throw new BadRequestException("Dự án không tồn tại");
        }
        
        var templateConstruction = await repoTemplateConstruction.Get().Where(t => t.Id == request.TemplateConstructionId).FirstOrDefaultAsync();
        
        if (templateConstruction == null) {
            throw new BadRequestException("Mẫu công trình không tồn tại");
        }
        // find the max version of quotation
        int version = await repoQuotation.Get()
            .Where(q => q.ProjectId == request.ProjectId && q.Idtemplate == request.TemplateConstructionId)
            .Select(q => q.Version)
            .DefaultIfEmpty(0)
            .MaxAsync();
        var quotation = new Quotation
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Idtemplate = request.TemplateConstructionId,
            Version = version,
            Status = EnumQuotationStatus.OPEN.ToString(),
            TotalPrice = 0,
            PromotionId = request.PromotionId ?? null
        };
        await repoQuotation.AddAsync(quotation, false);
     

        int totalPrice = 0;
        
        foreach (var service in request.Services)
        {
            var serviceRaw = await repoService.Get().
                Where(s => s.Id == service.Id).
                FirstOrDefaultAsync();
            if (serviceRaw == null || serviceRaw.IsActive == false) {
                throw new  BadRequestException("Có dịch vụ không tồn tại");
            }
            var quotationService = new QuotationDetail
            {
                // Id = Guid.NewGuid(),
                QuotationId = quotation.Id,
                ServiceId = service.Id,
                Quantity = service.Quantity,
                Price =  serviceRaw.Price,
                Note = service.Note,
                Category = service.Category
            };
            totalPrice += serviceRaw.Price * service.Quantity;
            await repoQuotationService.AddAsync(quotationService,false);
        }
        
       

    
        
        foreach (var equipment in request.Equipments)
        {
            var equipmentRaw = await repoEquiment.Get().Where(e => e.Id == equipment.Id).FirstOrDefaultAsync();
            if (equipmentRaw == null || equipmentRaw.IsActive == false) {
                throw new  BadRequestException("Có thiết bị không tồn tại");
            }
            var quotationEquipment = new QuotationEquipment
            {
                Id = Guid.NewGuid(),
                QuotationId = quotation.Id,
                EquipmentId = equipment.Id,
                Quantity = equipment.Quantity,
                Price =  equipment.Price,
                Note = equipment.Note,
                Category = equipment.Category
            };
            totalPrice += equipment.Price * equipment.Quantity;
            await repoQuotationEquiment.AddAsync(quotationEquipment,false);
        }
        
        quotation.TotalPrice = totalPrice;

       
        
        await _unitOfWork.SaveChangesAsync();
        
        
    }

    public async Task<(IEnumerable<QuotationResponse> Data, int TotalRecords)> GetsAsyncPaging(GetAllQuotationFilterRequest filter)
    {
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        
        var pageData =  repoQuotation.GetWithCount(
            filter.GetExpressions(),
            filter.GetOrder(),
            "QuotationDetails,QuotationEquipments,QuotationDetails.Service,QuotationEquipments.Equipment,Promotion",
            filter.PageNumber,
            filter.PageSize
        );
        
        var data = _mapper.Map<IEnumerable<QuotationResponse>>(pageData.Data);
        
        return (data, pageData.Count);
    }

    public async Task<QuotationResponse> GetQuotationByIdAsync(Guid id)
    {
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        
        var quotation = repoQuotation.Get(
            filter: q => q.Id == id,
            includeProperties: "QuotationDetails,QuotationEquipments,QuotationDetails.Service,QuotationEquipments.Equipment,Promotion"
        ).FirstOrDefault() ?? throw new NotFoundException("Báo giá không tồn tại");
        
        var data = _mapper.Map<QuotationResponse>(quotation);
        
        return data;
    }

    public async Task RejectOrAcceptQuotationAsync(Guid id, QuotationRejectOrAcceptRequest request)
    {   
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var quotation = await repoQuotation.Get().Where(q => q.Id == id).FirstOrDefaultAsync();
        if (quotation == null) {
            throw new BadRequestException("Báo giá không tồn tại");
        }
        if (quotation.Status != EnumQuotationStatus.OPEN.ToString()) {
            throw new BadRequestException("Báo giá không thể thay đổi trạng thái");
        }
        if (request.IsAccept) {
            quotation.Status = EnumQuotationStatus.PREVIEW.ToString();
        } else {
            quotation.Status = EnumQuotationStatus.REJECTED.ToString();
            if (request.Reason == null || request.Reason == "") {
                throw new BadRequestException("Lý do từ chối không được để trống");
            }
            quotation.Reason = request.Reason;
        }
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task ApproveOrCancelEditQuotationAsync(Guid id, QuotationApproveOrEditRequest request)
    {
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var quotation = await repoQuotation.Get().Where(q => q.Id == id).FirstOrDefaultAsync();
        if (quotation == null) {
            throw new BadRequestException("Báo giá không tồn tại");
        }
        if (quotation.Status != EnumQuotationStatus.PREVIEW.ToString()) {
            throw new BadRequestException("Báo giá không thể thay đổi trạng thái");
        }
        if (request.IsApprove) {
            quotation.Status = EnumQuotationStatus.APPROVED.ToString();
        } else {
            quotation.Status = EnumQuotationStatus.UPDATING.ToString();
            if (request.Reason == null || request.Reason == "") {
                throw new BadRequestException("Lý do từ chối không được để trống");
            }
            quotation.Reason = request.Reason;
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        // create new quotation and update status of old quotation
        var repoQuotation = _unitOfWork.Repository<Quotation>();
     
        
        
        var quotation = await repoQuotation.Get().Where(q => q.Id == id).FirstOrDefaultAsync();
        
        if (quotation == null) {
            throw new BadRequestException("Báo giá không tồn tại");
        }
        
        if (quotation.Status != EnumQuotationStatus.UPDATING.ToString()) {
            throw new BadRequestException("Báo giá không thể chỉnh sửa");
        }
        await CreateQuotationAsync(request);
        quotation.Status = EnumQuotationStatus.PREVIEW.ToString();
        await _unitOfWork.SaveChangesAsync();
        
        
        
        
        
    }

    public async Task RewriteQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var repoQuotationService = _unitOfWork.Repository<QuotationDetail>();
        var repoQuotationEquiment = _unitOfWork.Repository<QuotationEquipment>();
        var repoService = _unitOfWork.Repository<Service>();
        var repoEquiment = _unitOfWork.Repository<Equipment>();
        var repoProject = _unitOfWork.Repository<Project>();
        var repoTemplateConstruction = _unitOfWork.Repository<ConstructionTemplate>();
        
        var project = await repoProject.Get().Where(p => p.Id == request.ProjectId).FirstOrDefaultAsync();
        
        if (project == null) {
            throw new BadRequestException("Dự án không tồn tại");
        }
        
        var templateConstruction = await repoTemplateConstruction.Get().Where(t => t.Id == request.TemplateConstructionId).FirstOrDefaultAsync();
        
        if (templateConstruction == null) {
            throw new BadRequestException("Mẫu công trình không tồn tại");
        }
        
        var quotation = await repoQuotation.Get().Where(q => q.Id == id).FirstOrDefaultAsync();
        
        if (quotation == null) {
            throw new BadRequestException("Báo giá không tồn tại");
        }
        
        if (quotation.Status != EnumQuotationStatus.REJECTED.ToString()) {
            throw new BadRequestException("Báo giá không thể chỉnh sửa");
        }
        
        quotation.Status = EnumQuotationStatus.CANCELLED.ToString();
        // write new quotation with current version
       // find the max version of quotation
      
        var quotationNew = new Quotation
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Idtemplate = request.TemplateConstructionId,
            Version = quotation.Version,
            Status = EnumQuotationStatus.OPEN.ToString(),
            TotalPrice = 0,
            PromotionId = request.PromotionId ?? null
        };
        await repoQuotation.AddAsync(quotationNew, false);
     

        int totalPrice = 0;
        
        foreach (var service in request.Services)
        {
            var serviceRaw = await repoService.Get().
                Where(s => s.Id == service.Id).
                FirstOrDefaultAsync();
            if (serviceRaw == null || serviceRaw.IsActive == false) {
                throw new  BadRequestException("Có dịch vụ không tồn tại");
            }
            var quotationService = new QuotationDetail
            {
                // Id = Guid.NewGuid(),
                QuotationId = quotationNew.Id,
                ServiceId = service.Id,
                Quantity = service.Quantity,
                Price =  serviceRaw.Price,
                Note = service.Note,
                Category = service.Category
            };
            totalPrice += serviceRaw.Price * service.Quantity;
            await repoQuotationService.AddAsync(quotationService,false);
        }
        
        foreach (var equipment in request.Equipments)
        {
            var equipmentRaw = await repoEquiment.Get().Where(e => e.Id == equipment.Id).FirstOrDefaultAsync();
            if (equipmentRaw == null || equipmentRaw.IsActive == false) {
                throw new  BadRequestException("Có thiết bị không tồn tại");
            }
            var quotationEquipment = new QuotationEquipment
            {
                Id = Guid.NewGuid(),
                QuotationId = quotationNew.Id,
                EquipmentId = equipment.Id,
                Quantity = equipment.Quantity,
                Price =  equipment.Price,
                Note = equipment.Note,
                Category = equipment.Category
            };
            totalPrice += equipment.Price * equipment.Quantity;
            await repoQuotationEquiment.AddAsync(quotationEquipment,false);
        }
        
        quotation.TotalPrice = totalPrice;
        await _unitOfWork.SaveChangesAsync();
    }
}