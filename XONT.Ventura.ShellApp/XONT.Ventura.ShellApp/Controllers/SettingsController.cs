using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.BLL;
using XONT.Ventura.ShellApp.DOMAIN;

namespace XONT.Ventura.ShellApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IUserDAO _userManager;

        private string? userName => HttpContext.User?.Identity?.Name;

        private string? businessUnit => HttpContext.User?.FindFirst("BusinessUnit")?.Value;

        public SettingsController(IUserDAO userManager)
        {
            _userManager = userManager;
        }

        //[HttpGet("settings")]
        //public IActionResult GetSettings()
        //{
        //    MessageSet? message = null;
        //    var user = _userManager.GetUserInfo(userName, "", ref message);

        //    if (message != null)
        //        return BadRequest(new { message  });

        //    return Ok(new
        //    {
        //        theme = user.Theme,
        //        language = user.Language,
        //        fontFamily = user.FontName,
        //        fontSize = user.FontSize,
        //        fontColor = user.FontColor
        //    });
        //}

        //[HttpPut()]
        //public IActionResult UpdateSettings( [FromBody] UpdateSettingsRequest request)
        //{
        //    MessageSet? message = null;
        //    var user = new User
        //    {
        //        UserName = userName,
        //        Theme = request.Theme,
        //        Language = request.Language,
        //        FontName = request.FontFamily,
        //        FontSize = request.FontSize,
        //        FontColor = request.FontColor
        //    };

        //    _userManager.UpdateUserSetting(user, ref message);

        //    if (message != null)
        //        return BadRequest(new { message  });

        //    return Ok(new { success = true, message = "Settings updated successfully" });
        //}

        //[HttpPost("change-password")]
        //public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        //{
        //    MessageSet? message = null;
        //    _userManager.SaveChangePassword(userName, request.CurrentPassword, request.NewPassword, ref message);

        //    if (message != null)
        //        return BadRequest(new { success = false, message  });

        //    return Ok(new { success = true, message = "Password changed successfully" });
        //}

        //[HttpPost("upload-profile-image")]
        //public IActionResult UploadProfileImage( [FromBody] ProfileImageRequest request)
        //{
        //    MessageSet? message = null;
        //    byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);
        //    _userManager.saveProfilePicture(userName, imageBytes, ref message);

        //    if (message != null)
        //        return BadRequest(new { success = false, message  });

        //    return Ok(new { success = true, message = "Profile image uploaded successfully" });
        //}

        //[HttpPost("reset-defaults")]
        //public IActionResult ResetToDefaults()
        //{
        //    MessageSet? message = null;
        //    var user = new User
        //    {
        //        UserName = userName,
        //        Theme = "blue",
        //        Language = "en",
        //        FontName = "Arial",
        //        FontSize = 14,
        //        FontColor = "#000000"
        //    };

        //    _userManager.UpdateUserSetting(user, ref message);

        //    if (message != null)
        //        return BadRequest(new { success = false, message  });

        //    return Ok(new { success = true, message = "Settings reset to defaults" });
        //}
    }

    public class UpdateSettingsRequest
    {
        public string Theme { get; set; } = "blue";
        public string Language { get; set; } = "en";
        public string FontFamily { get; set; } = "Arial";
        public int FontSize { get; set; } = 14;
        public string FontColor { get; set; } = "#000000";
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ProfileImageRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
    }
}
