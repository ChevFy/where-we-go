using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using where_we_go.DTO;

using System.Security.Claims;

using where_we_go.Service;

namespace where_we_go.Controllers
{
    [Authorize]
    public class NotificationController(INotificationService notificationService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] NotificationQueryDto query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var (notifications, unReadCount) = await notificationService.GetNotificationsByUserIdAsync(userId, query);
            ViewBag.UnReadCount = unReadCount;
            ViewBag.CurrentFilter = query.IsReadFilter;
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, string? returnFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await notificationService.UpdateNotificationReadStatusAsync(notificationId, userId, true);
            if (!result)
            {
                return NotFound();
            }

            // Redirect back to notifications with the same filter
            var query = new NotificationQueryDto();
            if (!string.IsNullOrEmpty(returnFilter))
            {
                if (returnFilter == "read") query.IsReadFilter = true;
                else if (returnFilter == "unread") query.IsReadFilter = false;
            }

            return RedirectToAction("Index", query);
        }
    }

}