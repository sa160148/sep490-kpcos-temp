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
            throw new BadRequestException("Gói bảo trì không được để trống");
        }
        
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new BadRequestException("Tên yêu cầu bảo trì không được để trống");
        }
        
        if (request.Area == null || request.Depth == null)
        {
            throw new BadRequestException("Diện tích và độ sâu hồ cá không được để trống");
        }
        
        if (string.IsNullOrEmpty(request.Address))
        {
            throw new BadRequestException("Địa chỉ thực hiện bảo trì không được để trống");
        }
        
        if (request.Type == null)
        {
            throw new BadRequestException("Loại bảo trì không được để trống");
        }
        
        // Set duration based on request and validate if needed
        int duration = request.Duration.HasValue && request.Duration.Value > 0 
            ? request.Duration.Value 
            : 1;
            
        // Additional validation for UNSCHEDULED type
        if (request.Type.Equals(EnumMaintenanceRequestType.UNSCHEDULED.ToString(), StringComparison.OrdinalIgnoreCase) 
            && duration > 2)
        {
            throw new BadRequestException("Bảo trì không định kỳ (UNSCHEDULED) không thể có quá 2 lần bảo trì");
        }
        
        // Verify maintenance package exists
        var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>()
            .FindAsync(request.MaintenancePackageId.Value);
        
        if (maintenancePackage == null)
        {
            throw new NotFoundException($"Không tìm thấy gói bảo trì với ID {request.MaintenancePackageId}");
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
            throw new NotFoundException($"Gói bảo trì với ID {request.MaintenancePackageId} không có hạng mục bảo trì nào đang hoạt động");
        }

        // Calculate the total value
        int totalValue;
        bool isPostProjectMaintenance = false;

        if (request.TotalValue.HasValue)
        {
            if (request.TotalValue.Value == 0)
            {
                // This is a post-project maintenance request
                isPostProjectMaintenance = true;
                totalValue = 0;
            }
            else
            {
                // Use the specified total value if provided
                totalValue = request.TotalValue.Value;
            }
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
            throw new NotFoundException($"Không tìm thấy khách hàng với ID {customerId}");
        }

        // Create new maintenance request
        var maintenanceRequest = new MaintenanceRequest
        {
            Id = Guid.NewGuid(),
            Name = isPostProjectMaintenance 
                ? $"{request.Name} - bảo trì/bảo dưỡng sau dự án"
                : request.Name,
            Area = request.Area ?? 0,
            Depth = request.Depth ?? 0,
            Address = request.Address,
            TotalValue = totalValue,
            Type = request.Type,
            IsPaid = isPostProjectMaintenance,
            CustomerId = customer.Id,
            MaintenancePackageId = request.MaintenancePackageId.Value,
            Status = isPostProjectMaintenance 
                ? EnumMaintenanceRequestStatus.REQUESTING.ToString()
                : EnumMaintenanceRequestStatus.OPENING.ToString()
        };
        
        // Add the maintenance request
        await _unitOfWork.Repository<MaintenanceRequest>().AddAsync(maintenanceRequest, false);
        
        // Create maintenance request tasks if estimate date is provided
        if (request.EstimateAt.HasValue)
        {
            // Get HttpClient to fetch holidays
            var httpClient = _httpClientFactory.CreateClient();
            
            // Calculate maintenance dates (avoiding weekends and holidays)
            // Using the updated utility function that respects milestone days and adds end-of-month dates
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
            includeProperties: "MaintenancePackage,Customer,Customer.User,MaintenanceRequestTasks,MaintenanceRequestTasks.Staff,MaintenanceRequestTasks.Staff.User,MaintenanceRequestTasks.MaintenanceStaffs,MaintenanceRequestTasks.MaintenanceStaffs.Staff,MaintenanceRequestTasks.MaintenanceStaffs.Staff.User",
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
        // Get task by ID with related entities
        var maintenanceRequestTask = await GetMaintenanceRequestTaskById(id);
        
        // Validate task exists
        if (maintenanceRequestTask == null)
        {
            throw new NotFoundException($"Không tìm thấy công việc bảo trì với ID {id}");
        }
        
        // Track if we need to save changes at the end
        bool needToSaveChanges = false;
        
        // If name is provided, update it
        if (!string.IsNullOrEmpty(request.Name))
        {
            maintenanceRequestTask.Name = request.Name;
            needToSaveChanges = true;
        }
        
        // If description is provided, update it
        if (!string.IsNullOrEmpty(request.Description))
        {
            maintenanceRequestTask.Description = request.Description;
            needToSaveChanges = true;
        }
        
        // Mode 1: Staff assignment
        if (request.StaffId.HasValue)
        {
            // If this is a Level 1 task (ParentId is null), use MaintenanceStaff
            if (maintenanceRequestTask.ParentId == null)
            {
                await UpdateLevel1StaffAssignmentAsync(maintenanceRequestTask, request.StaffId.Value);
                // Changes are already saved by UpdateLevel1StaffAssignmentAsync
                needToSaveChanges = false;
            }
            // If this is a Level 2 task (has ParentId), assign directly
            else
            {
                await UpdateLevel2StaffAssignmentAsync(maintenanceRequestTask, request.StaffId.Value);
                // Changes are already saved by UpdateLevel2StaffAssignmentAsync
                needToSaveChanges = false;
            }
        }
        // Mode 2: Image upload
        else if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            // Task must be in PROCESSING status
            if (maintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.PROCESSING.ToString())
            {
                throw new BadRequestException("Chỉ có thể cập nhật hình ảnh cho công việc bảo trì đang trong trạng thái PROCESSING");
            }
            
            // Update image URL and status
            maintenanceRequestTask.ImageUrl = request.ImageUrl;
            maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PREVIEWING.ToString();
            await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask, false);
            needToSaveChanges = true;
        }
        // Mode 3: Update reason (requires task to be in PREVIEWING status)
        else if (!string.IsNullOrEmpty(request.Reason))
        {
            // Task must be in PREVIEWING status
            if (maintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.PREVIEWING.ToString())
            {
                throw new BadRequestException("Chỉ có thể cập nhật lý do cho công việc bảo trì đang trong trạng thái PREVIEWING");
            }
            
            // Update reason and revert to PROCESSING status
            maintenanceRequestTask.Reason = request.Reason;
            maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
            await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask, false);
            needToSaveChanges = true;
        }
        
        // Save all changes in a single transaction if needed
        if (needToSaveChanges)
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task UpdateLevel1StaffAssignmentAsync(MaintenanceRequestTask level1Task, Guid staffId)
    {
        // Get the staff by ID
        var staff = await _unitOfWork.Repository<Staff>()
            .SingleOrDefaultAsync(x => x.UserId == staffId);
            
        if (staff == null)
        {
            throw new NotFoundException($"Không tìm thấy nhân viên với ID người dùng {staffId}");
        }
        
        // Validate that this is a Level 1 task (no parent)
        if (level1Task.ParentId != null)
        {
            throw new BadRequestException("Nhân viên chỉ có thể được phân công trực tiếp cho công việc bảo trì cấp 1");
        }
        
        // Validate staff position is CONSTRUCTOR
        if (staff.Position != RoleEnum.CONSTRUCTOR.ToString())
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} không có chức vụ CONSTRUCTOR");
        }
        
        // Check if staff is assigned to any maintenance tasks that are not DONE
        var isStaffAssignedToOtherTask = await _unitOfWork.Repository<MaintenanceStaff>()
            .Where(ms => 
                ms.StaffId == staff.Id && 
                ms.MaintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString() &&
                ms.MaintenanceRequestTask.MaintenanceRequestId != level1Task.MaintenanceRequestId)
            .AnyAsync();
            
        if (isStaffAssignedToOtherTask)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc bảo trì khác đang hoạt động");
        }
        
        // Check for valid assignment to level 1 tasks from other maintenance requests
        var allStaffAssignments = _unitOfWork.Repository<MaintenanceStaff>()
            .Get(
                filter: ms => ms.StaffId == staff.Id,
                includeProperties: "MaintenanceRequestTask"
            )
            .ToList();
            
        // Check if staff is already assigned to active level 1 tasks from other maintenance requests
        var activeTasksFromOtherRequests = allStaffAssignments
            .Where(ms => 
                ms.MaintenanceRequestTask.ParentId == null && // Level 1 task
                ms.MaintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString() && // Not DONE
                ms.MaintenanceRequestTask.MaintenanceRequestId != level1Task.MaintenanceRequestId // Different maintenance request
            )
            .ToList();
            
        // If staff is already assigned to other level 1 tasks, throw an error with details
        if (activeTasksFromOtherRequests.Any())
        {
            // Create detailed error message with the specific conflicting tasks
            var conflictingTasksInfo = activeTasksFromOtherRequests
                .Select(ms => $"{ms.MaintenanceRequestTaskId} (Request: {ms.MaintenanceRequestTask.MaintenanceRequestId}, Status: {ms.MaintenanceRequestTask.Status})")
                .ToList();
                
            var conflictingTasksMessage = string.Join(", ", conflictingTasksInfo);
            
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc bảo trì cấp 1 từ một yêu cầu bảo trì khác. Task IDs: {conflictingTasksMessage}");
        }
        
        // Check if staff is assigned to any projects in CONSTRUCTING status
        var hasConstructingProjects = await _unitOfWork.Repository<ProjectStaff>()
            .Where(ps => ps.StaffId == staff.Id && ps.Project.Status == EnumProjectStatus.CONSTRUCTING.ToString())
            .AnyAsync();
                
        if (hasConstructingProjects)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đang tham gia các dự án đang trong giai đoạn thi công (CONSTRUCTING). Nhân viên chỉ có thể được phân công khi không còn tham gia dự án đang thi công.");
        }
        
        // Check if staff is assigned to any construction tasks that are not DONE
        var hasActiveConstructionTasks = await _unitOfWork.Repository<ConstructionTask>()
            .Where(ct => ct.StaffId == staff.Id && ct.Status != EnumConstructionTaskStatus.DONE.ToString())
            .AnyAsync();
            
        if (hasActiveConstructionTasks)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc xây dựng đang hoạt động");
        }
        
        // Check if staff is assigned to any project issues that are not DONE
        var hasActiveProjectIssues = await _unitOfWork.Repository<ProjectIssue>()
            .Where(pi => pi.StaffId == staff.Id && pi.Status != EnumProjectIssueStatus.DONE.ToString())
            .AnyAsync();
            
        if (hasActiveProjectIssues)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các vấn đề dự án đang hoạt động");
        }
        
        // Check if the staff is already assigned to this task
        var existingAssignment = await _unitOfWork.Repository<MaintenanceStaff>()
            .Where(ms => ms.StaffId == staff.Id && ms.MaintenanceRequestTaskId == level1Task.Id)
            .FirstOrDefaultAsync();
        
        // If already assigned, nothing to do
        if (existingAssignment != null)
        {
            return;
        }
        
        // Create new maintenance staff assignment
        var maintenanceStaff = new MaintenanceStaff
        {
            MaintenanceRequestTaskId = level1Task.Id,
            StaffId = staff.Id
        };
        
        await _unitOfWork.Repository<MaintenanceStaff>().AddAsync(maintenanceStaff, false);
        
        // Update task status to PROCESSING if it was OPENING
        if (level1Task.Status == EnumMaintenanceRequestTaskStatus.OPENING.ToString())
        {
            level1Task.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
            await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(level1Task, false);
        }
        
        // Get the maintenance request
        var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
            .FindAsync(level1Task.MaintenanceRequestId);
        if(maintenanceRequest.Status == EnumMaintenanceRequestStatus.OPENING.ToString())
        {
            throw new BadRequestException("Yêu cầu bảo trì đang ở trạng thái OPENING. Không thể phân công nhân viên cho công việc bảo trì cấp 1.");
        }
            
        if (maintenanceRequest != null && maintenanceRequest.Status == EnumMaintenanceRequestStatus.REQUESTING.ToString())
        {
            // Update maintenance request status to PROCESSING
            maintenanceRequest.Status = EnumMaintenanceRequestStatus.PROCESSING.ToString();
            await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    private async Task UpdateLevel2StaffAssignmentAsync(MaintenanceRequestTask maintenanceRequestTask, Guid staffId)
    {
        // Ensure this is a child task
        if (maintenanceRequestTask.ParentId == null)
        {
            throw new BadRequestException("Phương thức này chỉ áp dụng cho công việc bảo trì cấp 2 (có ParentId)");
        }
        
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
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} không có chức vụ CONSTRUCTOR");
        }
        
        // CRITICAL VALIDATION: Make sure the staff is assigned to the parent level 1 task
        var parentId = maintenanceRequestTask.ParentId.Value;
        var isStaffAssignedToParent = await _unitOfWork.Repository<MaintenanceStaff>()
            .Where(ms => ms.StaffId == staff.Id && ms.MaintenanceRequestTaskId == parentId)
            .AnyAsync();
            
        if (!isStaffAssignedToParent)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} phải được phân công cho công việc bảo trì cấp 1 (công việc cha) trước khi được phân công cho công việc cấp 2 (công việc con)");
        }
        
        // Update the task with the assigned staff ID
        maintenanceRequestTask.StaffId = staff.Id;
        
        // Update the task status to PROCESSING
        if (maintenanceRequestTask.Status == EnumMaintenanceRequestTaskStatus.OPENING.ToString())
        {
            maintenanceRequestTask.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
        }
        
        // Update the task in the database (but don't save changes yet)
        await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(maintenanceRequestTask, false);
        
        // Save all changes to the database
        await _unitOfWork.SaveChangesAsync();
    }
    
    private async Task UpdateStaffAssignmentAsync(MaintenanceRequestTask maintenanceRequestTask, Guid staffId)
    {
        // This method is kept for backward compatibility but we'll delegate to the new methods
        if (maintenanceRequestTask.ParentId == null)
        {
            await UpdateLevel1StaffAssignmentAsync(maintenanceRequestTask, staffId);
        }
        else
        {
            await UpdateLevel2StaffAssignmentAsync(maintenanceRequestTask, staffId);
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
                        
                    if (maintenanceRequest != null && maintenanceRequest.Status == EnumMaintenanceRequestStatus.PROCESSING.ToString())
                    {
                        maintenanceRequest.Status = EnumMaintenanceRequestStatus.DONE.ToString();
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
                includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User,MaintenanceStaffs,MaintenanceStaffs.Staff,MaintenanceStaffs.Staff.User"
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
                    includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User,MaintenanceStaffs,MaintenanceStaffs.Staff,MaintenanceStaffs.Staff.User",
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
                        includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User,MaintenanceStaffs,MaintenanceStaffs.Staff,MaintenanceStaffs.Staff.User",
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
            includeProperties: "MaintenanceRequest,MaintenanceItem,Staff,Staff.User,MaintenanceRequest.Customer,MaintenanceRequest.Customer.User,MaintenanceStaffs,MaintenanceStaffs.Staff,MaintenanceStaffs.Staff.User",
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
        
        // Get all level 1 maintenance tasks for this request
        var level1TaskIds = _unitOfWork.Repository<MaintenanceRequestTask>()
            .Get(filter: mrt => mrt.MaintenanceRequestId == maintenanceRequestId && mrt.ParentId == null)
            .Select(task => task.Id)
            .ToList();
            
        if (!level1TaskIds.Any())
        {
            return (Enumerable.Empty<GetAllStaffResponse>(), 0);
        }
        
        // Get the maintenance staff assigned to these level 1 tasks
        var maintenanceStaffs = _unitOfWork.Repository<MaintenanceStaff>()
            .Get(filter: ms => level1TaskIds.Contains(ms.MaintenanceRequestTaskId),
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
        if (!Guid.TryParse(request?.StaffId?.ToString(), out Guid staffId) || staffId == Guid.Empty)
        {
            throw new BadRequestException("ID nhân viên không hợp lệ");
        }
        
        // Get the maintenance request
        var maintenanceRequest = await _unitOfWork.Repository<MaintenanceRequest>()
            .FindAsync(maintenanceRequestId);
            
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"Không tìm thấy yêu cầu bảo trì với ID {maintenanceRequestId}");
        }
        
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
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} không có chức vụ CONSTRUCTOR");
        }
        
        // Get all level 1 tasks for this maintenance request
        var level1Tasks = await _unitOfWork.Repository<MaintenanceRequestTask>()
            .Where(t => t.MaintenanceRequestId == maintenanceRequestId && t.ParentId == null)
            .ToListAsync();
            
        if (!level1Tasks.Any())
        {
            throw new BadRequestException($"Không tìm thấy công việc bảo trì cấp 1 cho yêu cầu bảo trì có ID {maintenanceRequestId}");
        }
        
        // Check if staff is assigned to any maintenance tasks that are not DONE
        var isStaffAssignedToOtherTask = await _unitOfWork.Repository<MaintenanceStaff>()
            .Where(ms => 
                ms.StaffId == staff.Id && 
                ms.MaintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString() &&
                ms.MaintenanceRequestTask.MaintenanceRequestId != maintenanceRequestId)
            .AnyAsync();
            
        if (isStaffAssignedToOtherTask)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc bảo trì khác đang hoạt động");
        }
        
        // Check for valid assignment to level 1 tasks from other maintenance requests
        var allStaffAssignments = _unitOfWork.Repository<MaintenanceStaff>()
            .Get(
                filter: ms => ms.StaffId == staff.Id,
                includeProperties: "MaintenanceRequestTask"
            )
            .ToList();
            
        // Check if staff is already assigned to active level 1 tasks from other maintenance requests
        var activeTasksFromOtherRequests = allStaffAssignments
            .Where(ms => 
                ms.MaintenanceRequestTask?.ParentId == null && // Level 1 task
                ms.MaintenanceRequestTask?.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString() && // Not DONE
                ms.MaintenanceRequestTask?.MaintenanceRequestId != maintenanceRequestId // Different maintenance request
            )
            .ToList();
            
        // If staff is already assigned to other level 1 tasks, throw an error with details
        if (activeTasksFromOtherRequests.Any())
        {
            // Create detailed error message with the specific conflicting tasks
            var conflictingTasksInfo = activeTasksFromOtherRequests
                .Select(ms => $"{ms.MaintenanceRequestTaskId} (Request: {ms.MaintenanceRequestTask?.MaintenanceRequestId}, Status: {ms.MaintenanceRequestTask?.Status})")
                .ToList();
                
            var conflictingTasksMessage = string.Join(", ", conflictingTasksInfo);
            
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc bảo trì cấp 1 từ một yêu cầu bảo trì khác. Task IDs: {conflictingTasksMessage}");
        }
        
        // Check if staff is assigned to any projects in CONSTRUCTING status
        var hasConstructingProjects = await _unitOfWork.Repository<ProjectStaff>()
            .Where(ps => ps.StaffId == staff.Id && ps.Project.Status == EnumProjectStatus.CONSTRUCTING.ToString())
            .AnyAsync();
                
        if (hasConstructingProjects)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đang tham gia các dự án đang trong giai đoạn thi công (CONSTRUCTING). Nhân viên chỉ có thể được phân công khi không còn tham gia dự án đang thi công.");
        }
        
        // Check if staff is assigned to any construction tasks that are not DONE
        var hasActiveConstructionTasks = await _unitOfWork.Repository<ConstructionTask>()
            .Where(ct => ct.StaffId == staff.Id && ct.Status != EnumConstructionTaskStatus.DONE.ToString())
            .AnyAsync();
            
        if (hasActiveConstructionTasks)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các công việc xây dựng đang hoạt động");
        }
        
        // Check if staff is assigned to any project issues that are not DONE
        var hasActiveProjectIssues = await _unitOfWork.Repository<ProjectIssue>()
            .Where(pi => pi.StaffId == staff.Id && pi.Status != EnumProjectIssueStatus.DONE.ToString())
            .AnyAsync();
            
        if (hasActiveProjectIssues)
        {
            throw new BadRequestException($"Nhân viên {staff.User?.FullName} đã được phân công cho các vấn đề dự án đang hoạt động");
        }

        // Assign staff to each level 1 task
        foreach (var task in level1Tasks)
        {
            // Check if the staff is already assigned to this task
            var existingAssignment = await _unitOfWork.Repository<MaintenanceStaff>()
                .Where(ms => ms.StaffId == staff.Id && ms.MaintenanceRequestTaskId == task.Id)
                .FirstOrDefaultAsync();
            
            // If already assigned, nothing to do
            if (existingAssignment != null)
            {
                continue;
            }
            
            // Create new maintenance staff assignment
            var maintenanceStaff = new MaintenanceStaff
            {
                MaintenanceRequestTaskId = task.Id,
                StaffId = staff.Id
            };
            
            await _unitOfWork.Repository<MaintenanceStaff>().AddAsync(maintenanceStaff, false);
            
            // Update task status to PROCESSING if it was OPENING
            if (task.Status == EnumMaintenanceRequestTaskStatus.OPENING.ToString())
            {
                task.Status = EnumMaintenanceRequestTaskStatus.PROCESSING.ToString();
                await _unitOfWork.Repository<MaintenanceRequestTask>().UpdateAsync(task, false);
            }
        }
        
        // Update maintenance request status if it was still OPENING
        if (maintenanceRequest.Status == EnumMaintenanceRequestStatus.OPENING.ToString())
        {
            maintenanceRequest.Status = EnumMaintenanceRequestStatus.PROCESSING.ToString();
            await _unitOfWork.Repository<MaintenanceRequest>().UpdateAsync(maintenanceRequest, false);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<GetAllMaintenanceRequestResponse> GetDetailMaintenanceRequestAsync(Guid id)
    {
        // Get the maintenance request with all related data
        var maintenanceRequest = _unitOfWork.Repository<MaintenanceRequest>()
            .Get(
                filter: mr => mr.Id == id,
                includeProperties: "MaintenancePackage,Customer,Customer.User,MaintenanceRequestTasks,MaintenanceRequestTasks.Staff,MaintenanceRequestTasks.Staff.User,MaintenanceRequestTasks.MaintenanceItem,MaintenanceRequestTasks.MaintenanceStaffs,MaintenanceRequestTasks.MaintenanceStaffs.Staff,MaintenanceRequestTasks.MaintenanceStaffs.Staff.User"
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

    public async Task<GetAllMaintenancePackageResponse> GetDetailMaintenancePackageByIdAsync(Guid id)
    {
        var maintenancePackage = _unitOfWork.Repository<MaintenancePackage>()
            .Get(
                filter: mp => mp.Id == id,
                includeProperties: "MaintenancePackageItems,MaintenancePackageItems.MaintenanceItem"
            )
            .SingleOrDefault();

        if (maintenancePackage == null)
        {
            throw new NotFoundException($"Không tìm thấy gói bảo trì với ID {id}");
        }

        return _mapper.Map<GetAllMaintenancePackageResponse>(maintenancePackage);
    }

    public async Task DeleteMaintenancePackageItemAsync(Guid maintenancePackageId, Guid maintenanceItemId)
    {
        var maintenancePackageItem = await _unitOfWork.Repository<MaintenancePackageItem>()
            .FirstOrDefaultAsync(x => x.MaintenancePackageId == maintenancePackageId && 
                                    x.MaintenanceItemId == maintenanceItemId);

        if (maintenancePackageItem == null)
        {
            throw new NotFoundException($"Không tìm thấy mục bảo trì {maintenanceItemId} trong gói bảo trì {maintenancePackageId}");
        }

        await _unitOfWork.Repository<MaintenancePackageItem>().RemoveAsync(maintenancePackageItem);
    }

    public async Task UpdateMaintenancePackageAsync(Guid id, CommandMaintenancePackageRequest request)
    {
        var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>()
            .FindAsync(id);

        if (maintenancePackage == null)
        {
            throw new NotFoundException($"Không tìm thấy gói bảo trì với ID {id}");
        }

        // Use ReflectionUtil to update properties
        ReflectionUtil.UpdateProperties(request, maintenancePackage, new List<string> { "MaintenanceItems" });
        maintenancePackage.UpdatedAt = DateTime.UtcNow;

        // Handle maintenance items if provided
        if (request.MaintenanceItems != null && request.MaintenanceItems.Any())
        {
            // Get existing package items
            var existingPackageItems = _unitOfWork.Repository<MaintenancePackageItem>()
                .Get(x => x.MaintenancePackageId == id)
                .ToList();

            // Add new items that don't exist yet
            var existingItemIds = existingPackageItems.Select(x => x.MaintenanceItemId).ToList();
            var newItemIds = request.MaintenanceItems.Where(x => !existingItemIds.Contains(x)).ToList();

            foreach (var itemId in newItemIds)
            {
                // Validate that the maintenance item exists
                var maintenanceItem = await _unitOfWork.Repository<MaintenanceItem>().FindAsync(itemId);
                if (maintenanceItem == null)
                {
                    throw new NotFoundException($"Không tìm thấy mục bảo trì với ID {itemId}");
                }

                var newPackageItem = new MaintenancePackageItem
                {
                    Id = Guid.NewGuid(),
                    MaintenancePackageId = id,
                    MaintenanceItemId = itemId
                };

                await _unitOfWork.Repository<MaintenancePackageItem>().AddAsync(newPackageItem, false);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // Helper method to get maintenance request task by ID with related entities
    private async Task<MaintenanceRequestTask?> GetMaintenanceRequestTaskById(Guid id)
    {
        return await _unitOfWork.Repository<MaintenanceRequestTask>()
            .FindAsync(id);
    }
}
