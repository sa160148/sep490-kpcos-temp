using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.Common.Exceptions;

namespace KPCOS.BusinessLayer.Services.Implements;

public class DesignService : IDesignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DesignService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new design with associated design images
    /// </summary>
    /// <param name="designerId">The ID of the user (designer) creating the design</param>
    /// <param name="request">The design creation request containing project and image information</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="BadRequestException">Thrown when the designer ID is not found in the staff table</exception>
    public async Task CreateDesignAsync(
        Guid designerId, 
        CreateDesignRequest request)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = _mapper.Map<Design>(request);
        design.Id = Guid.NewGuid(); 
        design.StaffId = _unitOfWork.Repository<Staff>().SingleOrDefaultAsync(staff => staff.UserId == designerId).Result.Id;
        foreach (var image in design.DesignImages)
        {
            image.Id = Guid.NewGuid();
            image.DesignId = design.Id;
        }
        await repo.AddAsync(design);
    }

    /// <summary>
    /// Rejects a design and updates its status to REJECTED
    /// </summary>
    /// <param name="id">The ID of the design to reject</param>
    /// <param name="request">The rejection request containing the reason for rejection</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    public async Task RejectDesignAsync(
        Guid id, 
        RejectDesignRequest request)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = await repo.SingleOrDefaultAsync(d => d.Id == id);
        if (design == null)
        {
            throw new NotFoundException("Không tìm thấy Design");
        }

        if (design.Status != EnumDesignStatus.OPENING.ToString())
        {
            throw new BadRequestException("Design đang không ở trạng thái mở");
        }
        design.Status = EnumDesignStatus.REJECTED.ToString();
        design.Reason = request.Reason;
        await repo.UpdateAsync(design);
    }

    /// <summary>
    /// Accepts a design and updates its status based on the user's role
    /// </summary>
    /// <param name="id">The ID of the design to accept</param>
    /// <param name="role">The role of the user accepting the design (MANAGER or CUSTOMER)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    /// <remarks>
    /// For managers: Changes design status to PREVIEWING
    /// For customers: Changes design status to CONFIRMED and updates project status to CONSTRUCTING
    /// </remarks>
    public async Task AcceptDesignAsync(
        Guid id, 
        string role)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = await repo.SingleOrDefaultAsync(d => d.Id == id);
        if (design == null)
        {
            throw new NotFoundException("Không tìm thấy Design");
        }

        if (role == RoleEnum.MANAGER.ToString() && 
            design.Status == EnumDesignStatus.OPENING.ToString())
        {
            design.Status = EnumDesignStatus.PREVIEWING.ToString();
        }
        if (role == RoleEnum.CUSTOMER.ToString() && 
            design.Status == EnumDesignStatus.PREVIEWING.ToString())
        {
            design.Status = EnumDesignStatus.CONFIRMED.ToString();
            
            // Only update project status to CONSTRUCTING for 2D designs
            if (design.Type.Equals("2D", StringComparison.OrdinalIgnoreCase))
            {
                var projectRepo = _unitOfWork.Repository<Project>();
                var project = await projectRepo.SingleOrDefaultAsync(p => p.Id == design.ProjectId);
                if (project == null)
                {
                    throw new NotFoundException("Không tìm thấy Project");
                }
                project.Status = EnumProjectStatus.CONSTRUCTING.ToString();
                await projectRepo.UpdateAsync(project, false);
            }
        }
        
        await repo.UpdateAsync(design, false);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Requests an edit for a design and updates its status to EDITING
    /// </summary>
    /// <param name="id">The ID of the design to edit</param>
    /// <param name="request">The request containing the reason for the edit</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    public async Task EditDesignAsync(
        Guid id, 
        RejectDesignRequest request)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = await repo.SingleOrDefaultAsync(d => d.Id == id);
        if (design == null)
        {
            throw new NotFoundException("Không tìm thấy Design");
        }
        design.Status = EnumDesignStatus.EDITING.ToString();
        design.Reason = request.Reason;
        await repo.UpdateAsync(design);
    }

    /// <summary>
    /// Updates an existing design by creating a new version with the updated information
    /// </summary>
    /// <param name="id">The ID of the design to update</param>
    /// <param name="userId">The ID of the user (designer) updating the design</param>
    /// <param name="request">The update request containing the new design information</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    public async Task UpdateDesignAsync(
        Guid id, 
        Guid userId, 
        UpdateDesignRequest request)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = await repo.SingleOrDefaultAsync(d => d.Id == id);
        if (design == null)
        {
            throw new NotFoundException("Không tìm thấy Design");
        }
        if (design.Status == EnumDesignStatus.EDITING.ToString())
        {
            await UpdateDesignEditingAsync(design, userId, request);
        }
        if (design.Status == EnumDesignStatus.REJECTED.ToString())
        {
            await UpdateDesignRejectedAsync(design, userId, request);
        }
    }

    public async Task<GetDesignDetailResponse> GetDesignDetailAsync(Guid id)
    {
        var repo = _unitOfWork.Repository<Design>();
        var design = repo.Get(
                filter: d => d.Id == id,
                includeProperties: "DesignImages,Project")
            .SingleOrDefault();
        if (design == null)
        {
            throw new NotFoundException("Không tìm thấy Design");
        }
        return _mapper.Map<GetDesignDetailResponse>(design);
    }

    private async Task UpdateDesignEditingAsync(
        Design design, 
        Guid userId, 
        UpdateDesignRequest request)
    {
        Design clonedDesign = _mapper.Map<Design>(request);
        clonedDesign.Id = Guid.NewGuid();
        clonedDesign.StaffId = _unitOfWork.Repository<Staff>().SingleOrDefaultAsync(staff => staff.UserId == userId).Result.Id;
        foreach (var image in clonedDesign.DesignImages)
        {
            image.Id = Guid.NewGuid();
            image.DesignId = clonedDesign.Id;
        }
        design.Status = EnumDesignStatus.PREVIEWING.ToString();
        clonedDesign.Version = design.Version + 1;
        clonedDesign.ProjectId = design.ProjectId;
        
        await _unitOfWork.Repository<Design>().AddAsync(clonedDesign, false);
        await _unitOfWork.Repository<Design>().UpdateAsync(design, false);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateDesignRejectedAsync(
        Design design, 
        Guid userId, 
        UpdateDesignRequest request)
    {
        // Remove existing design images
        var designImageRepo = _unitOfWork.Repository<DesignImage>();
        var designImages = designImageRepo.Get(di => di.DesignId == design.Id).ToList();
        designImageRepo.RemoveRange(designImages);

        // Map request properties to existing design
        design.Type = request.Type;
        design.Status = EnumDesignStatus.OPENING.ToString();
        design.StaffId = _unitOfWork.Repository<Staff>().SingleOrDefaultAsync(staff => staff.UserId == userId).Result.Id;
        design.Version = design.Version + 1;

        // Create and add new design images
        foreach (var imageRequest in request.DesignImages)
        {
            var image = new DesignImage
            {
                Id = Guid.NewGuid(),
                DesignId = design.Id,
                ImageUrl = imageRequest.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await designImageRepo.AddAsync(image, false);
        }

        await _unitOfWork.Repository<Design>().UpdateAsync(design, false);
        await _unitOfWork.SaveChangesAsync();
    }
}