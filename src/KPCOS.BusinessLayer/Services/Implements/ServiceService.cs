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
            throw new BadRequestException("Dịch vụ đã tồn tại");
        }
        
        if (!Enum.TryParse<EnumService>(request.Type, true, out var enumType))
        {
            throw new BadRequestException($"Loại dịch vụ '{request.Type}' không hợp lệ");
        }
        
        
        
        
        
        request.Unit = request.Type switch
        {
            _ when request.Type == EnumService.M3.ToString() => "m3",
            _ when request.Type == EnumService.M2.ToString() => "m2",
            _ => request.Unit
        };

     
        var service = new Service
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Unit = request.Unit,
            Type = request.Type
        };

        // Lưu vào database
        await serviceRepo.AddAsync(service, false);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceReponse> GetServiceByIdAsync(Guid id)
    {
        var service = await _unitOfWork.Repository<Service>().FindAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException("Dịch vụ không tồn tại");
        }

        return new ServiceReponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description ?? "",
            Price = service.Price,
            Unit = service.Unit,
            Type = service.Type
        };
    }

    public async Task UpdateServiceAsync(Guid id, ServiceCreateRequest request)
    {
        var serviceRepo = _unitOfWork.Repository<Service>();
        var service = await serviceRepo.SingleOrDefaultAsync(s => s.Id == id);
        
        if (service == null)
        {
            throw new NotFoundException("Dịch vụ không tồn tại");
        }

        if (!Enum.TryParse<EnumService>(request.Type, true, out var enumType))
        {
            throw new BadRequestException($"Loại dịch vụ '{request.Type}' không hợp lệ");
        }
        

        request.Unit = request.Type switch
        {
            _ when request.Type == EnumService.M3.ToString() => "m3",
            _ when request.Type == EnumService.M2.ToString() => "m2",
            _ => request.Unit
        };

        service.Name = request.Name;
        service.Description = request.Description;
        service.Price = request.Price;
        service.Unit = request.Unit;
        service.Type = request.Type;

        await serviceRepo.UpdateAsync(service);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(Guid id)
    {
        var serviceRepo = _unitOfWork.Repository<Service>();
        var service = await serviceRepo.SingleOrDefaultAsync(s => s.Id == id);
        
        if (service == null)
        {
            throw new NotFoundException("Dịch vụ không tồn tại");
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
            Type = service.Type
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
            Type = service.Type
        });
        return (pagedDataResponse, _unitOfWork.Repository<Service>().Get().Count());
    }
}