
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace KPCOS.BusinessLayer.Services.Implements;

public class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private IServiceService _serviceServiceImplementation;
    private readonly ILogger<ServiceService> _logger;
    
    public ServiceService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ServiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task CreateService(ServiceCreateRequest request)
    {
        
        IRepository<Service?> serviceRepo = _unitOfWork.Repository<Service>();
        var serviceRaw = await serviceRepo.SingleOrDefaultAsync(service => service.Name == request.name);
        if (serviceRaw != null)
        {
            throw new BadRequestException("Service already exists");
        }
        _logger.LogInformation("Service does not exist");
        
        if (!Enum.TryParse<EnumService>(request.type, true, out var enumType))
        {
            throw new BadRequestException($"Service type '{request.type}' is not valid");
        }

        if (!EnumServiceDetails.EnumServiceMapping.ContainsKey(enumType))
        {
            throw new BadRequestException("Service type is not valid");
        }

       
        var typeDetails = EnumServiceDetails.EnumServiceMapping[enumType];

     
        var service = new Service
        {
            Name = request.name,
            Description = request.description,
            Price = request.price,
            Unit = request.unit,
            Type = typeDetails.Value
        };

        // Lưu vào database
        await serviceRepo.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();
    }

}