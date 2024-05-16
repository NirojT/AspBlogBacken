using Application.Blog;
using Domain.Blog.dto;
using Domain.Blog.entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Blog
{
    public class ReactService : IReactService
    {

        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;

        //constructor injection
        public ReactService(ApplicationDBContext dbContext, UserManager<User> userManager, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<string> createReact(ReactDto reactDto)
        {
            Task<User?> task = _userManager.FindByIdAsync(reactDto.userId);
          var user=  task.Result;
           Blogs blogs = null;
            Comment comment = null;


            if (reactDto.blogId != null)
            {
                blogs =   _dbContext.Blogs
         .Include(b => b.user) // Include the associated user
         .FirstOrDefault(b => b.id == reactDto.blogId);
            }
            if (reactDto.commentId != null)
            {


                comment =   _dbContext.Comments
       .Include(b => b.user) // Include the associated user
       .FirstOrDefault(c => c.id == reactDto.commentId);

            }

            
            if (user == null || (blogs == null && comment == null)) return null;

            
            React react = new React();
            react.id = Guid.NewGuid();

            react.type = reactDto.type;
            react.isInBlog = reactDto.isInBlog;
            
            react.user =  user;
            react.createdDate = DateTime.Now.ToUniversalTime();
            react.modifiedDate = DateTime.Now.ToUniversalTime();


            // For optional
            if (blogs != null) react.blogId = reactDto?.blogId;
            if (comment != null)   react.comment = comment;

            //////
            string message = "";
            if (blogs != null)
            {
                message = $"{user.UserName} has  {react.type}  in blog of title {blogs.title}";
                if (blogs.user != null && blogs.user.Id == user.Id)
                {
                    message = $"you have   {react.type}ed  in blog of title {blogs.title}";
                }
            }
            if (comment != null)
            {
                message = $"{user.UserName} has {react.type}ed in comment of {blogs.title}";
                if (blogs.user != null && blogs.user.Id == user.Id)
                {
                    message = $"You have {react.type}ed in comment of {blogs.title}";
                }

            }
            //save notification 
            await _notificationService.saveNotification(message, blogs.user.Id);
            /////

             
            await _dbContext.Reacts.AddAsync(react);
            await _dbContext.SaveChangesAsync();
            
            return message;
        }


        public async Task<bool> updateReact(ReactDto reactDto, Guid react_id)
        {
            //include user also while finding blogs
            var react = await _dbContext.Reacts.Include(b => b.user).FirstOrDefaultAsync(b => b.id == react_id);

            if (react == null) return false;

            //if user not found in react and also dont match than return false
            if (react.user != null &&  react.user.Id  != reactDto.userId) return false;


            //all good than update the value
            if (reactDto.type != null)  react.type = reactDto.type;
           

            


            react.modifiedDate = DateTime.Now.ToUniversalTime();
            await this._dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> deleteReact(ReactDto reactDto, Guid react_id)
        {
            //include user also while finding blogs
            var react = await _dbContext.Reacts.Include(b => b.user).FirstOrDefaultAsync(b => b.id == react_id);
            React reactWithComment = null;
            //if comment is present

            if (reactDto.commentId!=null)
            {
    reactWithComment = await _dbContext.Reacts
                .Include(b => b.user)
                .Include(b => b.comment)
                .FirstOrDefaultAsync(b => b.id == react_id);
            }
          

            if (react == null) return false;

            //if user not found in react and also dont match than return false
            if (react.user != null && react.user.Id != reactDto.userId) return false;

            Blogs blogs = null;
            Comment comment = null;
             //blog or comment may be null cuz they are optional
            if (reactDto.blogId != null)
            {
                blogs = await _dbContext.Blogs.FindAsync(reactDto.blogId);
            }
            if (reactDto.commentId != null)
            {
                comment = await _dbContext.Comments.FindAsync(reactDto.commentId);
            }


            //if react related to blogs
            if(blogs!=null && (react.blogId == blogs.id))
            {
                _dbContext.Reacts.Remove(react);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            //if react related to comment
            if (comment != null && reactWithComment != null && (reactWithComment.comment == comment))
            {
                _dbContext.Reacts.Remove(react);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;

        }

        public async Task<React> getReactById(ReactDto reactDto, Guid react_id)
        {
            //include user also while finding blogs
            return await _dbContext.Reacts.Include(b => b.user).FirstOrDefaultAsync(b => b.id == react_id);
             
        }

        public async Task<List<React>> getAllReacts()
        {
            return await _dbContext.Reacts.ToListAsync();
        }

        public async Task<object> getNoOfReact()
        {
            Dictionary <string, int> reactData=new Dictionary<string, int> ();
            

            //filtering data

              int reacts = await _dbContext.Reacts.CountAsync();
            int upVote = await _dbContext.Reacts.Where(blog => blog.type!=null &&
         blog.type.ToLower().Equals("upvote"))
           .CountAsync(); 
            
            int downVote = await _dbContext.Reacts.Where(blog => blog.type!=null &&
         blog.type.ToLower().Equals("downvote"))
           .CountAsync();

            //adding to map
            reactData.Add("reacts", reacts);
            reactData.Add("upvote", upVote);
            reactData.Add("downvote", downVote);
            return reactData;
        }

        public async Task<object> getNoOfReactByDate(string from, string to)
        {
            Dictionary<string, int> reactData = new Dictionary<string, int>();
            //changing string to DateTime type

            DateTime fromDate = DateTime.SpecifyKind(DateTime.Parse(from), DateTimeKind.Utc);
            DateTime toDate = DateTime.SpecifyKind(DateTime.Parse(to), DateTimeKind.Utc);



            int reacts = await _dbContext.Reacts
          .Where(blog => blog.createdDate >= fromDate && blog.modifiedDate <= toDate)
          .CountAsync();

            int upVote = await _dbContext.Reacts
                .Where(blog => blog.type != null && blog.type.ToLower() == "upvote" &&
                               blog.createdDate >= fromDate && blog.modifiedDate <= toDate)
                .CountAsync();

            int downVote = await _dbContext.Reacts
                .Where(blog => blog.type != null && blog.type.ToLower() == "downvote" &&
                               blog.createdDate >= fromDate && blog.modifiedDate <= toDate)
                .CountAsync();



            //adding to map
            reactData.Add("reacts", reacts);
            reactData.Add("upvote", upVote);
            reactData.Add("downvote", downVote);
            return reactData;
        }
    }
}
