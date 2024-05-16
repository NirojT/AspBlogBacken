using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.entity
{
    // Entity class representing a notification.
    public class Notification
    {
        // Unique identifier of the notification.
        [Key]
        public Guid Id { get; set; }

        // Message content of the notification.
        public string message { get; set; }

        // User ID to whom the notification is associated.
        public string user_Id { get; set; }

        // Date when the notification was created.
        public DateTime createdDate { get; set; }

        // Date when the notification was last modified.
        public DateTime modifiedDate { get; set; }
    }
}
