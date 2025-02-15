using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceService> _logger;
    
    public ServiceService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ServiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task CreateService(ServiceCreateRequest request)
    {
        
        IRepository<Service> serviceRepo = _unitOfWork.Repository<Service>();
        var serviceRaw = await serviceRepo.SingleOrDefaultAsync(service => service!.Name == request.Name);
        if (serviceRaw != null)
        {
            throw new BadRequestException("Service already exists");
        }
        _logger.LogInformation("Service does not exist");
        
        if (!Enum.TryParse<EnumService>(request.Type, true, out var enumType))
        {
            throw new BadRequestException($"Service type '{request.Type}' is not valid");
        }

        if (!EnumServiceDetails.EnumServiceMapping.ContainsKey(enumType))
        {
            throw new BadRequestException("Service type is not valid");
        }

       
        var typeDetails = EnumServiceDetails.EnumServiceMapping[enumType];

     
        var service = new Service
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Unit = request.Unit,
            Type = typeDetails.Value
        };

        // Lưu vào database
        await serviceRepo.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceReponse> GetServiceByIdAsync(Guid id)
    {
        var service = await _unitOfWork.Repository<Service>().FindAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException("Service not found");
        }

        return new ServiceReponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description ?? "",
            Price = service.Price,
            Unit = service.Unit,
            Type = EnumServiceDetails.EnumServiceMapping.FirstOrDefault(x => x.Value.Value == service.Type).Key.ToString()
        };
    }

    public async Task UpdateServiceAsync(Guid id, ServiceCreateRequest request)
    {
        var serviceRepo = _unitOfWork.Repository<Service>();
        var service = await serviceRepo.SingleOrDefaultAsync(s => s.Id == id);
        
        if (service == null)
        {
            throw new NotFoundException("Service not found");
        }

        if (!Enum.TryParse<EnumService>(request.Type, true, out var enumType))
        {
            throw new BadRequestException($"Service type '{request.Type}' is not valid");
        }

        if (!EnumServiceDetails.EnumServiceMapping.ContainsKey(enumType))
        {
            throw new BadRequestException("Service type is not valid");
        }

        var typeDetails = EnumServiceDetails.EnumServiceMapping[enumType];

        service.Name = request.Name;
        service.Description = request.Description;
        service.Price = request.Price;
        service.Unit = request.Unit;
        service.Type = typeDetails.Value;

        await serviceRepo.UpdateAsync(service);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(Guid id)
    {
        var serviceRepo = _unitOfWork.Repository<Service>();
        var service = await serviceRepo.SingleOrDefaultAsync(s => s.Id == id);
        
        if (service == null)
        {
            throw new NotFoundException("Service not found");
        }

        await serviceRepo.RemoveAsync(service);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<PaginationResult<ServiceReponse>> GetsAsync(PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
        var services = await _unitOfWork.Repository<Service>().Get().ToListAsync();
        var pagedData = services.Skip((validFilter.PageNumber - 1) * validFilter.PageSize).Take(validFilter.PageSize).Select(service => new ServiceReponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description ?? "",
            Price = service.Price,
            Unit = service.Unit,
            Type = EnumServiceDetails.EnumServiceMapping.FirstOrDefault(x => x.Value.Value == service.Type).Key.ToString()
        }).ToList();
        return new PaginationResult<ServiceReponse>(pagedData, validFilter.PageNumber, validFilter.PageSize, services.Count);
    }
    
    public async Task<(IEnumerable<ServiceReponse> Data, int TotalRecords)> GetsAsyncPaging(PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
        var pagedData = await _unitOfWork.Repository<Service>()
            .Get()
            .OrderBy(service => service.Id)
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();
        var pagedDataResponse = pagedData.Select(service => new ServiceReponse 
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description ?? "",
            Price = service.Price,
            Unit = service.Unit,
            Type = EnumServiceDetails.EnumServiceMapping.FirstOrDefault(x => x.Value.Value == service.Type).Key.ToString()
        });
        return (pagedDataResponse, _unitOfWork.Repository<Service>().Get().Count());
    }
}