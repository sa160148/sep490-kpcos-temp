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

public class PackageService : IPackageService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageService> _logger;
    
    public PackageService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CreatePackageAsync(PackageCreateRequest request)
    {
        if (request.Price <= 0)
        {
            throw new BadRequestException("Giá tiền không hợp lệ");
        }
        IRepository<Package> packageRepo = _unitOfWork.Repository<Package>();
        IRepository<PackageDetail> packageDetailRepo = _unitOfWork.Repository<PackageDetail>();
        
        var packageRaw = await packageRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (packageRaw != null)
        {
            throw new BadRequestException("Gói đã tồn tại");
        }
        // check if package detail is valid
        foreach (var packageDetail in request.Items)
        {
            var service = await _unitOfWork.Repository<PackageItem>().SingleOrDefaultAsync(s => s.Id == packageDetail.IdPackageItem);
            if (service == null)
            {
                throw new BadRequestException("có dịch vụ không tồn tại");
            }
        }
        
        var package = new Package
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
        };
        await packageRepo.AddAsync(package, false);
        
        foreach (var packageDetail in request.Items)
        {
            var detail = new PackageDetail
            {
                PackageId = package.Id,
                PackageItemId = packageDetail.IdPackageItem,
                Quantity = packageDetail.Quantity,
                Description = packageDetail.Description,
            };
            await packageDetailRepo.AddAsync(detail, false);
        }

        // Lưu thay đổi
        await _unitOfWork.SaveChangesAsync();
       
       
    }

    public async Task<(IEnumerable<PackageResponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        IRepository<Package> packgeRepo = _unitOfWork.Repository<Package>();
        var pageData = await _unitOfWork.Repository<Package>()
            .Get()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        var totalRecords = await packgeRepo.Get().CountAsync();
        
        var data = pageData.Select(package => new PackageResponse
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description ?? "",
            IsActive = package.IsActive,
            // generate price list 5 number with  package.price * 0.95 and descrease 5% each time
            Price = Enumerable.Range(0, 5).Select(i => (int)(package.Price * Math.Pow(0.95, i))).ToList(),
            Items = _unitOfWork.Repository<PackageDetail>()
                .Get()
                .Where(detail => detail.PackageId == package.Id)
                .Select(detail => new PackageResponse.PackageItem
                {
                    IdPackageItem = detail.PackageItemId,
                    Quantity = detail.Quantity,
                    Description = detail.Description,
                    Name = _unitOfWork.Repository<PackageItem>().Get()
                        .Where(item => item.Id == detail.PackageItemId)
                        .Select(item => item.Name)
                        .FirstOrDefault() ?? "Unknown"
                }).ToList()
        });
        return (data, totalRecords);
    }

    public async Task<PackageResponse> GetPackageByIdAsync(Guid id)
    {
        var packageRepo = _unitOfWork.Repository<Package>();
        var packageDetailRepo = _unitOfWork.Repository<PackageDetail>();
        var packageItemRepo = _unitOfWork.Repository<PackageItem>();

        // Fetch the package
        var package = await packageRepo.FindAsync(id);
        if (package == null)
        {
            throw new NotFoundException("Gói không tồn tại");
        }

        // Fetch package details and associated items in a single query
        var packageDetails = await packageDetailRepo.Get()
            .Where(detail => detail.PackageId == package.Id)
            .ToListAsync();

        // Fetch item names in bulk
        var itemIds = packageDetails.Select(detail => detail.PackageItemId).Distinct().ToList();
        var items = await packageItemRepo.Get()
            .Where(item => itemIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name);

        // Build the response
        var response = new PackageResponse
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description ?? "",
            IsActive = package.IsActive,
            Price = Enumerable.Range(0, 5).Select(i => (int)(package.Price * Math.Pow(0.95, i))).ToList(),
            Items = packageDetails.Select(detail => new PackageResponse.PackageItem
            {
                IdPackageItem = detail.PackageItemId,
                Quantity = detail.Quantity,
                Description = detail.Description,
                Name = items.TryGetValue(detail.PackageItemId, out var name) ? name : "Unknown"
            }).ToList()
        };
        return response;
    }

    public async Task UpdatePackageAsync(Guid id, PackageCreateRequest request)
    {
        if (request.Price <= 0)
        {
            throw new BadRequestException("Giá tiền không hợp lệ");
        }
        IRepository<Package> packageRepo = _unitOfWork.Repository<Package>();
        IRepository<PackageDetail> packageDetailRepo = _unitOfWork.Repository<PackageDetail>();
        
        var package = await packageRepo.SingleOrDefaultAsync(service => service!.Id == id);
        if (package == null)
        {
            throw new NotFoundException("Gói không tồn tại");
        }
        
        // check if package detail is valid
        foreach (var packageDetail in request.Items)
        {
            var service = await _unitOfWork.Repository<PackageItem>().SingleOrDefaultAsync(s => s.Id == packageDetail.IdPackageItem);
            if (service == null)
            {
                throw new BadRequestException("có dịch vụ không tồn tại");
            }
        }
        
        package.Name = request.Name;
        package.Description = request.Description;
        package.Price = request.Price;
        await packageRepo.UpdateAsync(package, false);
        
        // Remove all existing package details
        var existingDetails = await packageDetailRepo.Get()
            .Where(detail => detail.PackageId == package.Id)
            .ToListAsync();
        foreach (var detail in existingDetails)
        {
            await packageDetailRepo.RemoveAsync(detail, false);
        }
        
        // Add new package details
        foreach (var packageDetail in request.Items)
        {
            var detail = new PackageDetail
            {
                PackageId = package.Id,
                PackageItemId = packageDetail.IdPackageItem,
                Quantity = packageDetail.Quantity,
                Description = packageDetail.Description,
            };
            await packageDetailRepo.AddAsync(detail, false);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePackageAsync(Guid id)
    {
        IRepository<Package> packageRepo = _unitOfWork.Repository<Package>();
        IRepository<PackageDetail> packageDetailRepo = _unitOfWork.Repository<PackageDetail>();
        
        var package = await packageRepo.SingleOrDefaultAsync(service => service!.Id == id);
        if (package == null)
        {
            throw new NotFoundException("Gói không tồn tại");
        }
        
        // Remove all existing package details
        var existingDetails = await packageDetailRepo.Get()
            .Where(detail => detail.PackageId == package.Id)
            .ToListAsync();
        foreach (var detail in existingDetails)
        {
            await packageDetailRepo.RemoveAsync(detail, false);
        }
        
        await packageRepo.RemoveAsync(package, false);
        await _unitOfWork.SaveChangesAsync();
    }
}