using System;
using KPCOS.BusinessLayer.DTOs.Request.Notifications;
using KPCOS.BusinessLayer.DTOs.Response.Notifications;

namespace KPCOS.BusinessLayer.Services;

public interface INotificationService
{
    /// <summary>
    /// Get all notifications with optional filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="userId">Optional user ID to filter notifications for a specific user</param>
    /// <returns>Collection of notifications and total count</returns>
    Task<(IEnumerable<GetAllNotificationResponse> notifications, int total)> GetAllNotificationAsync(
        GetAllNotificationFilterRequest filter, 
        Guid? userId = null);

    /// <summary>
    /// Create a new notification
    /// </summary>
    /// <param name="request">Notification data</param>
    /// <returns>Task</returns>
    Task CreateNotificationAsync(CommandNotificationRequest request);

    /// <summary>
    /// Get notification by ID
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification details</returns>
    Task<GetAllNotificationResponse> GetNotificationByIdAsync(Guid id);
}
