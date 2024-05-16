using Domain.Blog.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Blog
{
    // Interface for managing comments on blogs.
    public interface ICommentService
    {
        // Creates a new comment on a blog.
        Task<string> createComment(Comment comment, Guid blog_id, Guid user_id);

        // Creates a new reply to a comment on a blog.
        Task<string> createCommentOfReply(Comment comment, Guid blog_id, string user_id);

        // Retrieves comments of a specific blog.
        Task<List<Comment>> getCommentsOfBlogs(Guid blog_id);

        // Updates an existing comment.
        Task<bool> updateComment(Comment comment, Guid comment_id, Guid blog_id, Guid user_id);

        // Retrieves all comments.
        Task<List<Comment>> getAllComments();

        // Retrieves a comment by its ID.
        Task<List<Comment>> getCommentById(Guid comment_id, Guid blog_id, Guid user_id);

        // Deletes a comment.
        Task<bool> deleteComment(Guid comment_id, Guid blog_id, Guid user_id);

        // Retrieves the total number of comments.
        Task<int> getNoOfComment();

        // Retrieves the number of comments within a specified date range.
        Task<int> getNoOfCommentByDate(string from, string to);
    }
}
