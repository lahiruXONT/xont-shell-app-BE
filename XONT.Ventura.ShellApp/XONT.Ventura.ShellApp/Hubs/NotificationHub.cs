using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace XONT.Ventura.ShellApp.Hubs
{
    [Authorize]
    public class NotificationHub: Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userName))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, userName);

                _logger.LogInformation(
                    "User {UserName} connected to notification hub. ConnectionId: {ConnectionId}",
                    userName,
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userName))
            {
                _logger.LogInformation(
                    "User {UserName} disconnected from notification hub",
                    userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        public async Task SendToUser(string userName, string title, string message)
        {
            await Clients.Group(userName).SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send notification to all users in a role
        /// </summary>
        public async Task SendToRole(string roleCode, string title, string message)
        {
            await Clients.Group($"role_{roleCode}").SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast to all connected users
        /// </summary>
        public async Task BroadcastNotification(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
