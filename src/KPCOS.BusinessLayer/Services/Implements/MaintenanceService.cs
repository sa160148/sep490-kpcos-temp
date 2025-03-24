using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace KPCOS.BusinessLayer.Services.Implements;

public class MaintenanceService : IMaintenanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public MaintenanceService(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IHttpClientFactory httpClientFactory)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Calculates the total maintenance cost based on area, depth, duration, and package details
    /// </summary>
    /// <param name="area">Area of the maintenance object (pond/pool)</param>
    /// <param name="depth">Depth of the maintenance object</param>
    /// <param name="duration">Number of maintenance tasks (months)</param>
    /// <param name="package">The maintenance package</param>
    /// <returns>Total price with all applicable discounts</returns>
    private int CalculateMaintenancePrice(double area, double depth, int duration, MaintenancePackage package)
    {
        // First calculate base price for a single maintenance task based on volume
        double cubicMeters = area * depth;
        int initialPrice = package.Price;
        int priceDropPerCubic = (int)(package.Rate / 100.0 * initialPrice);
        int minPrice = 0; // Default minimum price
        
        // Get price for a single maintenance task based on volume
        int baseMonthlyPrice = GlobalUtility.CalculatePrice(cubicMeters, initialPrice, priceDropPerCubic, minPrice);
        
        // Create discount groups based on duration - SIMPLIFIED VERSION
        var discountGroups = new Dictionary<int, int>();
        
        // Only use 6-month and 12-month groups
        if (duration >= 12) // 1 year of monthly maintenance
        {
            // Medium discount for 12+ month contracts - 20% discount
            int discountPerMonth = (int)(initialPrice * 0.20); 
            discountGroups.Add(12, discountPerMonth);
            discountGroups.Add(6, (int)(initialPrice * 0.15)); // 15% for groups of 6 months
            discountGroups.Add(1, (int)(initialPrice * 0.10)); // 10% for individual months
        }
        else if (duration >= 6) // 6 months of maintenance
        {
            // Smaller discount for 6+ month contracts - 15% discount
            int discountPerMonth = (int)(initialPrice * 0.15);
            discountGroups.Add(6, discountPerMonth);
            discountGroups.Add(1, (int)(initialPrice * 0.05)); // 5% for individual months
        }
        else // Less than 6 months
        {
            // Minimal discount for short-term contracts - 5% discount
            int discountPerMonth = (int)(initialPrice * 0.05);
            discountGroups.Add(1, discountPerMonth);
        }
        
        // Calculate total price with month-based group discounts
        return GlobalUtility.CalculatePriceWithMonthGroups(baseMonthlyPrice, duration, discountGroups);
    }

    public async Task CreateMaintenanceRequestAsync(CommandMaintenanceRequest request, Guid customerId)
    {
        // Validate required fields
        if (request.MaintenancePackageId == null)
        {
            throw new ArgumentException("Maintenance package ID is required");
        }
        
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new ArgumentException("Name is required for maintenance request");
        }
        
        if (request.Area == null || request.Depth == null)
        {
            throw new ArgumentException("Area and Depth are required for maintenance request");
        }
        
        if (string.IsNullOrEmpty(request.Address))
        {
            throw new ArgumentException("Address is required for maintenance request");
        }
        
        if (request.Type == null)
        {
            throw new ArgumentException("Type is required for maintenance request");
        }
        
        // Set duration based on request and validate if needed
        int duration = request.Duration.HasValue && request.Duration.Value > 0 
            ? request.Duration.Value 
            : 1;
            
        // Additional validation for UNSCHEDULED type
        if (request.Type.Equals(EnumMaintenanceRequestType.UNSCHEDULED.ToString(), StringComparison.OrdinalIgnoreCase) 
            && duration > 2)
        {
            throw new ArgumentException("Unscheduled maintenance requests cannot have more than 2 maintenance tasks");
        }
        
        // Verify maintenance package exists
        var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>()
            .FindAsync(request.MaintenancePackageId.Value);
        
        if (maintenancePackage == null)
        {
            throw new NotFoundException($"Maintenance package with ID {request.MaintenancePackageId} not found");
        }

        // Get maintenance package items associated with the package
        var maintenancePackageItems = _unitOfWork.Repository<MaintenancePackageItem>()
            .Get(
                filter: mpi => mpi.MaintenancePackageId == maintenancePackage.Id && mpi.IsActive == true,
                includeProperties: "MaintenanceItem"
            );
        
        // Make sure the package has maintenance items
        if (!maintenancePackageItems.Any())
        {
            throw new NotFoundException($"Maintenance package with ID {request.MaintenancePackageId} has no active maintenance items");
        }

        // Calculate the total value
        int totalValue;
        if (request.TotalValue.HasValue)
        {
            // Use the specified total value if provided
            totalValue = request.TotalValue.Value;
        }
        else
        {
            // Calculate price using the new method that coordinates both calculations
            totalValue = CalculateMaintenancePrice(
                request.Area.Value, 
                request.Depth.Value, 
                duration, 
                maintenancePackage);
        }

        var customer = await _unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(x => x.UserId == customerId);
        
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found");
        }

        // Create new maintenance request
        var maintenanceRequest = new MaintenanceRequest
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Area = request.Area ?? 0,
            Depth = request.Depth ?? 0,
            Address = request.Address,
            TotalValue = totalValue,
            Type = request.Type,
            IsPaid = false, // Ignore the IsPaid from request as specified
            CustomerId = customer.Id,
            MaintenancePackageId = request.MaintenancePackageId.Value,
            Status = EnumMaintenanceRequestTaskStatus.OPENING.ToString()
        };
        
        // Add the maintenance request
        await _unitOfWork.Repository<MaintenanceRequest>().AddAsync(maintenanceRequest, false);
        
        // Create maintenance request tasks if estimate date is provided
        if (request.EstimateAt.HasValue)
        {
            // Get HttpClient to fetch holidays
            var httpClient = _httpClientFactory.CreateClient();
            
            // Calculate maintenance dates (avoiding weekends and holidays)
            var maintenanceDates = await GlobalUtility.GetMaintenanceDatesAsync(
                request.EstimateAt.Value, 
                duration, 
                httpClient);
            
            // Create tasks for each date and for each maintenance item
            foreach (var date in maintenanceDates)
            {
                foreach (var packageItem in maintenancePackageItems)
                {
                    var maintenanceItem = packageItem.MaintenanceItem;
                    
                    if (maintenanceItem == null)
                    {
                        // Skip if the maintenance item is null (shouldn't happen but just in case)
                        continue;
                    }
                    
                    var taskName = $"{maintenanceItem.Name} - {date.ToString("yyyy-MM-dd")}";
                    var taskDescription = !string.IsNullOrEmpty(maintenanceItem.Description) 
                        ? $"{maintenanceItem.Description} - Scheduled for {date.ToString("yyyy-MM-dd")}" 
                        : $"Scheduled maintenance for {date.ToString("yyyy-MM-dd")}";
                    
                    var maintenanceRequestTask = new MaintenanceRequestTask
                    {
                        Id = Guid.NewGuid(),
                        MaintenanceRequestId = maintenanceRequest.Id,
                        Name = taskName,
                        Description = taskDescription,
                        EstimateAt = date,
                        MaintenanceItemId = maintenanceItem.Id,
                        Status = EnumMaintenanceRequestTaskStatus.OPENING.ToString()
                    };
                    
                    await _unitOfWork.Repository<MaintenanceRequestTask>().AddAsync(maintenanceRequestTask, false);
                }
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GetAllMaintenanceRequestResponse> data, int total)> GetMaintenanceRequestsAsync(GetAllMaintenanceRequestFilterRequest request)
    {
        var repository = _unitOfWork.Repository<MaintenanceRequest>();
        var filterExpression = request.GetExpressions();
        
        var (requests, total) = repository.GetWithCount(
            filter: filterExpression,
            includeProperties: "MaintenancePackage,Customer,MaintenanceRequestTasks",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        var requestResponses = _mapper.Map<IEnumerable<GetAllMaintenanceRequestResponse>>(requests);
        return (requestResponses, total);
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

    public async Task UpdateMaintenanceTaskStatusAsync(Guid id, CommandMaintenanceRequestTaskRequest request)
    {
        // Get the maintenance request task
        var maintenanceRequestTask = await _unitOfWork.Repository<MaintenanceRequestTask>()
            .FindAsync(id);
            
        if (maintenanceRequestTask == null)
        {
            throw new NotFoundException($"Maintenance request task with ID {id} not found");
        }
        
        // Get the current status
        var currentStatus = maintenanceRequestTask.Status;

        // Determine which update mode based on the request fields
        if (request.StaffId.HasValue)
        {
            // First mode: Update staff assignment and change status to PROCESSING
            await UpdateStaffAssignmentAsync(maintenanceRequestTask, request.StaffId.Value);
        }
        else if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            // Second mode: Update image URL and change status to PREVIEWING
            maintenanceRequestTask.ImageUrl = request.ImageUrl;
            maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PREVIEWING.ToString();
        }
        else if (!string.IsNullOrEmpty(request.Reason))
        {
            // Third mode: Update reason and change status to PROCESSING
            maintenanceRequestTask.Reason = request.Reason;
            maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
        }
        
        // Update other provided fields if they exist
        if (!string.IsNullOrEmpty(request.Name))
        {
            maintenanceRequestTask.Name = request.Name;
        }
        
        if (!string.IsNullOrEmpty(request.Description))
        {
            maintenanceRequestTask.Description = request.Description;
        }
        
        await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask);
        await _unitOfWork.SaveChangesAsync();
    }
    
    private async Task UpdateStaffAssignmentAsync(MaintenanceRequestTask maintenanceRequestTask, Guid staffId)
    {
        // Get the staff by ID
        var staff = await _unitOfWork.Repository<Staff>().FindAsync(staffId);
        if (staff == null)
        {
            throw new NotFoundException($"Staff with ID {staffId} not found");
        }
        
        // Validate staff position is CONSTRUCTOR
        if (staff.Position != RoleEnum.CONSTRUCTOR.ToString())
        {
            throw new InvalidOperationException("Only staff with CONSTRUCTOR position can be assigned to maintenance tasks");
        }
        
        // Check if the staff is already assigned to other active tasks
        // Construction tasks
        var hasActiveConstructionTasks = await _unitOfWork.Repository<ConstructionTask>()
            .Where(ct => ct.StaffId == staffId && ct.Status != "DONE")
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveConstructionTasks)
        {
            throw new InvalidOperationException("Staff is already assigned to active construction tasks");
        }
        
        // Project issues
        var hasActiveProjectIssues = await _unitOfWork.Repository<ProjectIssue>()
            .Where(pi => pi.StaffId == staffId && pi.Status != "DONE")
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveProjectIssues)
        {
            throw new InvalidOperationException("Staff is already assigned to active project issues");
        }
        
        // Other maintenance request tasks that are not done
        var hasActiveMaintenanceTasks = await _unitOfWork.Repository<MaintenanceRequestTask>()
            .Where(mrt => mrt.StaffId == staffId && mrt.Id != maintenanceRequestTask.Id && mrt.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString())
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveMaintenanceTasks)
        {
            throw new InvalidOperationException("Staff is already assigned to active maintenance tasks");
        }
        
        // All validations passed, assign the staff and update status
        maintenanceRequestTask.StaffId = staffId;
        maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
        
        // Update the maintenance request status if it's still OPENING
        var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
            .FindAsync(maintenanceRequestTask.MaintenanceRequestId);
            
        if (maintenanceRequest != null && maintenanceRequest.Status == EnumMaintenanceRequestTaskStatus.OPENING.ToString())
        {
            maintenanceRequest.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
            await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
        }
    }
    
    public async Task ConfirmMaintenanceTaskAsync(Guid id)
    {
        // Get the maintenance request task
        var maintenanceRequestTask = await _unitOfWork.Repository<MaintenanceRequestTask>()
            .FindAsync(id);
            
        if (maintenanceRequestTask == null)
        {
            throw new NotFoundException($"Maintenance request task with ID {id} not found");
        }
        
        // Validate current status is PREVIEWING
        if (maintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.PREVIEWING.ToString())
        {
            throw new InvalidOperationException($"Cannot confirm maintenance task that is not in PREVIEWING status. Current status: {maintenanceRequestTask.Status}");
        }
        
        // Change status to DONE
        maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.DONE.ToString();
        await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask, false);
        
        // Check if all other maintenance request tasks with the same maintenance request ID are DONE
        var allTasksDone = true;
        var maintenanceRequestId = maintenanceRequestTask.MaintenanceRequestId;
        
        var otherTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
            .Get(
                filter: mrt => mrt.MaintenanceRequestId == maintenanceRequestId && mrt.Id != id,
                includeProperties: ""
            );
            
        foreach (var task in otherTasks)
        {
            if (task.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString())
            {
                allTasksDone = false;
                break;
            }
        }
        
        // If all tasks are done, update maintenance request status
        if (allTasksDone)
        {
            var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
                .FindAsync(maintenanceRequestId);
                
            if (maintenanceRequest != null && maintenanceRequest.Status == EnumMaintenanceRequestTaskStatus.PROCESSING.ToString())
            {
                maintenanceRequest.Status = EnumMaintenanceRequestTaskStatus.DONE.ToString();
                await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}
