using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Linq.Expressions;
using LinqKit;
using KPCOS.BusinessLayer.DTOs.Response.Feedbacks;

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
            IsPaid = false,
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
            
            // Create tasks for each date
            foreach (var date in maintenanceDates)
            {
                // Create parent task (Level 1)
                var parentTask = new MaintenanceRequestTask
                {
                    Id = Guid.NewGuid(),
                    MaintenanceRequestId = maintenanceRequest.Id,
                    Name = $"Maintenance Visit - {date:yyyy-MM-dd}",
                    Description = $"Scheduled maintenance visit for {date:yyyy-MM-dd}",
                    EstimateAt = date,
                    ParentId = null, // Level 1 tasks have no parent
                    MaintenanceItemId = null, // Level 1 tasks have no maintenance item
                    Status = EnumMaintenanceRequestTaskStatus.OPENING.ToString()
                };
                
                await _unitOfWork.Repository<MaintenanceRequestTask>().AddAsync(parentTask, false);
                
                // Create child tasks (Level 2) for each maintenance item
                foreach (var packageItem in maintenancePackageItems)
                {
                    var maintenanceItem = packageItem.MaintenanceItem;
                    
                    if (maintenanceItem == null)
                    {
                        continue;
                    }
                    
                    var childTask = new MaintenanceRequestTask
                    {
                        Id = Guid.NewGuid(),
                        MaintenanceRequestId = maintenanceRequest.Id,
                        Name = $"{maintenanceItem.Name} - {date:yyyy-MM-dd}",
                        Description = !string.IsNullOrEmpty(maintenanceItem.Description) 
                            ? $"{maintenanceItem.Description} - Scheduled for {date:yyyy-MM-dd}" 
                            : $"Scheduled maintenance for {date:yyyy-MM-dd}",
                        EstimateAt = date, // Same date as parent
                        ParentId = parentTask.Id, // Reference to parent task
                        MaintenanceItemId = maintenanceItem.Id,
                        Status = EnumMaintenanceRequestTaskStatus.OPENING.ToString()
                    };
                    
                    await _unitOfWork.Repository<MaintenanceRequestTask>().AddAsync(childTask, false);
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
            includeProperties: "MaintenancePackage,Customer,Customer.User,MaintenanceRequestTasks,MaintenanceRequestTasks.Staff,MaintenanceRequestTasks.Staff.User",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        // Filter out child tasks (tasks with ParentId) from each maintenance request
        foreach (var maintenanceRequest in requests)
        {
            maintenanceRequest.MaintenanceRequestTasks = maintenanceRequest.MaintenanceRequestTasks
                .Where(t => t.ParentId == null)
                .ToList();
        }

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
            throw new NotFoundException($"Không tìm thấy công việc bảo trì với ID {id}");
        }
        if (maintenanceRequestTask.ParentId == null)
        {
            throw new BadRequestException("Không thể cập nhật trạng thái cho công việc cha");
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
        var staff = await _unitOfWork.Repository<Staff>()
        .SingleOrDefaultAsync(x => x.UserId == staffId);
        if (staff == null)
        {
            throw new NotFoundException($"Không tìm thấy nhân viên với ID người dùng {staffId}");
        }
        
        // Validate staff position is CONSTRUCTOR
        if (staff.Position != RoleEnum.CONSTRUCTOR.ToString())
        {
            throw new InvalidOperationException("Chỉ nhân viên có chức vụ CONSTRUCTOR mới có thể được phân công cho công việc bảo trì");
        }
        
        // Check if the staff is already assigned to other active tasks
        // Construction tasks
        var hasActiveConstructionTasks = await _unitOfWork.Repository<ConstructionTask>()
            .Where(ct => ct.StaffId == staff.Id && ct.Status != "DONE")
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveConstructionTasks)
        {
            throw new InvalidOperationException("Nhân viên đã được phân công cho các công việc xây dựng đang hoạt động");
        }
        
        // Project issues
        var hasActiveProjectIssues = await _unitOfWork.Repository<ProjectIssue>()
            .Where(pi => pi.StaffId == staff.Id && pi.Status != "DONE")
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveProjectIssues)
        {
            throw new InvalidOperationException("Nhân viên đã được phân công cho các vấn đề dự án đang hoạt động");
        }
        
        // Other maintenance request tasks that are not done
        var hasActiveMaintenanceTasks = await _unitOfWork.Repository<MaintenanceRequestTask>()
            .Where(mrt => mrt.StaffId == staff.Id && mrt.Id != maintenanceRequestTask.Id && mrt.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString())
            .FirstOrDefaultAsync() != null;
            
        if (hasActiveMaintenanceTasks)
        {
            throw new InvalidOperationException("Nhân viên đã được phân công cho các công việc bảo trì đang hoạt động");
        }
        
        // All validations passed, assign the staff and update status
        maintenanceRequestTask.StaffId = staff.Id;
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
            throw new NotFoundException($"Không tìm thấy công việc bảo trì với ID {id}");
        }
        
        // Validate current status is PREVIEWING
        if (maintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.PREVIEWING.ToString())
        {
            throw new BadRequestException($"Không thể xác nhận hoàn thành công việc bảo trì không ở trạng thái PREVIEWING. Trạng thái hiện tại: {maintenanceRequestTask.Status}");
        }

        // Validate this is a level 2 task (has ParentId)
        if (maintenanceRequestTask.ParentId == null)
        {
            throw new BadRequestException("Chỉ có thể xác nhận hoàn thành công việc bảo trì cấp 2");
        }
        
        // Change status to DONE
        maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.DONE.ToString();
        await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask, false);
        
        // Check if all other level 2 tasks under the same parent are DONE
        var parentId = maintenanceRequestTask.ParentId.Value;
        var siblingTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
            .Get(
                filter: mrt => mrt.ParentId == parentId,
                includeProperties: ""
            );
            
        var allSiblingsDone = siblingTasks.All(task => 
            task.Status == EnumMaintenanceRequestTaskStatus.DONE.ToString());
        
        if (allSiblingsDone)
        {
            // Update parent task (level 1) to DONE
            var parentTask = await _unitOfWork.Repository<MaintenanceRequestTask>()
                .FindAsync(parentId);
                
            if (parentTask != null && parentTask.Status == EnumMaintenanceRequestTaskStatus.PROCESSING.ToString())
            {
                parentTask.Status = EnumMaintenanceRequestTaskStatus.DONE.ToString();
                await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(parentTask, false);
                
                // Check if all level 1 tasks in the maintenance request are DONE
                var maintenanceRequestId = maintenanceRequestTask.MaintenanceRequestId;
                var otherLevel1Tasks = _unitOfWork.Repository<MaintenanceRequestTask>()
                    .Get(
                        filter: mrt => mrt.MaintenanceRequestId == maintenanceRequestId 
                            && mrt.ParentId == null,
                        includeProperties: ""
                    );
                    
                var allLevel1TasksDone = otherLevel1Tasks.All(task => 
                    task.Status == EnumMaintenanceRequestTaskStatus.DONE.ToString());
                
                if (allLevel1TasksDone)
                {
                    // Update maintenance request status to DONE
                    var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
                        .FindAsync(maintenanceRequestId);
                        
                    if (maintenanceRequest != null && maintenanceRequest.Status == EnumMaintenanceRequestTaskStatus.PROCESSING.ToString())
                    {
                        maintenanceRequest.Status = EnumMaintenanceRequestTaskStatus.DONE.ToString();
                        await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
                    }
                }
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<GetAllMaintenanceRequestTaskResponse> GetMaintenanceTaskAsync(Guid id)
    {
        // Find the maintenance request task by ID
        var maintenanceRequestTask = _unitOfWork.Repository<MaintenanceRequestTask>()
            .Get(
                filter: mrt => mrt.Id == id,
                includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User"
            )
            .FirstOrDefault();
            
        if (maintenanceRequestTask == null)
        {
            throw new NotFoundException($"Không tìm thấy công việc bảo trì với ID {id}");
        }
        
        // Map the entity to DTO
        var taskResponse = _mapper.Map<GetAllMaintenanceRequestTaskResponse>(maintenanceRequestTask);
        
        // If this is a parent task, get all its child tasks
        if (maintenanceRequestTask.ParentId == null)
        {
            var childTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
                .Get(
                    filter: mrt => mrt.ParentId == id,
                    includeProperties: "MaintenanceItem,Staff,Staff.User"
                );
                
            taskResponse.Childs = _mapper.Map<IEnumerable<GetMaintenanceRequestTaskChildResponse>>(childTasks);
        }
        
        return taskResponse;
    }
    
    public async Task<(IEnumerable<GetAllMaintenanceRequestTaskResponse> data, int total)> GetAllMaintenanceRequestTasksAsync(GetAllMaintenanceRequestTaskFilterRequest request, Guid? userId = null)
    {
        // If userId is provided, determine user role and prepare additional filter
        if (userId.HasValue)
        {
            // First try to find the user as a customer
            var customer = await _unitOfWork.Repository<Customer>()
                .SingleOrDefaultAsync(c => c.UserId == userId.Value);
                
            if (customer != null)
            {
                // User is a customer, filter tasks by customer's maintenance requests
                var baseFilter = request.GetExpressions();
                
                // Get all maintenance requests for this customer
                var customerRequests = _unitOfWork.Repository<MaintenanceRequest>()
                    .Get(filter: mr => mr.CustomerId == customer.Id)
                    .Select(mr => mr.Id)
                    .ToList();
                
                // Create a predicate for tasks related to customer's maintenance requests
                var customerFilter = PredicateBuilder.New<MaintenanceRequestTask>();
                customerFilter = customerFilter.And(mrt => customerRequests.Contains(mrt.MaintenanceRequestId));
                
                // Combine the base filter with the customer filter
                var combinedFilter = PredicateBuilder.New<MaintenanceRequestTask>()
                    .And(baseFilter)
                    .And(customerFilter);
                
                // Use GetWithCount with the combined filter
                var (tasks, total) = _unitOfWork.Repository<MaintenanceRequestTask>().GetWithCount(
                    filter: combinedFilter,
                    includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User",
                    orderBy: request.GetOrder(),
                    pageIndex: request.PageNumber,
                    pageSize: request.PageSize
                );
                
                var taskResponses = _mapper.Map<IEnumerable<GetAllMaintenanceRequestTaskResponse>>(tasks);
                
                // For each parent task, get its child tasks
                foreach (var taskResponse in taskResponses.Where(t => !tasks.Any(task => task.ParentId == t.Id)))
                {
                    var childTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
                        .Get(
                            filter: mrt => mrt.ParentId == taskResponse.Id,
                            includeProperties: "MaintenanceItem,Staff,Staff.User"
                        );
                    taskResponse.Childs = _mapper.Map<IEnumerable<GetMaintenanceRequestTaskChildResponse>>(childTasks);
                }
                
                return (taskResponses, total);
            }
            else
            {
                // Try to find the user as a staff member
                var staff = await _unitOfWork.Repository<Staff>()
                    .SingleOrDefaultAsync(s => s.UserId == userId.Value);
                    
                if (staff != null && staff.Position == RoleEnum.CONSTRUCTOR.ToString())
                {
                    // User is a constructor staff, filter tasks assigned to this staff
                    var baseFilter = request.GetExpressions();
                    
                    // Create a predicate for tasks assigned to this staff
                    var staffFilter = PredicateBuilder.New<MaintenanceRequestTask>();
                    staffFilter = staffFilter.And(mrt => mrt.StaffId == staff.Id);
                    
                    // Combine the base filter with the staff filter
                    var combinedFilter = PredicateBuilder.New<MaintenanceRequestTask>()
                        .And(baseFilter)
                        .And(staffFilter);
                    
                    // Use GetWithCount with the staff filter
                    var (tasks, total) = _unitOfWork.Repository<MaintenanceRequestTask>().GetWithCount(
                        filter: combinedFilter,
                        includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User",
                        orderBy: request.GetOrder(),
                        pageIndex: request.PageNumber,
                        pageSize: request.PageSize
                    );
                    
                    var taskResponses = _mapper.Map<IEnumerable<GetAllMaintenanceRequestTaskResponse>>(tasks);
                    
                    // For each parent task, get its child tasks
                    foreach (var taskResponse in taskResponses.Where(t => !tasks.Any(task => task.ParentId == t.Id)))
                    {
                        var childTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
                            .Get(
                                filter: mrt => mrt.ParentId == taskResponse.Id,
                                includeProperties: "MaintenanceItem,Staff,Staff.User"
                            );
                        taskResponse.Childs = _mapper.Map<IEnumerable<GetMaintenanceRequestTaskChildResponse>>(childTasks);
                    }
                    
                    return (taskResponses, total);
                }
            }
        }
        
        // No user ID provided or user is neither customer nor constructor staff
        // Use the default filter from the request
        var baseFilterExpression = request.GetExpressions();
        
        var (taskList, count) = _unitOfWork.Repository<MaintenanceRequestTask>().GetWithCount(
            filter: baseFilterExpression,
            includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );
        
        var responseList = _mapper.Map<IEnumerable<GetAllMaintenanceRequestTaskResponse>>(taskList);
        
        // For each parent task, get its child tasks
        foreach (var taskResponse in responseList.Where(t => !taskList.Any(task => task.ParentId == t.Id)))
        {
            var childTasks = _unitOfWork.Repository<MaintenanceRequestTask>()
                .Get(
                    filter: mrt => mrt.ParentId == taskResponse.Id,
                    includeProperties: "MaintenanceItem,Staff,Staff.User"
                );
            taskResponse.Childs = _mapper.Map<IEnumerable<GetMaintenanceRequestTaskChildResponse>>(childTasks);
        }
        
        return (responseList, count);
    }
    
    public async Task<(IEnumerable<GetAllStaffResponse> data, int total)> GetStaffsAsync(
        GetAllStaffRequest request, 
        Guid maintenanceRequestId)
    {
        // Check if maintenance request exists
        var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
            .FindAsync(maintenanceRequestId);
            
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"Không tìm thấy yêu cầu bảo trì với ID {maintenanceRequestId}");
        }
        
        // Get the maintenance staff assigned to this maintenance request
        var maintenanceStaffs = _unitOfWork.Repository<MaintenanceStaff>()
            .Get(filter: ms => ms.MaintenanceRequestId == maintenanceRequestId,
                 includeProperties: "Staff,Staff.User");
        
        // Extract staff IDs from maintenance staff records
        var staffIds = maintenanceStaffs.Select(ms => ms.StaffId).ToList();
        
        // Prepare search expression for Staff entity
        var staffExpression = PredicateBuilder.New<Staff>();
        
        // Filter staff with CONSTRUCTOR position that are assigned to this maintenance request
        staffExpression = staffExpression.And(s => s.Position == RoleEnum.CONSTRUCTOR.ToString());
        staffExpression = staffExpression.And(s => staffIds.Contains(s.Id));
        
        // Apply additional filters from request
        var baseFilterExpression = request.GetExpressions();
        
        // Combine our filter with the request's filter
        var combinedFilter = PredicateBuilder.New<Staff>()
            .And(staffExpression)
            .And(baseFilterExpression);
        
        // Get the staff with pagination
        var (staffList, total) = _unitOfWork.Repository<Staff>().GetWithCount(
            filter: combinedFilter,
            includeProperties: "User",
            orderBy: request.GetOrder(),
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );
        
        // Use the mapper to map Staff entities to GetAllStaffResponse DTOs
        var responseList = _mapper.Map<IEnumerable<GetAllStaffResponse>>(staffList);
        
        return (responseList, total);
    }
    
    public async Task AssignStaffsAsync(Guid maintenanceRequestId, CommandMaintenanceRequestTaskRequest request)
    {
        // Validate maintenance request exists
        var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
            .FindAsync(maintenanceRequestId);
            
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"Không tìm thấy yêu cầu bảo trì với ID {maintenanceRequestId}");
        }
        
        // Ensure staff IDs are provided
        if (request.StaffIds == null || !request.StaffIds.Any())
        {
            throw new BadRequestException("Danh sách ID nhân viên không được để trống");
        }
        
        // Validate that all staff exist and have CONSTRUCTOR position
        foreach (var staffId in request.StaffIds)
        {
            var staff = await _unitOfWork.Repository<Staff>()
                .SingleOrDefaultAsync(s => s.UserId == staffId);
                
            if (staff == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên với ID người dùng {staffId}");
            }
            
            // Validate staff position is CONSTRUCTOR
            if (staff.Position != RoleEnum.CONSTRUCTOR.ToString())
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} không có chức vụ CONSTRUCTOR");
            }
            
            // Check if staff is assigned to any construction tasks that are not DONE
            var hasActiveConstructionTasks = await _unitOfWork.Repository<ConstructionTask>()
                .Where(ct => ct.StaffId == staff.Id && ct.Status != "DONE")
                .FirstOrDefaultAsync() != null;
                
            if (hasActiveConstructionTasks)
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc xây dựng đang hoạt động");
            }
            
            // Check if staff is involved in any projects that are in CONSTRUCTING status
            var hasConstructingProjects = await _unitOfWork.Repository<ProjectStaff>()
                .Where(ps => ps.StaffId == staff.Id && ps.Project.Status == EnumProjectStatus.CONSTRUCTING.ToString())
                .FirstOrDefaultAsync() != null;
                
            if (hasConstructingProjects)
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} đang tham gia các dự án đang trong giai đoạn thi công");
            }
            
            // Check if staff is assigned to any project issues that are not DONE
            var hasActiveProjectIssues = await _unitOfWork.Repository<ProjectIssue>()
                .Where(pi => pi.StaffId == staff.Id && pi.Status != "DONE")
                .FirstOrDefaultAsync() != null;
                
            if (hasActiveProjectIssues)
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các vấn đề dự án đang hoạt động");
            }
            
            // Check if staff is assigned to any Level 2 maintenance tasks that belong to Level 1 tasks that are not DONE
            var hasActiveMaintLv2Tasks = await _unitOfWork.Repository<MaintenanceRequestTask>()
                .Where(mrt => mrt.StaffId == staff.Id 
                    && mrt.ParentId != null 
                    && mrt.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString())
                .FirstOrDefaultAsync() != null;
                
            if (hasActiveMaintLv2Tasks)
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc bảo trì cấp 2 đang hoạt động");
            }
            
            // Check if staff is assigned to any maintenance requests (through MaintenanceStaff) that are not DONE
            var hasActiveMaintenanceRequests = await _unitOfWork.Repository<MaintenanceStaff>()
                .Where(ms => ms.StaffId == staff.Id 
                    && ms.MaintenanceRequest.Status != EnumMaintanceRequestStatus.DONE.ToString())
                .FirstOrDefaultAsync() != null;
                
            if (hasActiveMaintenanceRequests)
            {
                throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các yêu cầu bảo trì đang hoạt động");
            }
        }
        
        // Clear existing maintenance staff associations for this request
        var existingMaintenanceStaffs = _unitOfWork.Repository<MaintenanceStaff>()
            .Get(filter: ms => ms.MaintenanceRequestId == maintenanceRequestId)
            .ToList();
            
        _unitOfWork.Repository<MaintenanceStaff>().RemoveRange(existingMaintenanceStaffs);
        
        // Create new maintenance staff records
        foreach (var staffId in request.StaffIds)
        {
            var staff = await _unitOfWork.Repository<Staff>()
                .SingleOrDefaultAsync(s => s.UserId == staffId);
            
            var maintenanceStaff = new MaintenanceStaff
            {
                MaintenanceRequestId = maintenanceRequestId,
                StaffId = staff.Id
            };
            
            await _unitOfWork.Repository<MaintenanceStaff>().AddAsync(maintenanceStaff, false);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<GetAllMaintenanceRequestResponse> GetDetailMaintenanceRequestAsync(Guid id)
    {
        // Get the maintenance request with all related data
        var maintenanceRequest = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: mr => mr.Id == id,
                includeProperties: "MaintenancePackage,Customer,Customer.User,MaintenanceRequestTasks,MaintenanceRequestTasks.Staff,MaintenanceRequestTasks.Staff.User,MaintenanceRequestTasks.MaintenanceItem"
            )
            .FirstOrDefault();

        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"Không tìm thấy yêu cầu bảo trì với ID {id}");
        }

        // Get feedbacks for this maintenance request
        var feedbacks = _unitOfWork.Repository<Feedback>()
            .Get(
                filter: f => f.Type == EnumFeedbackType.MAINTENANCE.ToString() && f.No == id,
                includeProperties: "Customer,Customer.User"
            );

        // Map the maintenance request to response DTO
        var response = _mapper.Map<GetAllMaintenanceRequestResponse>(maintenanceRequest);

        // Add feedbacks to the response
        response.Feedbacks = _mapper.Map<IEnumerable<GetAllFeedbackResponse>>(feedbacks);

        // Get level 1 tasks (parent tasks)
        var level1Tasks = maintenanceRequest.MaintenanceRequestTasks
            .Where(t => t.ParentId == null)
            .ToList();

        // Map level 1 tasks
        var mappedLevel1Tasks = _mapper.Map<List<GetMaintenanceRequestTaskForMaintenanceRequestResponse>>(level1Tasks);

        // For each level 1 task, get and map its level 2 tasks (children)
        foreach (var level1Task in mappedLevel1Tasks)
        {
            var level2Tasks = maintenanceRequest.MaintenanceRequestTasks
                .Where(t => t.ParentId == level1Task.Id)
                .ToList();

            level1Task.Childs = _mapper.Map<IEnumerable<GetMaintenanceRequestTaskChildResponse>>(level2Tasks);
        }

        // Set the maintenance request tasks to only include level 1 tasks with their children
        response.MaintenanceRequestTasks = mappedLevel1Tasks;

        return response;
    }
}
