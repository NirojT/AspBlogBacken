using Domain.Blog;
using Domain.Blog.dto;
using Domain.Blog.entity;
using System.Numerics;

namespace Application.Blog
{
    // Interface for managing blog-related operations.
    public interface IBlogsService
    {
        // Creates a new blog.
        Task<bool> createBlogs(BlogsDto blogsDto, Guid user_id);

        // Updates an existing blog.
        Task<bool> updateBlogs(BlogsDto blogsDto, Guid blog_id, Guid user_id);

        // Retrieves all blogs.
        Task<List<BlogsDto>> getAllBlogs();

        // Retrieves a specific blog by its ID.
        Task<BlogsDto> getBlogsById(Guid blog_id, Guid user_id);

        // Deletes a blog.
        Task<bool> deleteBlogs(Guid blog_id, Guid user_id);

        // Retrieves the total number of blogs.
        Task<int> getNoOfBlog();

        // Retrieves the number of blogs within a specified date range.
        Task<int> getNoOfBlogByDate(string from, string to);

        // Retrieves the top 10 blogs and their bloggers.
        Task<Dictionary<string, object>> getTop10BlogsAndBloggers();

        // Retrieves a paginated list of blogs.
        Task<List<BlogsDto>> getPaginatedBlogs(int pageSize, int pageNumber);

        // Retrieves blogs authored by a specific user.
        Task<List<BlogsDto>> getBlogofUser(string userId);

        // Retrieves the most recent blogs.
        Task<List<BlogsDto>> getRecentBlog();

        // Searches for blogs based on the provided title.
        Task<List<BlogsDto>> searchBlog(string title);
    }
}
