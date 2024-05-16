using Application.Blog;
using Domain.Blog;
using Domain.Blog.dto;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Blog.utility;
using System.Threading.Tasks;

namespace Presentation.Blog.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class BlogController : ControllerBase
    {

        private readonly ILogger<BlogController> _logger;

        private readonly IBlogsService _blogService;
        private readonly IWebHostEnvironment _hostingEnvironment;





        // Constructor for the BlogController class, responsible for initializing required services.
        public BlogController(ILogger<BlogController> logger,
             IBlogsService blogService, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;  
            _blogService = blogService; 
            _hostingEnvironment = hostingEnvironment;
        }

        // Method to handle server errors and return a standardized error response.
        private IActionResult serverError(Exception ex)
        {
            return BadRequest(new { status = 500, message = "An error occurred in BlogController: " + ex.Message });
        }

        // HTTP POST endpoint to create a new blog.
        [HttpPost("create/{user_id}")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogsDto blogs, Guid user_id)
        {
            try
            {
                // Check if the image is provided.
                if (blogs?.image == null)
                    return BadRequest(new { status = 400, message = "Image cannot be null" });

                // Check if the image size exceeds the maximum allowed size (3 MB).
                if (blogs?.image.Length > 3 * 1024 * 1024)
                {
                    return BadRequest(new { status = 400, error = "Image size is more than 3 MB" });
                }

                // Save the uploaded image.
                string imageName = await FileHelper.SaveImage(blogs.image, _hostingEnvironment);
                blogs.imageName = imageName;

                // Create the blog using the provided blog service.
                bool isCreated = await _blogService.createBlogs(blogs, user_id);

                // Return success or failure message based on the result of blog creation.
                return isCreated
                    ? Ok(new { status = 200, message = "Blog created successfully" })
                    : BadRequest(new { status = 400, message = "Failed to create blog" });
            }
            catch (Exception ex)
            {
                // Call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }

        // HTTP GET endpoint to search for blogs by title.
        [HttpGet("search")]
        public IActionResult SearchBlog(string title)
        {
            try
            {
                // Call the searchBlog method of the blog service to search for blogs based on the provided title.
                Task<List<BlogsDto>> task = _blogService.searchBlog(title);
                List<BlogsDto> blogs = task.Result;

                // Return a success response with the retrieved blogs if any, otherwise return an empty list.
                return blogs.Count > 0
                    ? Ok(new { status = 200, data = blogs })
                    : BadRequest(new { status = 400, data = new List<BlogsDto>() });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the search operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }

        // HTTP PUT endpoint to update a blog.
        [HttpPut("update/{blog_id}/{user_id}")]
        public async Task<IActionResult> UpdateBlog([FromForm] BlogsDto blogsDto, Guid blog_id, Guid user_id)
        {
            try
            {
                // Check if a new image is provided for the blog update.
                if (blogsDto.image != null)
                {
                    // Checking the size of the image (in bytes).
                    if (blogsDto?.image.Length > 3 * 1024 * 1024)
                    {
                        // Return a bad request response if the image size exceeds the maximum allowed size.
                        return BadRequest(new { status = 400, error = "Image size is more than 3 MB" });
                    }

                    // Save the new image and update the image name in the blogsDto.
                    string imageName = await FileHelper.SaveImage(blogsDto.image, _hostingEnvironment);
                    blogsDto.imageName = imageName;
                }

                // Update the blog using the provided blog service.
                Task<bool> isUpdated = _blogService.updateBlogs(blogsDto, blog_id, user_id);
                bool result = isUpdated.Result;

                // Return success or failure message based on the result of the blog update.
                return result
                    ? Ok(new { status = 200, message = "Blog updated successfully" })
                    : BadRequest(new { status = 400, message = "Failed to update blog" });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the update operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }




        // HTTP GET endpoint to retrieve all blogs.
        [HttpGet, Route("get-all")]
        [AllowAnonymous] // Allows all users to view blogs.

        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                // Retrieve all blogs using the blog service.
                List<BlogsDto> blogs = await _blogService.getAllBlogs();

                // Return a response containing the retrieved blogs.
                return Ok(new
                {
                    status = blogs.Count > 0 ? 200 : 400, // Determine the status based on the presence of blogs.
                    data = blogs.Count > 0 ? blogs : new List<BlogsDto>() // Return the blogs if present, otherwise return an empty list.
                });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }



        // HTTP GET endpoint to retrieve a blog by its ID.
        [HttpGet("get/{blog_id}/{user_id}")]
        public IActionResult GetBlogById(Guid blog_id, Guid user_id)
        {
            try
            {
                // Retrieve the blog with the specified ID using the blog service.
                Task<BlogsDto> blog = _blogService.getBlogsById(blog_id, user_id);
                BlogsDto blogDto = blog.Result;

                // Check if the retrieved blog is not null and return it if found, otherwise return an empty blog DTO.
                return blogDto != null
                    ? Ok(new { status = 200, data = blogDto })
                    : BadRequest(new { status = 400, data = new BlogsDto() });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }


        // HTTP GET endpoint to retrieve all blogs paginated.
        [HttpGet("getBlogPagination")]
        public async Task<IActionResult> GetAllBlogsPaginated(int pageSize, int pageNumber)
        {
            try
            {
                // Retrieve paginated blogs using the blog service.
                List<BlogsDto> blogsDtos = await _blogService.getPaginatedBlogs(pageSize, pageNumber);

                // Retrieve the total number of blogs for pagination purposes.
                List<BlogsDto> sizePurpose = await _blogService.getAllBlogs();

                // Return paginated blogs along with the total number of blogs.
                return Ok(new
                {
                    data = blogsDtos, // Paginated blogs.
                    length = sizePurpose.Count // Total number of blogs.
                });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }

        // HTTP GET endpoint to retrieve blogs of a specific user.
        [HttpGet("blogOfUser/{user_id}")]
        public async Task<IActionResult> GetBlogsOfUser(string user_id)
        {
            _logger.LogInformation("blogofuser");
            try
            {
                // Retrieve blogs of the specified user using the blog service.
                List<BlogsDto> blogs = await _blogService.getBlogofUser(user_id);

                // Return the blogs of the user.
                return Ok(new
                {
                    data = blogs.ToList() // Convert the list of blogs to a List object for serialization.
                });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, return a bad request response with the error message.
                return BadRequest(new { error = ex.Message });
            }
        }



        // HTTP GET endpoint to retrieve popular blogs.
        [HttpGet("getBlogPopular")]
        public async Task<IActionResult> GetPopularBlog()
        {
            try
            {
                // Retrieve the top 10 popular blogs and bloggers using the blog service.
                Task<Dictionary<string, object>> task = _blogService.getTop10BlogsAndBloggers();
                Dictionary<string, object> blog = task.Result;

                // Return the retrieved popular blogs.
                return Ok(new
                {
                    status = 200, // Success status code.
                    data = blog // Popular blogs data.
                });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }


        // HTTP GET endpoint to retrieve recent blogs.
        [HttpGet("getRecentBlog")]
        public async Task<IActionResult> GetRecentBlog()
        {
            try
            {
                // Retrieve recent blogs using the blog service.
                Task<List<BlogsDto>> task = _blogService.getRecentBlog();
                List<BlogsDto> blogs = task.Result;

                // Return the retrieved recent blogs.
                return Ok(new
                {
                    status = 200, // Success status code.
                    data = blogs.ToList() // Convert the list of recent blogs to a List object for serialization.
                });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the retrieval operation, return a bad request response with the error message.
                return BadRequest(new { error = ex.Message });
            }
        }








        // HTTP DELETE endpoint to delete a blog.
        [HttpDelete("delete/{blog_id}/{user_id}")]
        public IActionResult DeleteBlog(Guid blog_id, Guid user_id)
        {
            try
            {
                // Delete the blog using the blog service.
                Task<bool> isDeleted = _blogService.deleteBlogs(blog_id, user_id);
                bool result = isDeleted.Result;

                // Return the appropriate response based on the deletion result.
                return result
          ? Ok(new { status = 200, message = "Blog deleted successfully" }) // If the blog is successfully deleted, return a success response.
          : BadRequest(new { status = 400, message = "Failed to delete blog" }); // If the blog deletion fails, return a bad request response.
            }
            catch (Exception ex)
            {
                // If an exception occurs during the deletion operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }


        // HTTP GET endpoint to get the count of blogs for dashboard.
        [HttpGet("get-count")]
        public IActionResult GetBlogCounts()
        {
            try
            {
                // Get the count of blogs using the blog service.
                Task<int> blogCountTask = _blogService.getNoOfBlog();
                int count = blogCountTask.Result;

                // Return the count of blogs along with a success response.
                return base.Ok(new { status = 200, data = count, message = "Blogs found" });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }

        // HTTP GET endpoint to get the count of blogs within a specified date range.
        [HttpGet("get-countByDate")]
        public IActionResult GetBlogCountsWithDate(string from, string to)
        {
            try
            {
                // Get the count of blogs within the specified date range using the blog service.
                Task<int> blogCountTask = _blogService.getNoOfBlogByDate(from, to);
                int count = blogCountTask.Result;

                // Return the count of blogs along with a success response.
                return base.Ok(new { status = 200, data = count, message = "Blogs found" });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }


        // HTTP GET endpoint to retrieve the top 10 blogs.
        [HttpGet("get-top10Blogs")]
        public IActionResult GetTop10Blog()
        {
            try
            {
                // Retrieve the top 10 blogs and bloggers using the blog service.
                Task<Dictionary<string, object>> task = _blogService.getTop10BlogsAndBloggers();
                object blogs = task.Result;

                // Return the top 10 blogs along with a success response.
                return base.Ok(new { status = 200, data = blogs });
            }
            catch (Exception ex)
            {
                // If an exception occurs during the operation, call the serverError method to handle and return the error response.
                return serverError(ex);
            }
        }






    }
}