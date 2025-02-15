using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
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
            throw new BadRequestException("Giá không hợp lệ");
        }
        IRepository<Package> packageRepo = _unitOfWork.Repository<Package>();
        IRepository<PackageDetail> packageDetailRepo = _unitOfWork.Repository<PackageDetail>();
        var packageRaw = await packageRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (packageRaw != null)
        {
            throw new BadRequestException("Gói đã tồn tại");
        }
        _logger.LogInformation("Gói không tồn tại");
        
        
        
        var package = new Package
        {
            Name = request.Name,
        };
        await packageRepo.AddAsync(package);
        await _unitOfWork.SaveChangesAsync();
    }
}