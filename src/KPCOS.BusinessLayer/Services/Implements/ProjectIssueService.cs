using System;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectIssueService : IProjectIssueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProjectIssueService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task CreateProjectIssueAsync(
        Guid constructionItemId, 
        CommandProjectIssueRequest request, 
        Guid userId)
    {
        // Get repositories
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        var constructionItemRepository = _unitOfWork.Repository<ConstructionItem>();
        
        // Validate request
        ValidateProjectIssueRequest(request);
        
        // Validate construction item exists
        var constructionItem = await constructionItemRepository.FindAsync(constructionItemId)
            ?? throw new NotFoundException("Không tìm thấy hạng mục xây dựng với ID đã cung cấp.");

        // Validate that only level 1 construction items (no parent) can be associated with issues
        if (constructionItem.ParentId != null)
        {
            throw new BadRequestException("Chỉ có thể tạo vấn đề cho hạng mục xây dựng cấp 1. Vui lòng chọn hạng mục cha.");
        }

        // Validate issue name is unique for this construction item
        var existingIssue = await projectIssueRepository
            .FirstOrDefaultAsync(pi => pi.Name == request.Name && pi.ConstructionItemId == constructionItem.Id);
        
        if (existingIssue != null)
        {
            throw new BadRequestException($"Vấn đề dự án với tên '{request.Name}' đã tồn tại cho hạng mục xây dựng này.");
        }

        if (constructionItem.Status == EnumConstructionItemStatus.DONE.ToString() || 
            constructionItem.Status == EnumConstructionItemStatus.OPENING.ToString())
        {
            constructionItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
            await constructionItemRepository.UpdateAsync(constructionItem, false);
        }

        // Create new project issue with only the allowed fields
        var projectIssue = new ProjectIssue
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Cause = request.Cause,
            Solution = request.Solution, // Allow setting solution during creation
            IssueImage = request.IssueImage,
            IssueTypeId = request.IssueTypeId!.Value,
            EstimateAt = request.EstimateAt, // Include the estimate date
            Status = EnumProjectIssueStatus.OPENING.ToString(),
            ConstructionItemId = constructionItem.Id,
            // Explicitly set fields that should not be allowed to null
            Reason = null,
            ConfirmImage = null,
            StaffId = null,
            ActualAt = null // Set ActualAt to null initially
        };
        
        // Now save all changes to database
        await projectIssueRepository.AddAsync(projectIssue, false);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task UpdateProjectIssueAsync(Guid id, CommandProjectIssueRequest request)
    {
        // Get repositories
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        var staffRepository = _unitOfWork.Repository<Staff>();
        
        // Find project issue
        var projectIssue = await projectIssueRepository.FindAsync(id)
            ?? throw new NotFoundException("Không tìm thấy vấn đề dự án với ID đã cung cấp.");

        // Save original values to check for changes
        var originalStaffId = projectIssue.StaffId;
        var originalConfirmImage = projectIssue.ConfirmImage;
        var originalReason = projectIssue.Reason;

        // Determine what type of update this is
        bool isStaffOnlyUpdate = 
            request.StaffId != null && 
            request.Name == null && 
            request.Description == null && 
            request.Reason == null && 
            request.Cause == null &&
            request.Solution == null &&
            request.IssueTypeId == null && 
            request.IssueImage == null &&
            request.ConfirmImage == null;
            
        bool isConfirmImageOnlyUpdate = 
            request.ConfirmImage != null && 
            request.Name == null && 
            request.Description == null && 
            request.Reason == null && 
            request.Cause == null &&
            request.Solution == null &&
            request.IssueTypeId == null && 
            request.StaffId == null &&
            request.IssueImage == null;
            
        bool isReasonOnlyUpdate = 
            request.Reason != null && 
            request.Name == null && 
            request.Description == null && 
            request.Cause == null &&
            request.Solution == null &&
            request.IssueTypeId == null && 
            request.StaffId == null &&
            request.IssueImage == null &&
            request.ConfirmImage == null;
            
        bool isNormalUpdate = !isStaffOnlyUpdate && !isConfirmImageOnlyUpdate && !isReasonOnlyUpdate;

        // Validate for normal updates - certain fields cannot be updated in normal mode
        if (isNormalUpdate)
        {
            // For normal updates, validate that restricted fields aren't being modified if they're null in DB
            if (string.IsNullOrEmpty(originalReason) && request.Reason != null)
            {
                throw new BadRequestException("Không thể cập nhật lý do trong cập nhật thông thường. Sử dụng cập nhật riêng lý do.");
            }
            
            if (originalStaffId == null && request.StaffId != null)
            {
                throw new BadRequestException("Không thể gán nhân viên trong cập nhật thông thường. Sử dụng cập nhật riêng nhân viên.");
            }
            
            if (string.IsNullOrEmpty(originalConfirmImage) && request.ConfirmImage != null)
            {
                throw new BadRequestException("Không thể thêm hình ảnh xác nhận trong cập nhật thông thường. Sử dụng cập nhật riêng hình ảnh xác nhận.");
            }
            
            if (request.IssueTypeId != null)
            {
                throw new BadRequestException("Không thể thay đổi loại vấn đề sau khi đã tạo.");
            }
        }
        
        // Validate staff exists if staffId is provided
        if (request.StaffId != null)
        {
            // The staffId in the request is actually the userId
            // Find the staff by userId directly using the staff repository
            var staff = await staffRepository.FirstOrDefaultAsync(s => s.UserId == request.StaffId.Value);
            
            if (staff == null)
            {
                throw new BadRequestException("Không tìm thấy nhân viên với ID người dùng đã cung cấp.");
            }
            
            // Now that we have the actual staff entity, use staff.Id for the rest of the validation
            var projectStaffRepository = _unitOfWork.Repository<ProjectStaff>();
            var constructionItemRepository = _unitOfWork.Repository<ConstructionItem>();
            
            // Get the construction item and project info
            var constructionItem = await constructionItemRepository.FindAsync(projectIssue.ConstructionItemId)
                ?? throw new BadRequestException("Không tìm thấy hạng mục xây dựng cho vấn đề này.");
                
            // Check if staff is assigned to the project
            var isStaffAssignedToProject = await projectStaffRepository.FirstOrDefaultAsync(
                ps => ps.StaffId == staff.Id && ps.ProjectId == constructionItem.ProjectId);
                
            if (isStaffAssignedToProject == null)
            {
                throw new BadRequestException("Nhân viên này không được phân công cho dự án này.");
            }
            
            // Check if staff is already busy with other tasks or issues
            var constructionTaskRepository = _unitOfWork.Repository<ConstructionTask>();
            
            // Check active construction tasks
            var busyWithConstructionTask = await constructionTaskRepository.FirstOrDefaultAsync(
                ct => ct.StaffId == staff.Id && 
                      ct.Status != "DONE" && 
                      ct.IsActive == true);
                      
            if (busyWithConstructionTask != null)
            {
                throw new BadRequestException("Nhân viên này đang bận với một công việc xây dựng khác.");
            }
            
            // Check active project issues (excluding the current one being updated)
            var busyWithProjectIssue = await projectIssueRepository.FirstOrDefaultAsync(
                pi => pi.StaffId == staff.Id && 
                      pi.Status != EnumProjectIssueStatus.DONE.ToString() && 
                      pi.Id != id && 
                      pi.IsActive == true);
                      
            if (busyWithProjectIssue != null)
            {
                throw new BadRequestException("Nhân viên này đang bận giải quyết vấn đề khác.");
            }
            
            // Set the StaffId to the actual staff ID, not the user ID
            projectIssue.StaffId = staff.Id;
            
            // We'll add StaffId to excludeProperties after it's created
            request.StaffId = null; // Prevent reflection from trying to update it
        }
        
        // If name is provided, validate uniqueness
        if (request.Name != null)
        {
            var existingIssue = await projectIssueRepository
                .FirstOrDefaultAsync(pi => pi.Name == request.Name && 
                                    pi.ConstructionItemId == projectIssue.ConstructionItemId && 
                                    pi.Id != id);
            
            if (existingIssue != null)
            {
                throw new BadRequestException($"Vấn đề dự án với tên '{request.Name}' đã tồn tại cho hạng mục xây dựng này.");
            }
        }
        
        // Create list of properties to exclude from update
        var excludeProperties = new List<string>();
        
        // Always exclude StaffId since we handle it separately
        excludeProperties.Add("StaffId");
        
        // Conditionally exclude properties based on update type
        if (isNormalUpdate)
        {
            if (string.IsNullOrEmpty(originalReason))
                excludeProperties.Add("Reason");
                
            if (originalStaffId == null)
                excludeProperties.Add("StaffId");
                
            if (string.IsNullOrEmpty(originalConfirmImage))
                excludeProperties.Add("ConfirmImage");
                
            // Always exclude these in normal update
            excludeProperties.Add("IssueTypeId");
        }
        else
        {
            // For special updates, don't allow direct status changes
            excludeProperties.Add("Status");
        }
        
        // Update the properties using reflection
        ReflectionUtil.UpdateProperties(request, projectIssue, excludeProperties);

        // Handle status updates based on update type and business rules
        
        // Case 1: Staff-only update - change status to PROCESSING if currently OPENING
        if (isStaffOnlyUpdate && projectIssue.Status == EnumProjectIssueStatus.OPENING.ToString())
        {
            projectIssue.Status = EnumProjectIssueStatus.PROCESSING.ToString();
        }
        // Case 2: Confirm-image-only update - change status to PREVIEWING
        else if (isConfirmImageOnlyUpdate)
        {
            projectIssue.Status = EnumProjectIssueStatus.PREVIEWING.ToString();
        }
        // Case 3: Reason-only update - change status from PREVIEWING to PROCESSING
        else if (isReasonOnlyUpdate && projectIssue.Status == EnumProjectIssueStatus.PREVIEWING.ToString())
        {
            projectIssue.Status = EnumProjectIssueStatus.PROCESSING.ToString();
        }

        // Save the changes
        await projectIssueRepository.UpdateAsync(projectIssue, false);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task DeleteIssueImageAsync(Guid id)
    {
        // This method is no longer applicable with the new model
        // Instead, we'll clear the IssueImage field for the specified project issue
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        
        var projectIssue = await projectIssueRepository.FindAsync(id)
            ?? throw new NotFoundException("Không tìm thấy vấn đề dự án với ID đã cung cấp.");
            
        projectIssue.IssueImage = null;
        await projectIssueRepository.UpdateAsync(projectIssue);
        await _unitOfWork.SaveChangesAsync();
    }
    
    private void ValidateProjectIssueRequest(CommandProjectIssueRequest request)
    {
        // Only validate the required fields for creating a project issue
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Tên vấn đề dự án không được để trống.");
        }
        
        if (request.IssueTypeId == null)
        {
            throw new BadRequestException("Loại vấn đề không được để trống.");
        }
        
        // Validate cause is required
        if (string.IsNullOrWhiteSpace(request.Cause))
        {
            throw new BadRequestException("Nguyên nhân không được để trống.");
        }
        
        // Validate issue image is required
        if (string.IsNullOrWhiteSpace(request.IssueImage))
        {
            throw new BadRequestException("Hình ảnh vấn đề không được để trống.");
        }
    }

    public async Task ConfirmProjectIssueAsync(Guid id)
    {
        // Get repositories
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        var constructionItemRepository = _unitOfWork.Repository<ConstructionItem>();
        
        // Find and validate project issue
        var projectIssue = await projectIssueRepository.FindAsync(id)
            ?? throw new NotFoundException("Không tìm thấy vấn đề dự án với ID đã cung cấp.");
            
        if (projectIssue.Status != EnumProjectIssueStatus.PREVIEWING.ToString())
        {
            throw new BadRequestException("Chỉ có thể xác nhận hoàn thành vấn đề đang ở trạng thái PREVIEWING.");
        }
        
        // Change issue status to DONE and set ActualAt to current date
        projectIssue.Status = EnumProjectIssueStatus.DONE.ToString();
        
        // Set the actual completion date to current date using GlobalUtility
        var currentDate = GlobalUtility.GetCurrentSEATime().Date;
        projectIssue.ActualAt = DateOnly.FromDateTime(currentDate);
        
        await projectIssueRepository.UpdateAsync(projectIssue, false);
        
        // Get the construction item (level 1) associated with this issue
        var constructionItem = await constructionItemRepository.FindAsync(projectIssue.ConstructionItemId)
            ?? throw new NotFoundException("Không tìm thấy hạng mục xây dựng cho vấn đề này.");
        
        // Verify this is a level 1 item (no parent)
        if (constructionItem.ParentId != null)
        {
            throw new BadRequestException("Vấn đề dự án chỉ có thể liên kết với hạng mục xây dựng cấp 1.");
        }
        
        // Check if we should update the level 1 construction item status
        // Pass the current issue ID to exclude it from the active issues check
        bool shouldUpdateToCompleted = await ShouldUpdateLevel1ItemStatusAsync(
            constructionItem.Id, 
            projectIssue.Id,
            constructionItemRepository, 
            projectIssueRepository);
            
        // If all conditions are met, update the status here
        if (shouldUpdateToCompleted)
        {
            constructionItem.Status = EnumConstructionItemStatus.DONE.ToString();
            await constructionItemRepository.UpdateAsync(constructionItem, false);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    private async Task<bool> ShouldUpdateLevel1ItemStatusAsync(
        Guid level1ItemId,
        Guid currentIssueId,
        IRepository<ConstructionItem> constructionItemRepository,
        IRepository<ProjectIssue> projectIssueRepository)
    {
        // Get the level 1 item
        var level1Item = await constructionItemRepository.FindAsync(level1ItemId);
        if (level1Item == null || level1Item.ParentId != null) return false; // Not a level 1 item
        
        // Skip if already marked as DONE
        if (level1Item.Status == EnumConstructionItemStatus.DONE.ToString()) return false;
        
        // Check if level 1 item has any unresolved project issues, excluding the current issue
        bool hasActiveIssues = await projectIssueRepository
            .Where(pi => pi.ConstructionItemId == level1ItemId && 
                         pi.Status != EnumProjectIssueStatus.DONE.ToString() &&
                         pi.IsActive == true &&
                         pi.Id != currentIssueId)  // Exclude the current issue that's being marked as DONE
            .AnyAsync();
        
        // If level 1 item has active issues, it can't be marked as DONE
        if (hasActiveIssues) return false;
        
        // Check if all level 2 children are already DONE
        var childItems = await constructionItemRepository
            .Where(ci => ci.ParentId == level1ItemId && ci.IsActive == true)
            .ToListAsync();
        
        // If there are no children or all children are already DONE
        if (!childItems.Any() || childItems.All(c => c.Status == EnumConstructionItemStatus.DONE.ToString()))
        {
            // All conditions are met to update the level 1 item status to DONE
            return true;
        }
        
        return false;
    }
}
