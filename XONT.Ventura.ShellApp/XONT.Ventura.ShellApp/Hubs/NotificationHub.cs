using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace XONT.Ventura.ShellApp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userId))
            {
                ConnectedUsers[Context.ConnectionId] = userName;

                var userRoles = Context.User?.FindAll(ClaimTypes.Role); 
                if (userRoles != null)
                {
                    foreach (var claim in userRoles)
                    {
                        var roleCode = claim.Value;
                        if (!string.IsNullOrEmpty(roleCode))
                        {
                            try
                            {
                                await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{roleCode}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error adding user {userName} to role group 'role_{roleCode}': {ex.Message}");
                            }
                        }
                    }
                }

                await Clients.Caller.SendAsync("Connected", new { message = "Connected to notification hub" });
            }
            else
            {
                Console.WriteLine($"Connection {Context.ConnectionId} authenticated but missing Name or NameIdentifier claim.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUsers.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userName, object notification)
        {
            await Clients.User(userName).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToRole(string roleCode, object notification)
        {
            await Clients.Group($"role_{roleCode}").SendAsync("ReceiveNotification", notification);
        }

        public async Task BroadcastToAll(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        // Optional: Add a method for clients to request joining a specific group (e.g., for dynamic filtering)
        // Use with caution and implement server-side validation.
        // public async Task JoinGroup(string groupName)
        // {
        //     // Example: Validate groupName format or check if user is allowed
        //     // if (IsUserAllowedToJoinGroup(Context.User, groupName))
        //     // {
        //         await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //     // }
        // }

        // Optional: Add a method for clients to request leaving a specific group
        // public async Task LeaveGroup(string groupName)
        // {
        //     await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        // }
    }
}