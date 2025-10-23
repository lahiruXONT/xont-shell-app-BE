using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using XONT.Common.Data;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.BLL;
using XONT.Ventura.ShellApp.DOMAIN;

namespace XONT.Ventura.ShellApp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserDAO _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly SessionManager _sessionManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            JwtTokenService jwtTokenService,
            SessionManager sessionManager,
            ILogger<AuthController> logger,IUserDAO userManager)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _sessionManager = sessionManager;
            _logger = logger;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            try
            {
                MessageSet? message = null;

                // Use existing BLL to validate user
                string encryptedPassword = new StroEncript().Encript(request.Password.Trim());
                var user = _userManager.GetUserInfo(request.UserName, encryptedPassword, ref message);

                if (message != null || user == null || !user.isExists)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Check if user is active
                if (!user.ActiveFlag)
                {
                    return Unauthorized(new { message = "User account is disabled" });
                }

                // Get user roles using existing BLL
                var userRoles = _userManager.GetUserRoles(request.UserName, ref message);

                // Get default business unit
                var defaultBU = user.BusinessUnit;
                var defaultRole = user.DefaultRoleCode ?? (userRoles?.FirstOrDefault()?.RoleCode ?? "");
                var rolelist = userRoles.Select(r => r.RoleCode).ToList() ?? new List<string>();
                // Generate JWT tokens
                var token = _jwtTokenService.GenerateAccessToken(user.UserName, defaultBU, defaultRole, rolelist);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Create session
                var sessionId = Guid.NewGuid().ToString();
                _sessionManager.CreateSession(sessionId, user.UserName, defaultBU);

                // Save login data using existing BLL
                user.SessionId = sessionId;
                user.WorkstationId = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                user.SuccessfulLogin = "1";
                _userManager.SaveUserLoginData(user, ref message);

                // Map to DTO
                var response = new LoginResponse
                { Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        UserId = user.UserName,
                        UserName = user.UserName,
                        FullName = user.UserFullName,
                        Email = user.Email ?? "",
                        ProfileImage = user.HasProPicture == '1' ? $"/api/user/profile-image/{user.UserName}" : "",
                        Roles = userRoles?.Select(r => new RoleDto
                        {
                            RoleCode = r.RoleCode,
                            Description = r.Description,
                            Icon = r.Icon
                        }).ToList() ?? new List<RoleDto>(),
                        CurrentRole = defaultRole,
                        CurrentBusinessUnit = defaultBU,
                        MustChangePassword=false
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {UserName}", request.UserName);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("refresh")]
        public ActionResult<LoginResponse> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
                if (principal == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var userName = principal.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Generate new tokens
                var businessUnit = principal.FindFirst("BusinessUnit")?.Value ?? "";
                var roleCode = principal.FindFirst("RoleCode")?.Value ?? "";
                var userRoles = principal.FindAll(ClaimTypes.Role).Select(c=>c.Value).ToList();
                var newToken = _jwtTokenService.GenerateAccessToken(userName, businessUnit, roleCode, userRoles);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                return Ok(new
                {
                    token = newToken,
                    refreshToken = newRefreshToken,
                    expiresAt = DateTime.UtcNow.AddMinutes(60)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Get session ID from header or query
            var sessionId = HttpContext.Request.Headers["X-Session-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionManager.RemoveSession(sessionId);
            }

            return Ok(new { message = "Logged out successfully" });
        }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
