using System;
using System.Text;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Promotions;
using KPCOS.BusinessLayer.DTOs.Response.Promotions;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.BusinessLayer.Services.Implements;

public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBackgroundService _backgroundService;

    public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, IBackgroundService backgroundService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _backgroundService = backgroundService;
    }

    public async Task<(IEnumerable<GetAllPromotionResponse> data, int total)> GetAllPromotions(GetAllPromotionFilterRequest filter)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        
        // Get paginated results directly using filter methods
        var result = promotionRepo.GetWithCount(
            filter.GetExpressions(),
            filter.GetOrder(),
            includeProperties: "",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );

        var promotionsResponse = _mapper.Map<IEnumerable<GetAllPromotionResponse>>(result.Data);
        return (promotionsResponse, result.Count);
    }

    public async Task<GetAllPromotionResponse> GetPromotionById(Guid id)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        var promotion = await promotionRepo.FindAsync(id);
        if (promotion == null || promotion.IsActive == false)
        {
            throw new NotFoundException("Promotion not found");
        }

        return _mapper.Map<GetAllPromotionResponse>(promotion);
    }

    public async Task CreatePromotion(CommandPromotionRequest request)
    {
        if (request.Discount == null || request.Discount <= 0)
        {
            throw new BadRequestException("Giảm giá phải lớn hơn 0");
        }

        var promotion = _mapper.Map<Promotion>(request);
        var promotionRepo = _unitOfWork.Repository<Promotion>();

        // Auto-generate name if not provided
        if (string.IsNullOrEmpty(promotion.Name))
        {
            promotion.Name = $"Khuyến mãi {promotion.Discount}%";
        }

        // Auto-generate code if not provided
        if (string.IsNullOrEmpty(promotion.Code))
        {
            promotion.Code = GlobalUtility.GenerateRandomCode(6);
        }
        else
        {
            // Check if code already exists
            var existingPromo = await promotionRepo.FirstOrDefaultAsync(p => p.Code == promotion.Code && p.IsActive == true);
            if (existingPromo != null)
            {
                throw new BadRequestException("Mã khuyến mãi đã tồn tại");
            }
        }

        // Use GetCurrentSEATime for date handling, similar to PaymentVnpayCallback
        var currentDate = GlobalUtility.GetCurrentSEATime();
        var normalizedCurrentDate = GlobalUtility.NormalizeDateTime(currentDate) ?? currentDate;
        
        // Handle StartAt and ExpiredAt dates using the new utility function
        promotion.StartAt = GlobalUtility.NormalizeDateTime(promotion.StartAt);
        promotion.ExpiredAt = GlobalUtility.NormalizeDateTime(promotion.ExpiredAt);

        // Set default status based on dates
        if (promotion.StartAt != null && promotion.ExpiredAt != null)
        {   
            if (promotion.StartAt > normalizedCurrentDate)
            {
                promotion.Status = EnumPromotionStatus.PENDING.ToString();
                
                // Schedule background job to activate promotion when start date is reached
                var delayToStart = promotion.StartAt.Value - normalizedCurrentDate;
                if (delayToStart.TotalMinutes > 0)
                {
                    _backgroundService.DelayedActivatePromotionJob((int)delayToStart.TotalMinutes, promotion.Id);
                }
            }
            else if (promotion.StartAt <= normalizedCurrentDate && promotion.ExpiredAt >= normalizedCurrentDate)
            {
                promotion.Status = EnumPromotionStatus.ACTIVE.ToString();
                
                // Schedule background job to expire the promotion
                if (promotion.ExpiredAt.HasValue)
                {
                    var delayTimeSpan = promotion.ExpiredAt.Value - normalizedCurrentDate;
                    if (delayTimeSpan.TotalMinutes > 0)
                    {
                        // Schedule job to set promotion to expired
                        _backgroundService.DelayedExpirePromotionJob((int)delayTimeSpan.TotalMinutes, promotion.Id);
                    }
                }
            }
            else
            {
                promotion.Status = EnumPromotionStatus.EXPIRED.ToString();
            }
        }
        else
        {
            promotion.Status = EnumPromotionStatus.ACTIVE.ToString();
        }

        // Database will handle Id, CreatedAt, and UpdatedAt
        promotion.IsActive = true;
        await promotionRepo.AddAsync(promotion);
    }

    public async Task UpdatePromotion(Guid id, CommandPromotionRequest request)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        var promotion = await promotionRepo.FindAsync(id);
        if (promotion == null || promotion.IsActive == false)
        {
            throw new NotFoundException("Không tìm thấy khuyến mãi");
        }

        if (request.Discount != null && request.Discount <= 0)
        {
            throw new BadRequestException("Giảm giá phải lớn hơn 0");
        }

        // Check if code is being changed and if the new code already exists
        if (!string.IsNullOrEmpty(request.Code) && request.Code != promotion.Code)
        {
            var existingPromo = await promotionRepo.FirstOrDefaultAsync(p => p.Code == request.Code && p.IsActive == true && p.Id != id);
            if (existingPromo != null)
            {
                throw new BadRequestException("Mã khuyến mãi đã tồn tại");
            }
        }

        // Get current date using GlobalUtility
        var currentDate = GlobalUtility.GetCurrentSEATime();
        var normalizedCurrentDate = GlobalUtility.NormalizeDateTime(currentDate) ?? currentDate;
        
        // Update properties from request (excluding Status and dates that need special handling)
        ReflectionUtil.UpdateProperties(request, promotion, new List<string> { "Status", "StartAt", "ExpiredAt" });
        
        // Explicitly update date fields using the new utility function
        if (request.StartAt.HasValue)
        {
            promotion.StartAt = GlobalUtility.NormalizeDateTime(request.StartAt);
        }
        
        if (request.ExpiredAt.HasValue)
        {
            promotion.ExpiredAt = GlobalUtility.NormalizeDateTime(request.ExpiredAt);
        }
        
        // Update name if discount changed and name was auto-generated
        if (request.Discount.HasValue && promotion.Name.Contains("Khuyến mãi"))
        {
            promotion.Name = $"Khuyến mãi {request.Discount}%";
        }

        // Update status based on dates
        if (promotion.StartAt != null && promotion.ExpiredAt != null)
        {            
            if (promotion.StartAt > normalizedCurrentDate)
            {
                promotion.Status = EnumPromotionStatus.PENDING.ToString();
                
                // Schedule activation job
                var delayToStart = promotion.StartAt.Value - normalizedCurrentDate;
                if (delayToStart.TotalMinutes > 0)
                {
                    _backgroundService.DelayedActivatePromotionJob((int)delayToStart.TotalMinutes, promotion.Id);
                }
            }
            else if (promotion.StartAt <= normalizedCurrentDate && promotion.ExpiredAt >= normalizedCurrentDate)
            {
                promotion.Status = EnumPromotionStatus.ACTIVE.ToString();
                
                // Schedule background job to expire the promotion
                if (promotion.ExpiredAt.HasValue)
                {
                    var delayTimeSpan = promotion.ExpiredAt.Value - normalizedCurrentDate;
                    if (delayTimeSpan.TotalMinutes > 0)
                    {
                        // Schedule job to set promotion to expired
                        _backgroundService.DelayedExpirePromotionJob((int)delayTimeSpan.TotalMinutes, promotion.Id);
                    }
                }
            }
            else
            {
                promotion.Status = EnumPromotionStatus.EXPIRED.ToString();
            }
        }

        // Database will handle UpdatedAt
        await promotionRepo.UpdateAsync(promotion);
    }

    public async Task DeletePromotion(Guid id)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        var promotion = await promotionRepo.FindAsync(id);
        if (promotion == null)
        {
            throw new NotFoundException("Không tìm thấy khuyến mãi");
        }

        // Check if this promotion is used in any quotations
        var quotationRepo = _unitOfWork.Repository<Quotation>();
        var quotationsUsingPromotion = await quotationRepo.Where(q => q.PromotionId == id).AnyAsync();
        
        if (quotationsUsingPromotion)
        {
            // If promotion is in use, just set it as inactive
            promotion.IsActive = false;
            await promotionRepo.UpdateAsync(promotion);
        }
        else
        {
            // If not in use, it can be deleted from the database
            await promotionRepo.RemoveAsync(promotion);
        }
    }
}
