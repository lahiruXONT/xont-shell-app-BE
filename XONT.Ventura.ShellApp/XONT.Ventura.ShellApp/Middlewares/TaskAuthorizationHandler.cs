using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using XONT.Ventura.ShellApp.BLL;


namespace XONT.Ventura.ShellApp;
public class TaskAuthorizationRequirement : IAuthorizationRequirement { }

public class TaskAuthorizationHandler : AuthorizationHandler<TaskAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TaskAuthorizationRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            var taskid = httpContext.Request.RouteValues["taskid"]?.ToString();
            var controller = httpContext.Request.RouteValues["controller"]?.ToString();
            string? task = (taskid ?? controller)?.Trim();

            if (string.IsNullOrWhiteSpace(task))
            {
                context.Succeed(requirement);   
                return Task.CompletedTask;
            }

            var sessionManager = (SessionManager)httpContext.RequestServices.GetService(typeof(SessionManager));
            var session = sessionManager?.GetSession(httpContext.Session.Id);

            List<string> unAuthTaskList = session?.UnAuthorizedTasks ?? new List<string>();

            if (!unAuthTaskList.Any())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            else if (unAuthTaskList.Contains(task, StringComparer.OrdinalIgnoreCase))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            else
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        else
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
