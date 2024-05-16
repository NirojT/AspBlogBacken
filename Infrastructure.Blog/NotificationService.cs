using Application.Blog;
using Domain.Blog.entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Blog
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDBContext _dbContext;
        

        //constructor injection
        public NotificationService(ApplicationDBContext dbContext )
        {
            _dbContext = dbContext;
          
        }
        public    List<Notification> getAllNotificationsOfUser(string user_id)
        {
            return  _dbContext.Notifications.Where(n => n.user_Id == user_id)
                     .OrderByDescending(b => b.createdDate)
                     .ToList();
        }

        public async Task<bool> saveNotification(string message,string user_id)
        {
            Notification notification = new Notification();
            notification.message= message;
            notification.user_Id= user_id;
            notification.Id = Guid.NewGuid();
            notification.createdDate = DateTime.Now.ToUniversalTime();
            notification.modifiedDate = DateTime.Now.ToUniversalTime();
           
            await  _dbContext.Notifications.AddAsync( notification );
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
