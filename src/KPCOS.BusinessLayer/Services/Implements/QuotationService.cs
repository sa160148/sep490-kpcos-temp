using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
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
    
    public QuotationService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }


    public async Task CreateQuotationAsync(QuotationCreateRequest request)
    {
        var repoService = _unitOfWork.Repository<Service>();
        var repoEquiment = _unitOfWork.Repository<Equipment>();
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var repoQuotationService = _unitOfWork.Repository<QuotationDetail>();
        var repoQuotationEquiment = _unitOfWork.Repository<QuotationEquipment>();
        var quotation = new Quotation
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Idtemplate = request.TemplateConstructionId,
            Version = await repoQuotation.Get().Where(q => q.ProjectId == request.ProjectId).CountAsync() + 1,
            Status = EnumQuotationStatus.OPEN.ToString(),
        };
        await repoQuotation.AddAsync(quotation, false);
        
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
                Id = Guid.NewGuid(),
                QuotationId = quotation.Id,
                ServiceId = service.Id,
                Quantity = service.Quantity,
                Price =  serviceRaw.Price,
                Note = service.Note,
                Category = service.Category
            };
            await repoQuotationService.AddAsync(quotationService, false);
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
            await repoQuotationEquiment.AddAsync(quotationEquipment, false);
        }
        await _unitOfWork.SaveChangesAsync();
        
    }

    public async Task<(IEnumerable<QuotationResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        var repoService = _unitOfWork.Repository<Service>();
        var repoEquiment = _unitOfWork.Repository<Equipment>();
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var repoQuotationService = _unitOfWork.Repository<QuotationDetail>();
        var repoQuotationEquiment = _unitOfWork.Repository<QuotationEquipment>();
        
        var pageData = await repoQuotation.Get()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        
        var totalRecords = await repoQuotation.Get().CountAsync();
        
        var data = pageData.Select(quotation => new QuotationResponse
        {
            Id = quotation.Id,
            ProjectId = quotation.ProjectId,
            TemplateConstructionId = quotation.Idtemplate,
            Version = quotation.Version,
            Status = quotation.Status, 
            CreatedDate = quotation.CreatedAt.ToString(),
            UpdatedDate = quotation.UpdatedAt.ToString(),
            Services = repoQuotationService.Get().Where(qs => qs.QuotationId == quotation.Id)
                .Select(qs => new QuotationResponse.Service
                {
                    Id = qs.ServiceId,
                    Name = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Name).FirstOrDefault(),
                    Description = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Description).FirstOrDefault(),
                    Price = qs.Price,
                    Unit = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Unit).FirstOrDefault(),
                    Type = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Type).FirstOrDefault(),
                    Note = qs.Note,
                    Category = qs.Category
                }).ToList(),
            Equipments = repoQuotationEquiment.Get().Where(qe => qe.QuotationId == quotation.Id)
                .Select(qe => new QuotationResponse.Equipment
                {
                    Id = qe.EquipmentId,
                    Name = repoEquiment.Get().Where(e => e.Id == qe.EquipmentId).Select(e => e.Name).FirstOrDefault(),
                    Description = repoEquiment.Get().Where(e => e.Id == qe.EquipmentId).Select(e => e.Description).FirstOrDefault(),
                    Quantity = qe.Quantity,
                    Price = qe.Price,
                    Note = qe.Note,
                    Category = qe.Category
                }).ToList()
        });
        
        return (data, totalRecords);
    }

    public async Task<QuotationResponse> GetQuotationByIdAsync(Guid id)
    {
        var repoService = _unitOfWork.Repository<Service>();
        var repoEquiment = _unitOfWork.Repository<Equipment>();
        var repoQuotation = _unitOfWork.Repository<Quotation>();
        var repoQuotationService = _unitOfWork.Repository<QuotationDetail>();
        var repoQuotationEquiment = _unitOfWork.Repository<QuotationEquipment>();
        
        var quotation = await repoQuotation.Get().Where(q => q.Id == id).FirstOrDefaultAsync();
        if (quotation == null) {
            throw new BadRequestException("Báo giá không tồn tại");
        }
        
        var data = new QuotationResponse
        {
            Id = quotation.Id,
            ProjectId = quotation.ProjectId,
            TemplateConstructionId = quotation.Idtemplate,
            Version = quotation.Version,
            Status = quotation.Status, 
            CreatedDate = quotation.CreatedAt.ToString(),
            UpdatedDate = quotation.UpdatedAt.ToString(),
            Services = repoQuotationService.Get().Where(qs => qs.QuotationId == quotation.Id)
                .Select(qs => new QuotationResponse.Service
                {
                    Id = qs.ServiceId,
                    Name = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Name).FirstOrDefault(),
                    Description = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Description).FirstOrDefault(),
                    Price = qs.Price,
                    Unit = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Unit).FirstOrDefault(),
                    Type = repoService.Get().Where(s => s.Id == qs.ServiceId).Select(s => s.Type).FirstOrDefault(),
                    Note = qs.Note,
                    Category = qs.Category
                }).ToList(),
            Equipments = repoQuotationEquiment.Get().Where(qe => qe.QuotationId == quotation.Id)
                .Select(qe => new QuotationResponse.Equipment
                {
                    Id = qe.EquipmentId,
                    Name = repoEquiment.Get().Where(e => e.Id == qe.EquipmentId).Select(e => e.Name).FirstOrDefault(),
                    Description = repoEquiment.Get().Where(e => e.Id == qe.EquipmentId).Select(e => e.Description).FirstOrDefault(),
                    Quantity = qe.Quantity,
                    Price = qe.Price,
                    Note = qe.Note,
                    Category = qe.Category
                }).ToList()
        };
        
        return data;
    }
}