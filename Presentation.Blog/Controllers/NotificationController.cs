using Application.Blog;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Blog.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;

        // Constructor injection to inject required services.
        public NotificationController(ILogger<NotificationController> logger,
             INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        // Method to handle server errors and return standardized error response.
        private IActionResult ServerError(Exception ex)
        {
            return BadRequest(new { status = 500, message = "An error occurred in NotificationController: " + ex.Message });
        }

        // API endpoint to retrieve notifications for a specific user.
        [HttpGet("getNotification/{user_id}")]
        public async Task<IActionResult> GetNotificationOfUser(string user_id)
        {
            try
            {
                // Retrieve notifications for the specified user.
                List<Notification> notifications = _notificationService.getAllNotificationsOfUser(user_id);

                // Return the notifications if any, else return an empty list.
                return notifications.Count > 0 ? Ok(new
                {
                    status = 200,
                    data = notifications
                }) :
                BadRequest(new
                {
                    status = 400,
                    data = new List<Notification>()
                });
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions.
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}