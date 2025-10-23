using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XONT.Ventura.ShellApp.DOMAIN;
using XONT.Ventura.ShellApp.BLL;
using XONT.Common.Message;

namespace XONT.Ventura.ShellApp.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IUserDAO _userManager;
        private readonly ILogger<MenuController> _logger;
        private string? userName => HttpContext.User?.Identity?.Name;

        private string? businessUnit => HttpContext.User?.FindFirst("BusinessUnit")?.Value;

        public MenuController(ILogger<MenuController> logger, IUserDAO userManager)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("user/role/{roleCode}")]
        public ActionResult<MenuHierarchyDto> GetUserMenu(
            string roleCode)
        {
            try
            {
                MessageSet? message = null;

                // Get menu using existing BLL
                var menuGroups = _userManager.GetUserManu(userName, roleCode, ref message);

                if (message != null)
                {
                    return BadRequest(new { message });
                }

                var hierarchy = new MenuHierarchyDto
                {
                    RoleCode = roleCode,
                    RoleName = roleCode,
                    IsPriorityRole = roleCode.StartsWith("PRTROLE"),
                    IsDefaultRole = false,
                    MenuGroups = new List<MenuGroupDto>(),
                    TotalTasks = 0
                };

                int order = 0;
                foreach (var menu in menuGroups ?? Enumerable.Empty<UserMenu>())
                {
                    // Get tasks for this menu
                    var tasks = _userManager.GetUserTask(menu.MenuCode, userName, ref message);
                    if (message != null)
                    {
                        return BadRequest(new { message });
                    }
                    var menuGroup = new MenuGroupDto
                    {
                        MenuCode = menu.MenuCode,
                        Description = menu.Description,
                        Icon = menu.Icon,
                        Order = order++,
                        IsExpanded = false,
                        IsVisible = true,
                        Tasks = tasks?.Select(t => new MenuTaskDto
                        {
                            TaskCode = t.TaskCode,
                            MenuCode = menu.MenuCode,
                            Caption = t.Caption,
                            Description = t.Description,
                            Icon = t.Icon,
                            Url = t.ExecutionScript,
                            TaskType = t.TaskType ?? "",
                            ApplicationCode = t.ApplicationCode ?? "",
                            ExclusivityMode = int.TryParse(t.ExclusivityMode, out var em) ? em : 0,
                            ExecutionType = 0,
                            Order = 0,
                            IsVisible = true,
                            IsFavorite = false
                        }).ToList() ?? new List<MenuTaskDto>()
                    };

                    hierarchy.MenuGroups.Add(menuGroup);
                    hierarchy.TotalTasks += menuGroup.Tasks.Count;
                }

                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load menu for user {UserName}, role {RoleCode}", userName, roleCode);
                return StatusCode(500, new { message = "Failed to load menu" });
            }
        }

        [HttpGet("system-tasks")]
        public ActionResult<List<SystemTaskDto>> GetSystemTasks()
        {
            // Return AUTOMENU and AUTODAILY tasks
            // This would require additional BLL method or database query
            return Ok(new List<SystemTaskDto>());
        }

        [HttpGet("check-daily-menu")]
        public ActionResult<object> CheckDailyMenu([FromQuery] string menuCode)
        {
            try
            {
                MessageSet? message = null;
                var available = _userManager.CheckDailyMenu(menuCode, ref message);

                return Ok(new { available });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check daily menu");
                return StatusCode(500, new { message = "Failed to check daily menu" });
            }
        }

        [HttpPost("update-daily-menu")]
        public IActionResult UpdateDailyMenu([FromBody] UpdateDailyMenuRequest request)
        {
            try
            {
                MessageSet? message = null;
                _userManager.UpdateDailyMenu(request.MenuCode, ref message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update daily menu");
                return StatusCode(500, new { message = "Failed to update daily menu" });
            }
        }
    }

    public class SystemTaskDto
    {
        public string TaskCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool ShouldAutoLoad { get; set; }
        public DateTime? LastExecuted { get; set; }
    }

    public class UpdateDailyMenuRequest
    {
        public string MenuCode { get; set; } = string.Empty;
    }
}
