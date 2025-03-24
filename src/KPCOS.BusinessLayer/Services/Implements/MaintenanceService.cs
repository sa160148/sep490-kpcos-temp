using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.BusinessLayer.Services.Implements;

public class MaintenanceService : IMaintenanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MaintenanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task CreateMaintenancePackageItemAsync(CommandMaintenanceItemRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new ArgumentException("Name is required for maintenance item");
        }
        
        var maintenanceItem = _mapper.Map<MaintenanceItem>(request);
        await _unitOfWork.Repository<MaintenanceItem>().AddAsync(maintenanceItem);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GetAllMaintenanceItemResponse> data, int total)> GetAllMaintenanceItemAsync(GetAllMaintenanceItemFilterRequest request)
    {
        var repository = _unitOfWork.Repository<MaintenanceItem>();
        var filterExpression = request.GetExpressions();
        
        var (items, total) = repository.GetWithCount(
            filter: filterExpression,
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        var itemResponses = _mapper.Map<IEnumerable<GetAllMaintenanceItemResponse>>(items);
        return (itemResponses, total);
    }

    public async Task CreateMaintenancePackageAsync(CommandMaintenancePackageRequest request)
    {
        var maintenancePackage = _mapper.Map<MaintenancePackage>(request);
        maintenancePackage.Id = Guid.NewGuid();
        
        await _unitOfWork.Repository<MaintenancePackage>().AddAsync(maintenancePackage, false);
        
        // Add maintenance items to package if provided
        if (request.MaintenanceItems != null && request.MaintenanceItems.Any())
        {
            foreach (var itemId in request.MaintenanceItems)
            {
                var packageItem = new MaintenancePackageItem
                {
                    Id = Guid.NewGuid(),
                    MaintenancePackageId = maintenancePackage.Id,
                    MaintenanceItemId = itemId,
                };
                
                await _unitOfWork.Repository<MaintenancePackageItem>().AddAsync(packageItem, false);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GetAllMaintenancePackageResponse> data, int total)> GetAllMaintenancePackageAsync(GetAllMaintenancePackageFilterRequest request)
    {
        var repository = _unitOfWork.Repository<MaintenancePackage>();
        var filterExpression = request.GetExpressions();
        
        var (packages, total) = repository.GetWithCount(
            filter: filterExpression,
            includeProperties: "MaintenancePackageItems,MaintenancePackageItems.MaintenanceItem",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        var packageResponses = _mapper.Map<IEnumerable<GetAllMaintenancePackageResponse>>(packages);
        return (packageResponses, total);
    }
}
