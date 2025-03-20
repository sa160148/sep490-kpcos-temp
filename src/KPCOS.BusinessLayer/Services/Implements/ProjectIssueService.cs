using System;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ProjectIssueService : IProjectIssueService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectIssueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateProjectIssueAsync(
        Guid constructionItemId, 
        CommandProjectIssueRequest request, 
        Guid userId)
    {
        // Get repositories
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        var constructionItemRepository = _unitOfWork.Repository<ConstructionItem>();
        var issueImageRepository = _unitOfWork.Repository<IssueImage>();
        
        // Validate request
        ValidateProjectIssueRequest(request);
        
        // Validate construction item exists
        var constructionItem = await constructionItemRepository.FindAsync(constructionItemId)
            ?? throw new NotFoundException("Không tìm thấy hạng mục xây dựng với ID đã cung cấp.");

        // Find the root level construction item (parent = null or lv1)
        var rootItem = constructionItem;
        if (rootItem.ParentId != null)
        {
            rootItem = await constructionItemRepository.FirstOrDefaultAsync(ci => ci.Id == constructionItem.ParentId && ci.ParentId == null);
            if (rootItem == null)
            {
                throw new BadRequestException("Không tìm thấy hạng mục xây dựng cấp 1 cho ID đã cung cấp.");
            }
        }

        // Validate issue name is unique for this construction item
        var existingIssue = await projectIssueRepository
            .FirstOrDefaultAsync(pi => pi.Name == request.Name && pi.ConstructionItemId == rootItem.Id);
        
        if (existingIssue != null)
        {
            throw new BadRequestException($"Vấn đề dự án với tên '{request.Name}' đã tồn tại cho hạng mục xây dựng này.");
        }

        var projectIssueId = Guid.NewGuid();
        // Create new project issue with only required properties
        var projectIssue = new ProjectIssue
        {
            Id = projectIssueId,
            Status = EnumProjectIssueStatus.OPENING.ToString(),
            ConstructionItemId = rootItem.Id,
            UserId = userId
        };

        // Manually set only specific properties from request
        projectIssue.Name = request.Name;
        
        if (request.Description != null)
            projectIssue.Description = request.Description;
        
        if (request.Reason != null)
            projectIssue.Reason = request.Reason;
        
        if (request.IssueTypeId != null)
            projectIssue.IssueTypeId = request.IssueTypeId.Value;

        // Save the project issue (don't save changes to database yet)
        await projectIssueRepository.AddAsync(projectIssue, false);

        // Process issue images if provided
        if (request.IssueImages != null && request.IssueImages.Any())
        {
            foreach (var imageRequest in request.IssueImages)
            {
                var issueImage = new IssueImage
                {
                    Name = imageRequest.Name ?? null,
                    ImageUrl = imageRequest.ImageUrl,
                    ProjectIssueId = projectIssueId
                };
                
                await issueImageRepository.AddAsync(issueImage, false);
            }
        }

        // Now save all changes to database
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task UpdateProjectIssueAsync(Guid id, CommandProjectIssueRequest request)
    {
        // Get repositories
        var projectIssueRepository = _unitOfWork.Repository<ProjectIssue>();
        var issueImageRepository = _unitOfWork.Repository<IssueImage>();
        
        // Find project issue
        var projectIssue = await projectIssueRepository.FindAsync(id)
            ?? throw new NotFoundException("Không tìm thấy vấn đề dự án với ID đã cung cấp.");

        // Save the original solution to check if it's being updated from null to non-null
        var originalSolution = projectIssue.Solution;
        
        // Check if only solution is being updated (special case)
        bool onlySolutionUpdate = 
            request.Solution != null && 
            request.Name == null && 
            request.Description == null && 
            request.Reason == null && 
            request.Status == null &&
            request.IssueTypeId == null && 
            request.IsSolved == null &&
            (request.IssueImages == null || !request.IssueImages.Any());
        
        // Determine if this is a special "mark as solved" request where only IsSolved is provided
        bool isMarkAsSolvedRequest = request.IsSolved == true && 
            (request.Name == null && 
            request.Description == null && 
            request.Reason == null && 
            request.Status == null &&
            request.IssueTypeId == null);

        // Solution field handling based on update type
        if (request.Solution != null)
        {
            // Allow updating the solution field if:
            // 1. This is an only-solution update (special case that should update status) OR
            // 2. This is a mark-as-solved request OR
            // 3. The solution already exists in the database
            bool canUpdateSolution = 
                onlySolutionUpdate || 
                isMarkAsSolvedRequest || 
                !string.IsNullOrEmpty(originalSolution);
                
            if (!canUpdateSolution)
            {
                throw new BadRequestException("Không thể thêm giải pháp trong một cập nhật thông thường. Sử dụng chức năng cập nhật riêng giải pháp hoặc đánh dấu đã giải quyết.");
            }
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
        
        // Validate issue images if provided
        if (request.IssueImages != null && request.IssueImages.Any())
        {
            // Validate that each issue image has an ImageUrl
            foreach (var image in request.IssueImages)
            {
                if (string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    throw new BadRequestException("URL hình ảnh không được để trống.");
                }
            }
        }

        // Create a new properties list to exclude from update
        var excludeProperties = new List<string> { "Status", "IssueImages" };
        
        // Use reflection to update properties from request to projectIssue
        ReflectionUtil.UpdateProperties(request, projectIssue, excludeProperties);

        // Handle status updates based on business rules
        
        // Case 1: If IsSolved is true, update status to SOLVED
        if (request.IsSolved == true)
        {
            projectIssue.Status = EnumProjectIssueStatus.SOLVED.ToString();
        }
        // Case 2: If this is only a solution update and status is OPENING, change to PROCESSING
        else if (onlySolutionUpdate && projectIssue.Status == EnumProjectIssueStatus.OPENING.ToString())
        {
            projectIssue.Status = EnumProjectIssueStatus.PROCESSING.ToString();
        }
        // Case 3: If solution is being updated (not added for the first time) and status is OPENING, change to PROCESSING
        else if (!string.IsNullOrEmpty(originalSolution) && request.Solution != null && 
                projectIssue.Status == EnumProjectIssueStatus.OPENING.ToString())
        {
            projectIssue.Status = EnumProjectIssueStatus.PROCESSING.ToString();
        }
        // Case 4: Only update status from request if not in the only-solution-update case and IsSolved is not true
        else if (request.Status != null && !onlySolutionUpdate && request.IsSolved != true)
        {
            projectIssue.Status = request.Status;
        }
        
        // Process issue images if provided
        if (request.IssueImages != null && request.IssueImages.Any())
        {
            foreach (var imageRequest in request.IssueImages)
            {
                var issueImage = new IssueImage
                {
                    Name = imageRequest.Name,
                    ImageUrl = imageRequest.ImageUrl,
                    ProjectIssueId = projectIssue.Id
                };
                
                await issueImageRepository.AddAsync(issueImage, false);
            }
        }

        // Save the changes
        await projectIssueRepository.UpdateAsync(projectIssue, false);

        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task DeleteIssueImageAsync(Guid id)
    {
        var issueImageRepository = _unitOfWork.Repository<IssueImage>();
        
        var issueImage = await issueImageRepository.FindAsync(id)
            ?? throw new NotFoundException("Không tìm thấy hình ảnh với ID đã cung cấp.");
            
        await issueImageRepository.RemoveAsync(issueImage);
    }
    
    private void ValidateProjectIssueRequest(CommandProjectIssueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Tên vấn đề dự án không được để trống.");
        }
        
        if (request.IssueTypeId == null)
        {
            throw new BadRequestException("Loại vấn đề không được để trống.");
        }
        
        // Validate issue images if provided
        if (request.IssueImages != null && request.IssueImages.Any())
        {
            foreach (var image in request.IssueImages)
            {
                if (string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    throw new BadRequestException("URL hình ảnh không được để trống.");
                }
            }
        }
    }
}
