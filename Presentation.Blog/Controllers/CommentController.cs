using Application.Blog;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Presentation.Blog.utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Blog.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> _logger;
        private readonly ICommentService _commentService;
        private readonly IHubContext<SignalRNoti> _signalRNoti;

        // Constructor injection
        public CommentController(ILogger<CommentController> logger, ICommentService commentService, IHubContext<SignalRNoti> signalRNoti)
        {
            _logger = logger;
            _commentService = commentService;
            _signalRNoti = signalRNoti;
        }

        // Handle server error responses
        private IActionResult serverError(Exception ex)
        {
            return BadRequest(new { status = 500, message = "in Comment error is " + ex.Message });
        }

        // Create a new comment for a blog
        [HttpPost("create/{blog_id}/{user_id}")]
        public async Task<IActionResult> createComment([FromBody] Comment comment, Guid blog_id, Guid user_id)
        {
            try
            {
                // Create the comment and get the result
                Task<string> isCreated = _commentService.createComment(comment, blog_id, user_id);
                string result = isCreated.Result;

                // Send notification if the comment is created successfully
                if (result != null) await _signalRNoti.Clients.All.SendAsync("notis", result);

                // Return appropriate response based on the result
                return result != null
                    ? Ok(new { status = 200, message = "Comment created successfully" })
                    : BadRequest(new { status = 400, message = "Failed to create Comment" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Update an existing comment
        [HttpPut("update/{comment_id}/{blog_id}/{user_id}")]
        public IActionResult updateComment([FromBody] Comment Comment, Guid comment_id, Guid blog_id, Guid user_id)
        {
            try
            {
                // Update the comment and get the result
                Task<bool> isUpdated = _commentService.updateComment(Comment, comment_id, blog_id, user_id);
                bool result = isUpdated.Result;

                // Return appropriate response based on the result
                return result
                    ? Ok(new { status = 200, message = "Comment updated successfully" })
                    : BadRequest(new { status = 400, message = "Failed to update Comment" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Create a reply comment for a blog
        [HttpPost("reply/create/{blog_id}/{user_id}")]
        public async Task<IActionResult> createReplyComment([FromBody] Comment comment, Guid blog_id, string user_id)
        {
            try
            {
                // Create the reply comment and get the result
                Task<string> task = _commentService.createCommentOfReply(comment, blog_id, user_id);
                string res = task.Result;

                // Send notification if the reply comment is created successfully
                if (res != null) await _signalRNoti.Clients.All.SendAsync("notis", res);

                // Return appropriate response based on the result
                return res != null
                    ? Ok(new { status = 200, message = "Reply Comment created successfully" })
                    : BadRequest(new { status = 400, message = "Failed to create Reply Comment" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Retrieve all comments
        [HttpGet, Route("get-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllcomment()
        {
            try
            {
                // Get all comments
                List<Comment> comment = await _commentService.getAllComments();

                // Return appropriate response based on the result
                return base.Ok(new
                {
                    status = comment.Count > 0 ? 200 : 400,
                    data = comment.Count > 0 ? comment : new List<Comment>()
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Retrieve comments of a specific blog
        [HttpGet("getCommentofBlog/{blog_id}")]
        public async Task<IActionResult> GetCommentofBlog(Guid blog_id)
        {
            try
            {
                // Get comments of the specified blog
                List<Comment> comment = await _commentService.getCommentsOfBlogs(blog_id);

                // Return appropriate response based on the result
                return comment != null
                    ? base.Ok(new { status = 200, data = comment })
                    : base.BadRequest(new { status = 400, data = new List<Comment>() });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return BadRequest(new { error = ex.Message });
            }
        }

        // Retrieve a comment by its ID
        [HttpGet("get/{comment_id}/{blog_id}/{user_id}")]
        public IActionResult getCommentbyId(Guid comment_id, Guid blog_id, Guid user_id)
        {
            try
            {
                // Get the comment by its ID
                Task<List<Comment>> task = _commentService.getCommentById(comment_id, blog_id, user_id);
                List<Comment> comment = task.Result;

                // Return appropriate response based on the result
                return comment != null
                    ? base.Ok(new { status = 200, data = comment })
                    : base.BadRequest(new { status = 400, data = new List<Comment>() });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Delete a comment
        [HttpDelete("delete/{comment_id}/{blog_id}/{user_id}")]
        public IActionResult deleteComment(Guid comment_id, Guid blog_id, Guid user_id)
        {
            try
            {
                // Delete the comment and get the result
                Task<bool> isDeleted = _commentService.deleteComment(comment_id, blog_id, user_id);
                bool result = isDeleted.Result;

                // Return appropriate response based on the result
                return result
                    ? Ok(new { status = 200, message = "Comment deleted successfully" })
                    : BadRequest(new { status = 400, message = "Failed to delete Comment" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Retrieve the count of all comments
        [HttpGet("get-count")]
        public IActionResult getCommentCounts()
        {
            try
            {
                // Get the count of all comments
                Task<int> commentCountTask = _commentService.getNoOfComment();
                int count = commentCountTask.Result;

                // Return the count along with a success response
                return base.Ok(new { status = 200, data = count, message = "Comments found" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }

        // Retrieve the count of comments within a date range
        [HttpGet("get-countByDate")]
        public IActionResult getCommentCountsWithDate(string from, string to)
        {
            try
            {
                // Get the count of comments within the specified date range
                Task<int> commentCountTask = _commentService.getNoOfCommentByDate(from, to);
                int count = commentCountTask.Result;

                // Return the count along with a success response
                return base.Ok(new { status = 200, data = count, message = "Comments found" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }
    }
}
