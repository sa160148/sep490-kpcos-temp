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
        _logger.LogInformation("Gói không tồn tại");
        
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
        await packageRepo.AddAsync(package);
        
        foreach (var packageDetail in request.Items)
        {
            var detail = new PackageDetail
            {
                PackageId = package.Id,
                PackageItemId = packageDetail.IdPackageItem,
                Quantity = packageDetail.Quantity,
                Description = packageDetail.Description,
            };
            await packageDetailRepo.AddAsync(detail);
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
                }).ToList()
        });
        return (data, totalRecords);
    }
}