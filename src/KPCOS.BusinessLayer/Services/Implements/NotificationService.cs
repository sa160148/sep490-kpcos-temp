using System;
using AutoMapper;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.BusinessLayer.DTOs.Request.Notifications;
using KPCOS.BusinessLayer.DTOs.Response.Notifications;
using KPCOS.BusinessLayer.DTOs.Notifications;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LinqKit;

namespace KPCOS.BusinessLayer.Services.Implements;

public class NotificationService : INotificationService
{
    private readonly IFirebaseService _firebaseService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IFirebaseService firebaseService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _firebaseService = firebaseService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    /// <summary>
    /// Get all notifications with optional filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="userId">Optional user ID to filter notifications for a specific user</param>
    /// <returns>Collection of notifications and total count</returns>
    public async Task<(IEnumerable<GetAllNotificationResponse> notifications, int total)> GetAllNotificationAsync(
        GetAllNotificationFilterRequest filter, 
        Guid? userId = null)
    {
        try
        {
            // Build the predicate for filtering
            var predicate = PredicateBuilder.New<Notification>(true);
            
            // Apply userId filter if provided
            if (userId.HasValue)
            {
                predicate = predicate.And(x => x.RecipientId == userId.ToString());
            }
            
            // Apply IsActive filter
            if (filter.IsActive.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == filter.IsActive.Value);
            }
            
            // Apply IsRead filter
            if (filter.IsRead.HasValue)
            {
                predicate = predicate.And(x => x.IsRead == filter.IsRead.Value);
            }
            
            // Apply search filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                predicate = predicate.And(x => 
                    (x.Name != null && x.Name.Contains(filter.Search)) ||
                    (x.Description != null && x.Description.Contains(filter.Search)) ||
                    (x.Link != null && x.Link.Contains(filter.Search))
                );
            }
            
            // Apply Type filter
            if (!string.IsNullOrEmpty(filter.Type))
            {
                var types = filter.Type.Split(',').ToList();
                predicate = predicate.And(x => types.Contains(x.Type));
            }
            
            // Apply Date Range filters
            if (filter.CreatedAtFrom.HasValue)
            {
                predicate = predicate.And(x => x.CreatedAt >= filter.CreatedAtFrom.Value);
            }
            
            if (filter.CreatedAtTo.HasValue)
            {
                predicate = predicate.And(x => x.CreatedAt <= filter.CreatedAtTo.Value);
            }
            
            // Apply No filter
            if (filter.No.HasValue)
            {
                predicate = predicate.And(x => x.No == filter.No.ToString());
            }
            
            // Use Firebase service to get notifications with the predicate
            var (notifications, totalCount) = await _firebaseService.GetNotificationsAsync(
                predicate, 
                filter.PageNumber, 
                filter.PageSize
            );
            
            // Map to response using mapper
            var notificationResponses = _mapper.Map<IEnumerable<GetAllNotificationResponse>>(notifications);
            
            return (notificationResponses, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notifications: {ex.Message}");
            throw new AppException("Error retrieving notifications");
        }
    }

    /// <summary>
    /// Create a new notification
    /// </summary>
    /// <param name="request">Notification data</param>
    /// <returns>Task</returns>
    public async Task CreateNotificationAsync(CommandNotificationRequest request)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Link = request.Link,
                Type = request.Type,
                No = request.No?.ToString(),
                RecipientId = request.RecipientId?.ToString(),
                CreatedAt = GlobalUtility.GetCurrentSEATime(),
                IsActive = true,
                IsRead = false
            };
            
            await _firebaseService.CreateNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating notification: {ex.Message}");
            throw new AppException("Error creating notification");
        }
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification details</returns>
    public async Task<GetAllNotificationResponse> GetNotificationByIdAsync(Guid id)
    {
        try
        {
            var notification = await _firebaseService.GetNotificationByIdAsync(id.ToString());
            
            // Mark notification as read
            await _firebaseService.UpdateNotificationReadStatusAsync(id.ToString(), true);
            notification.IsRead = true;
            
            // Map to response using mapper
            return _mapper.Map<GetAllNotificationResponse>(notification);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notification by ID: {ex.Message}");
            throw new AppException($"Error retrieving notification {id}");
        }
    }
}
