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

        /*
        [HttpPut("{id}")]
        public async Task<ApiResult> UpdateNotificationRead(Guid id)
        {
            await _notificationService.UpdateNotificationRead(id);
            return Ok();
        }
        */
    }
}
