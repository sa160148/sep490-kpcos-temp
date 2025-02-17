using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class EquipmentService : IEquipmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageService> _logger;

    public EquipmentService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CreateEquipmentAsync(EquipmentCreateRequest request)
    {
        IRepository<Equipment> equipmentRepo = _unitOfWork.Repository<Equipment>();
        var equipmentRaw = await equipmentRepo.SingleOrDefaultAsync(equipment => equipment!.Name == request.Name);
        if (equipmentRaw != null)
        {
            throw new BadRequestException("Thiết bị đã tồn tại");
        }
        _logger.LogInformation("Thiết bị không tồn tại");

        var equipment = new Equipment
        {
            Name = request.Name,
            Description = request.Description,
        };
        await equipmentRepo.AddAsync(equipment);
    }

    public async Task<(IEnumerable<EquipmentResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        IRepository<Equipment> equipmentRepo = _unitOfWork.Repository<Equipment>();
        var pageData = await _unitOfWork.Repository<Equipment>()
            .Get()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        var totalRecords = await equipmentRepo.Get().CountAsync();
        return (Data: pageData.Select(equipment => new EquipmentResponse
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Description = equipment.Description,
        }), TotalRecords: totalRecords);
    }

    public async Task<EquipmentResponse> GetEquipmentByIdAsync(Guid id)
    {
        IRepository<Equipment> equipmentRepo = _unitOfWork.Repository<Equipment>();
        var equipment = await equipmentRepo.FindAsync(id);
        if (equipment == null)
        {
            throw new BadRequestException("Thiết bị không tồn tại");
        }
        return new EquipmentResponse
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Description = equipment.Description,
        };
    }

    public async Task UpdateEquipmentAsync(Guid id, EquipmentCreateRequest request)
    {
        IRepository<Equipment> equipmentRepo = _unitOfWork.Repository<Equipment>();
        var equipment = await equipmentRepo.FindAsync(id);
        if (equipment == null)
        {
            throw new BadRequestException("Thiết bị không tồn tại");
        }
        var equipmentRaw = await equipmentRepo.SingleOrDefaultAsync(equipment => equipment!.Name == request.Name);
        if (equipmentRaw != null)
        {
            throw new BadRequestException("Thiết bị đã tồn tại");
        }
        equipment.Name = request.Name;
        equipment.Description = request.Description;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteEquipmentAsync(Guid id)
    {
        IRepository<Equipment> equipmentRepo = _unitOfWork.Repository<Equipment>();
        var equipment = await equipmentRepo.FindAsync(id);
        if (equipment == null)
        {
            throw new BadRequestException("Thiết bị không tồn tại");
        }
        equipmentRepo.RemoveAsync(equipment);
        await _unitOfWork.SaveChangesAsync();
    }
}