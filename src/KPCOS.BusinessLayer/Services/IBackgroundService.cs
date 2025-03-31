using System;

namespace KPCOS.BusinessLayer.Services;

/// <summary>
/// Interface dịch vụ cho các tác vụ chạy nền trong hệ thống xây dựng và bảo trì hồ cá Koi
/// </summary>
public interface IBackgroundService
{
    Task DelayedJob();
    /// <summary>
    /// Delay cancel otp job.
    /// <para>This function will delete the otp document in firebase after the minute that put in</para>
    /// </summary>
    /// <param name="timespanMinutes">int, the lifetime of a contract otp</param>
    /// <param name="contractId">string</param>
    void DelayedCancelOtpJob(int timespanMinutes, string contractId);
    
    /// <summary>
    /// Delay cancel doc otp job.
    /// <para>This function will delete the doc otp document in firebase after the minute that put in</para>
    /// </summary>
    /// <param name="timespanMinutes">int, the lifetime of a document otp</param>
    /// <param name="docId">string</param>
    void DelayedCancelDocOtpJob(int timespanMinutes, string docId);

    /// <summary>
    /// Schedule a job to set a promotion status to EXPIRED after a delay
    /// </summary>
    /// <param name="timespanMinutes">Minutes to wait before executing the job</param>
    /// <param name="promotionId">ID of the promotion to expire</param>
    void DelayedExpirePromotionJob(int timespanMinutes, Guid promotionId);

    /// <summary>
    /// Schedule a job to set a promotion status to ACTIVE after a delay
    /// </summary>
    /// <param name="timespanMinutes">Minutes to wait before executing the job</param>
    /// <param name="promotionId">ID of the promotion to activate</param>
    void DelayedActivatePromotionJob(int timespanMinutes, Guid promotionId);

    /// <summary>
    /// Cập nhật trạng thái của khuyến mãi dựa trên ngày hiện tại và thời hạn của khuyến mãi
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi cần cập nhật trạng thái</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    Task SetPromotionStatusAsync(Guid promotionId);
    
    /// <summary>
    /// Lên lịch tác vụ chạy nền để kích hoạt khuyến mãi vào ngày bắt đầu
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi</param>
    /// <param name="startAt">Thời gian bắt đầu của khuyến mãi</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    Task SchedulePromotionActivationAsync(Guid promotionId, DateTime startAt);
    
    /// <summary>
    /// Lên lịch tác vụ chạy nền để kết thúc khuyến mãi vào ngày hết hạn
    /// </summary>
    /// <param name="promotionId">ID của khuyến mãi</param>
    /// <param name="expireAt">Thời gian hết hạn của khuyến mãi</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    Task SchedulePromotionExpirationAsync(Guid promotionId, DateTime expireAt);
}