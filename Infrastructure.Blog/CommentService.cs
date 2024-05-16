using Application.Blog;
using AutoMapper;
using Domain.Blog.dto;
using Domain.Blog.entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastructure.Blog
{
    public class CommentService : ICommentService
    {

        private readonly ApplicationDBContext _dbContext;
        private readonly INotificationService _notificationService;
      

        //constructor injection
        public CommentService(ApplicationDBContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            
        }

        public async Task<string> createComment(Comment comment, Guid blog_id, Guid user_id)
        {
            // Finding blogs
            Blogs? blogs = await _dbContext.Blogs
           .Include(b => b.user)  
           .FirstOrDefaultAsync(b => b.id == blog_id);
            // Finding user
            User? user = await _dbContext.Users.FindAsync(user_id.ToString());

            // Return false if nothing found
            if (user == null || blogs == null)
            {
                return null;
            }
            
            comment.id = Guid.NewGuid();
            comment.blogs = blogs;
            comment.user = user;
           
            comment.createdDate = DateTime.Now.ToUniversalTime();
            comment.modifiedDate = DateTime.Now.ToUniversalTime();

            ////////
             
 

            string message = $"{user.UserName} has commented on {blogs.title} comment as {comment.content}";
            if (blogs.user != null && blogs.user.Id == user.Id)
            {
                message = $"You have commented on your {comment.content} comment";
            }

            //save notification 
            await _notificationService.saveNotification(message, blogs.user.Id);
            
            ////////

 
            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
          
            return message;
        }

        public async Task<bool> updateComment(Comment commented, Guid comment_id, Guid blog_id, Guid user_id)
        {
            //finding comment


            var comment = await _dbContext.Comments
           .Include(c => c.user)
           .Include(c => c.blogs)
           .FirstOrDefaultAsync(c => c.id == comment_id);
            //if comment did not found return
            if (comment == null) return false;

            //if comment found but user or blogs missing
            if (comment.blogs == null || comment.user == null) return false;

            //if found
            comment.content = commented.content;
            comment.modifiedDate= DateTime.Now.ToUniversalTime();
 
            await this._dbContext.SaveChangesAsync();
            return true;

        }

        public async Task<bool> deleteComment(Guid comment_id, Guid blog_id, Guid user_id)
        {
            //finding comment
            var comment = await _dbContext.Comments
            .Include(c => c.user)
            .Include(c => c.blogs)
            .FirstOrDefaultAsync(c => c.id == comment_id);

           
            var reacts = _dbContext.Reacts.Include(b => b.comment).Where(r => r.comment.id == comment.id);
            _dbContext.Reacts.RemoveRange(reacts);
            await _dbContext.SaveChangesAsync();

            //if comment did not found return
            if (comment == null) return false;

            //if comment found but user or blogs missing
            if (comment.blogs == null || comment.user == null) return false;

            //if all found but ids not matching
            if (comment.blogs.id != blog_id || comment.user.Id != user_id.ToString()) return false;



            //if all good
              this._dbContext.Remove(comment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Comment>> getAllComments()
        {
            return await _dbContext.Comments
        .ToListAsync();
        }

        public async Task<List<Comment>> getCommentById(Guid comment_id, Guid blog_id, Guid user_id)
        {
            //finding comment

            return await _dbContext.Comments
           .Include(c => c.user)
            .Include(c => c.blogs)
           .Where(c => (c.id == comment_id || c.referenceCommentId == comment_id.ToString()) && c.blogs.id == blog_id)
           .ToListAsync();

             
        }

         

        

        public async Task<string> createCommentOfReply(Comment comment, Guid blog_id, string user_id)
        {
              // Finding parent comment
  Comment parentComment = await _dbContext.Comments.Include(b => b.user).FirstOrDefaultAsync(c=>c.id== comment.id);
            // Finding blog
            Blogs blog = blog = await _dbContext.Blogs
        .Include(b => b.user) // Include the associated User
        .FirstOrDefaultAsync(b => b.id == blog_id);
            // Finding user
            User user = await _dbContext.Users.FindAsync(user_id);

            // Return false if nothing found
            if (parentComment == null || blog == null || user == null)
            {
                return null;
            }

            // If found

            comment.blogs = blog;
            comment.user = user;
            comment.referenceCommentId = comment.id.ToString();  
            comment.id = Guid.NewGuid();


            comment.createdDate = DateTime.Now.ToUniversalTime();
            comment.modifiedDate = DateTime.Now.ToUniversalTime();


            ////////
            ///
            string message = "";
            if (comment.user.Id == user.Id) message = $"You have replied to your own comment: '{comment.content}'.";

            else message = $"{user.UserName} has replied to the comment: '{comment.content}'.";
      


        //save notification 
        await _notificationService.saveNotification(message, comment.user.Id);

            ////////

             

            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public async Task<List<Comment>> getCommentsOfBlogs(Guid blog_id)
        {
           var comments = await _dbContext.Comments
                .Where(c => c.blogs.id == blog_id)
                .Select(c => new Comment
                {
                   
                    id = c.id,
                    content = c.content,
                    referenceCommentId = c.referenceCommentId,
                    // Include necessary navigation properties
                    user = c.user,
                    reacts = c.reacts // Include Reacts if necessary
                })
                .ToListAsync();

            foreach (var comment in comments)
            {
                var reacts = await _dbContext.Reacts
                    .Where(r => r.comment.id == comment.id)

                    .ToListAsync();


                comment.reacts = reacts;

            }

            return comments;
        }

        public async Task<int> getNoOfComment()
        {
           return await _dbContext.Comments.CountAsync();
        }

        public async Task<int> getNoOfCommentByDate(string from, string to)
        {
            // Changing string to DateTime type
           DateTime fromDate = DateTime.SpecifyKind(DateTime.Parse(from), DateTimeKind.Utc);
            DateTime toDate = DateTime.SpecifyKind(DateTime.Parse(to), DateTimeKind.Utc);




        return await _dbContext.Comments.Where(comment =>
        comment.createdDate >= fromDate && comment.modifiedDate
                <= toDate)  .CountAsync();
        }



 

    }
}
