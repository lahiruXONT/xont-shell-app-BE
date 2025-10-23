using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.BLL;
using XONT.Ventura.ShellApp.Hubs;
using System.Security.Claims;
using XONT.Ventura.ShellApp.DOMAIN;

namespace XONT.Ventura.ShellApp.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IUserDAO _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        private string? userName => HttpContext.User?.Identity?.Name;

        private string? businessUnit => HttpContext.User?.FindFirst("BusinessUnit")?.Value;
        public NotificationsController(IUserDAO userManager, IHubContext<NotificationHub> hubContext)
        {
            _userManager = userManager;
            _hubContext = hubContext;
        }

        //[HttpGet]
        //public IActionResult GetNotifications()
        //{

        //    MessageSet? message = null;
        //    var notifications = _userManager.getUserNotification(userName, businessUnit, ref message);

        //    if (message != null)
        //        return BadRequest(new { message = message.Desc });

        //    return Ok(notifications);
        //}

        //[HttpPut("{id}")]
        //public IActionResult MarkAsRead(int id)
        //{
        //    MessageSet? message = null;
        //    _userManager.UpdateNotification(businessUnit, userName, id, 0, ref message);

        //    if (message != null)
        //        return BadRequest(new { message  });

        //    return Ok(new { message = "Notification marked as read" });
        //}

        //[HttpDelete("{id}")]
        //public IActionResult DeleteNotification(int id)
        //{
        //    MessageSet? message = null;
        //    _userManager.DeleteNotifications(userName, id, ref message);

        //    if (message != null)
        //        return BadRequest(new { message  });

        //    return Ok(new { message = "Notification deleted" });
        //}

        //[HttpPost("broadcast")]
        //public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastRequest request)
        //{
        //    await _hubContext.Clients.User(request.UserName).SendAsync("ReceiveNotification", request.Notification);
        //    return Ok();
        //}
    }

    public class BroadcastRequest
    {
        public string UserName { get; set; } = string.Empty;
        public object Notification { get; set; } = new();
    }
}
