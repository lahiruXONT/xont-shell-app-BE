using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.BLL;

namespace XONT.Ventura.ShellApp.Controllers
{
 
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly IUserDAO _userManager;
        private readonly string userName;
        private readonly string businessUnit;

        public FavoritesController(IUserDAO userManager)
        {
            _userManager = userManager;

            var httpContextUser = HttpContext.User;
            userName = httpContextUser?.Identity?.Name;
            var businessUnitClaim = httpContextUser?.FindFirst("BusinessUnit");
            businessUnit = businessUnitClaim?.Value;
        }

        //[HttpGet]
        //public IActionResult GetFavorites()
        //{
        //    MessageSet? message = null;
        //    var favorites = _userManager.getUserfavourites(businessUnit, userName, ref message);

        //    if (message != null)
        //        return BadRequest(new { message });

        //    return Ok(favorites);
        //}

        //[HttpPost]
        //public IActionResult AddFavorite([FromBody] AddFavoriteRequest request)
        //{
        //    MessageSet? message = null;
        //    _userManager.saveFavourites(
        //        businessUnit,
        //        userName,
        //        Guid.NewGuid().ToString(),
        //        request.BookmarkName,
        //        request.Path,
        //        ref message
        //    );

        //    if (message != null)
        //        return BadRequest(new {  message });

        //    return Ok(new { message = "Favorite added successfully" });
        //}

        //[HttpDelete("{bookmarkId}")]
        //public IActionResult DeleteFavorite(string bookmarkId)
        //{
        //    MessageSet? message = null;
        //    _userManager.DeleteBookmarks(userName, bookmarkId, ref message);

        //    if (message != null)
        //        return BadRequest(new { message });

        //    return Ok(new { message = "Favorite deleted successfully" });
        //}
    }

    public class AddFavoriteRequest
    {
        public string BookmarkName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string TaskCode { get; set; } = string.Empty;
    }
}
