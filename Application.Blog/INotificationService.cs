using Domain.Blog.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Blog
{
    // Interface for managing notifications.
    public interface INotificationService
    {
        // Saves a notification for a user.
        Task<bool> saveNotification(string message, string user_id);

        // Retrieves all notifications of a user.
        List<Notification> getAllNotificationsOfUser(string user_id);
    }
}
