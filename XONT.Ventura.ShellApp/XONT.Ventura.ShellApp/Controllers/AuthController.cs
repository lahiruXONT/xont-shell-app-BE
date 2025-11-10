using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
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
                var defaultRole = string.IsNullOrWhiteSpace(user.DefaultRoleCode) ? userRoles?.FirstOrDefault() : userRoles?.Where(x=>x.RoleCode==user.DefaultRoleCode).FirstOrDefault();
                var rolelist = userRoles.Select(r => r.RoleCode).ToList() ?? new List<string>();
                // Generate JWT tokens
                var token = _jwtTokenService.GenerateAccessToken(user.UserName, defaultBU, defaultRole?.RoleCode ??"", rolelist);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var unAuthorizedTasks = _userManager.GetUnAuthorizedTasksForUser(user.UserName, ref message);

                if (message!=null)
                {
                    return StatusCode(500, new { message});
                }
                // Create session
                var sessionId = Guid.NewGuid().ToString();
                _sessionManager.CreateSession(sessionId, user.UserName, defaultBU, unAuthorizedTasks);

                // Save login data using existing BLL
                user.SessionId = sessionId;
                user.WorkstationId = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                user.SuccessfulLogin = "1";

                var refreshTokenExpire = DateTime.UtcNow.AddDays(7);

                _userManager.SaveUserLoginData(user,refreshToken,refreshTokenExpire, ref message);

                // Map to DTO
                var response = new LoginResponse
                { Success = true,
                    Token = token,
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
                        CurrentRole = defaultRole!=null ? new RoleDto
                        {
                            RoleCode = defaultRole.RoleCode,
                            Description = defaultRole.Description,
                            Icon = defaultRole.Icon
                        }:null,
                        CurrentBusinessUnit = defaultBU,
                        MustChangePassword=false
                    }
                };
                Response.Cookies.Append("Host-token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshTokenExpire
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {UserName}", request.UserName);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpGet("refresh")]
        public ActionResult<LoginResponse> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["Host-token"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { message = "Refresh token missing" });
                }

                MessageSet? message = null;
                var(UserName, Password) = _userManager.ValidateRefreshToken(refreshToken,ref message);
                if (message!= null)
                {
                    return Unauthorized(new { message  });
                }
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var user = _userManager.GetUserInfo(UserName,Password, ref message);
                if (message != null || user == null || !user.isExists)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                // Check if user is active
                if (!user.ActiveFlag)
                {
                    return Unauthorized(new { message = "User account is disabled" });
                }

                // Get user roles using existing BLL
                var userRoles = _userManager.GetUserRoles(UserName, ref message);

                // Get default business unit
                var defaultBU = user.BusinessUnit;
                var defaultRole = string.IsNullOrWhiteSpace(user.DefaultRoleCode) ? userRoles?.FirstOrDefault() : userRoles?.Where(x => x.RoleCode == user.DefaultRoleCode).FirstOrDefault();
                var rolelist = userRoles.Select(r => r.RoleCode).ToList() ?? new List<string>();
                // Generate JWT tokens
                var token = _jwtTokenService.GenerateAccessToken(user.UserName, defaultBU, defaultRole?.RoleCode ??"", rolelist);
                var unAuthorizedTasks = _userManager.GetUnAuthorizedTasksForUser(user.UserName, ref message);

                if (message != null)
                {
                    return StatusCode(500, new { message });
                }
                // Map to DTO
                var response = new LoginResponse
                {
                    Success = true,
                    Token = token,
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
                        CurrentRole = defaultRole != null ? new RoleDto
                        {
                            RoleCode = defaultRole.RoleCode,
                            Description = defaultRole.Description,
                            Icon = defaultRole.Icon
                        } : null,
                        CurrentBusinessUnit = defaultBU,
                        MustChangePassword = false
                    }
                };
                return Ok(response);
               
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

            Response.Cookies.Delete("Host-token");
            // Get session ID from header or query
            var sessionId = HttpContext.Request.Headers["X-Session-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionManager.RemoveSession(sessionId);
            }

            return Ok(new { message = "Logged out successfully" });
        }
    }

}
