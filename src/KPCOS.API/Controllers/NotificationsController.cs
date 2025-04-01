using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Notifications;
using KPCOS.BusinessLayer.DTOs.Response.Notifications;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lấy danh sách thông báo")]
        public async Task<PagedApiResponse<GetAllNotificationResponse>> GetNotifications(
            [FromQuery] GetAllNotificationFilterRequest filter
        )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllNotificationResponse> notifications, int total) result;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var userId = Guid.Parse(userIdClaim);
                result = await _notificationService.GetAllNotificationAsync(filter, userId);
            }
            else
            {
                result = await _notificationService.GetAllNotificationAsync(filter);
            }
            
            return new PagedApiResponse<GetAllNotificationResponse>(
                result.notifications,
                filter.PageNumber,
                filter.PageSize,
                result.total
            );
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Tạo thông báo")]
        public async Task<ApiResult> CreateNotification(
            [FromBody] CommandNotificationRequest request
        )
        {
            await _notificationService.CreateNotificationAsync(request);
            return Ok();
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Lấy thông báo theo id")]
        public async Task<ApiResult<GetAllNotificationResponse>> GetNotificationById(
            [FromRoute] Guid id
        )
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            return Ok(notification);
        }
    }
}
