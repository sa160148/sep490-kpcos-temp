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

public class PackageItemService : IPackageItemService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageItemService> _logger;

    public PackageItemService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CreatePackageItemAsync(PackageItemCreateRequest request)
    {
        IRepository<PackageItem> packgeItemRepo = _unitOfWork.Repository<PackageItem>();
        var packageItemRaw = await packgeItemRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (packageItemRaw != null)
        {
            throw new BadRequestException("Mục đã tồn tại");
        }
       
        
        var packageItem = new PackageItem
        {
            Name = request.Name,
        };
        await packgeItemRepo.AddAsync(packageItem, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PackageItemResponse> GetPackageItemByIdAsync(Guid id)
    {
        IRepository<PackageItem> packgeItemRepo = _unitOfWork.Repository<PackageItem>(); 
        var packageItem =  await packgeItemRepo.FindAsync(id);
        if (packageItem == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        return new PackageItemResponse
        {
            Id = packageItem.Id,
            Name = packageItem.Name,
        };
    }

    public async Task UpdatePackageItemAsync(Guid id, PackageItemCreateRequest request)
    {
        IRepository<PackageItem> packgeItemRepo = _unitOfWork.Repository<PackageItem>();
        var packageItem =  await packgeItemRepo.FindAsync(id);
        if (packageItem == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        var packageItemRaw = await packgeItemRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (packageItemRaw != null)
        {
            throw new BadRequestException("Mục đã tồn tại");
        }
        packageItem.Name = request.Name;
        await _unitOfWork.SaveChangesAsync();
        
    }

    public async Task DeletePackageItemAsync(Guid id)
    {
        IRepository<PackageItem> packgeItemRepo = _unitOfWork.Repository<PackageItem>();
        var packageItem = await packgeItemRepo.FindAsync(id);
        if (packageItem == null)
        {
            throw new BadRequestException("Mục không tồn tại");
        }
        await packgeItemRepo.RemoveAsync(packageItem, false);
        await _unitOfWork.SaveChangesAsync();
        
    }

    public async Task<(IEnumerable<PackageItemResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        IRepository<PackageItem> packgeItemRepo = _unitOfWork.Repository<PackageItem>();

        var pageData = await _unitOfWork.Repository<PackageItem>()
            .Get()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        
        var totalRecords = await packgeItemRepo.Get().CountAsync();
        return (pageData.Select(packageItem => new PackageItemResponse
        {
            Id = packageItem.Id,
            Name = packageItem.Name,
        }), totalRecords);
    }
}