using Hangfire;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;

namespace KPCOS.BusinessLayer.Services.Implements;

/// <summary>
/// Dịch vụ quản lý các tác vụ chạy nền trong hệ thống xây dựng và bảo trì hồ cá Koi
/// </summary>
public class BackgroundService : IBackgroundService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IFirebaseService _firebaseService;

    /// <summary>
    /// Khởi tạo dịch vụ nền với các phụ thuộc cần thiết
    /// </summary>
    public BackgroundService(IBackgroundJobClient backgroundJobClient, IUnitOfWork unitOfWork, IFirebaseService firebaseService)
    {
        _backgroundJobClient = backgroundJobClient;
        _unitOfWork = unitOfWork;
        _firebaseService = firebaseService;
    }

    public Task DelayedJob()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Hủy mã OTP của hợp đồng sau khoảng thời gian nhất định
    /// </summary>
    public void DelayedCancelOtpJob(int timespanMinutes, string contractId)
    { 
        _backgroundJobClient.Schedule(() => _firebaseService.DeleteContractOtpAsync(contractId), TimeSpan.FromMinutes(timespanMinutes));
    }
    
    /// <summary>
    /// Hủy mã OTP của tài liệu sau khoảng thời gian nhất định
    /// </summary>
    public void DelayedCancelDocOtpJob(int timespanMinutes, string docId)
    { 
        _backgroundJobClient.Schedule(() => _firebaseService.DeleteDocOtpAsync(docId), TimeSpan.FromMinutes(timespanMinutes));
    }

    /// <summary>
    /// Lên lịch kết thúc khuyến mãi sau khoảng thời gian nhất định
    /// </summary>
    public void DelayedExpirePromotionJob(int timespanMinutes, Guid promotionId)
    {
        _backgroundJobClient.Schedule(() => SetPromotionStatusAsync(promotionId, EnumPromotionStatus.EXPIRED.ToString()), 
            TimeSpan.FromMinutes(timespanMinutes));
    }

    /// <summary>
    /// Lên lịch kích hoạt khuyến mãi sau khoảng thời gian nhất định
    /// </summary>
    public void DelayedActivatePromotionJob(int timespanMinutes, Guid promotionId)
    {
        _backgroundJobClient.Schedule(() => SetPromotionStatusAsync(promotionId, EnumPromotionStatus.ACTIVE.ToString()), 
            TimeSpan.FromMinutes(timespanMinutes));
    }

    /// <summary>
    /// Cập nhật trạng thái của khuyến mãi
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi</param>
    /// <param name="status">Trạng thái mới cần thiết lập</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    public async Task SetPromotionStatusAsync(Guid promotionId, string status)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        var promotion = await promotionRepo.FindAsync(promotionId);
        if (promotion != null && promotion.IsActive == true)
        {
            promotion.Status = status;
            
            // Cơ sở dữ liệu sẽ tự động cập nhật trường UpdatedAt
            await promotionRepo.UpdateAsync(promotion);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái của khuyến mãi dựa trên ngày hiện tại và thời hạn của khuyến mãi
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi cần cập nhật trạng thái</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    public async Task SetPromotionStatusAsync(Guid promotionId)
    {
        var promotionRepo = _unitOfWork.Repository<Promotion>();
        var promotion = await promotionRepo.FindAsync(promotionId);
        if (promotion != null && promotion.IsActive == true)
        {
            var currentTime = GlobalUtility.GetCurrentSEATime();
            // Ensure we're comparing with normalized dates
            var normalizedCurrentTime = GlobalUtility.NormalizeDateTime(currentTime);
            
            // Only update status automatically if it's a date-based promotion
            if (promotion.StartAt.HasValue && promotion.ExpiredAt.HasValue)
            {
                if (normalizedCurrentTime < promotion.StartAt)
                {
                    promotion.Status = EnumPromotionStatus.PENDING.ToString();
                }
                else if (normalizedCurrentTime >= promotion.StartAt && normalizedCurrentTime <= promotion.ExpiredAt)
                {
                    promotion.Status = EnumPromotionStatus.ACTIVE.ToString();
                }
                else
                {
                    promotion.Status = EnumPromotionStatus.EXPIRED.ToString();
                }
                
                // Cơ sở dữ liệu sẽ tự động cập nhật trường UpdatedAt
                await promotionRepo.UpdateAsync(promotion);
            }
            // If it's not a date-based promotion, don't update status automatically
        }
    }
    
    /// <summary>
    /// Lên lịch tác vụ chạy nền để kích hoạt khuyến mãi vào ngày bắt đầu
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi</param>
    /// <param name="startAt">Thời gian bắt đầu của khuyến mãi</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    public Task SchedulePromotionActivationAsync(Guid promotionId, DateTime startAt)
    {
        var currentTime = GlobalUtility.GetCurrentSEATime();
        // Normalize the input date
        var normalizedStartAt = GlobalUtility.NormalizeDateTime(startAt) ?? startAt;
        var normalizedCurrentTime = GlobalUtility.NormalizeDateTime(currentTime) ?? currentTime;
        
        var delay = normalizedStartAt - normalizedCurrentTime;
        
        if (delay.TotalMinutes > 0)
        {
            DelayedActivatePromotionJob((int)delay.TotalMinutes, promotionId);
        }
        else
        {
            // Nếu thời gian bắt đầu đã qua, kích hoạt ngay lập tức
            return SetPromotionStatusAsync(promotionId, EnumPromotionStatus.ACTIVE.ToString());
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Lên lịch tác vụ chạy nền để kết thúc khuyến mãi vào ngày hết hạn
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi</param>
    /// <param name="expireAt">Thời gian hết hạn của khuyến mãi</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    public Task SchedulePromotionExpirationAsync(Guid promotionId, DateTime expireAt)
    {
        var currentTime = GlobalUtility.GetCurrentSEATime();
        // Normalize the input date
        var normalizedExpireAt = GlobalUtility.NormalizeDateTime(expireAt) ?? expireAt;
        var normalizedCurrentTime = GlobalUtility.NormalizeDateTime(currentTime) ?? currentTime;
        
        var delay = normalizedExpireAt - normalizedCurrentTime;
        
        if (delay.TotalMinutes > 0)
        {
            DelayedExpirePromotionJob((int)delay.TotalMinutes, promotionId);
        }
        else
        {
            // Nếu thời gian hết hạn đã qua, kết thúc ngay lập tức
            return SetPromotionStatusAsync(promotionId, EnumPromotionStatus.EXPIRED.ToString());
        }
        
        return Task.CompletedTask;
    }
}