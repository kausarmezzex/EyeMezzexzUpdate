using Microsoft.AspNetCore.Mvc;

namespace MezzexEye.Controllers
{
    public class NotificationController : Controller
    {
        /// <summary>
        /// Clears notification message and type from session.
        /// </summary>
        [HttpPost]
        public IActionResult ClearNotification()
        {
            HttpContext.Session.Remove("NotificationMessage");
            HttpContext.Session.Remove("NotificationType");
            return Json(new { success = true });
        }
    }
}
