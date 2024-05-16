using Application.Blog;
using AutoMapper;
using Domain.Blog.dto;
using Domain.Blog.entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using static System.Reflection.Metadata.BlobBuilder;

namespace Infrastructure.Blog
{
    public class BlogsService : IBlogsService
    {
        private readonly ApplicationDBContext _dbContext;

        private readonly UserManager<User> _userManager;

        //constructor injection
        public BlogsService(ApplicationDBContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;

            _userManager = userManager;
        }



        public async Task<bool> createBlogs(BlogsDto blogsDto, Guid user_id)
        {

            var user = await _userManager.FindByIdAsync(user_id.ToString());

            // if null return
            if (blogsDto == null || user == null) return false;

            blogsDto.id = Guid.NewGuid();
            blogsDto.user = user;





            blogsDto.createdDate = DateTime.Now.ToUniversalTime();
            blogsDto.modifiedDate = DateTime.Now.ToUniversalTime();

            //changing dto to entity
            Blogs blogs = ConvertDtoToEntity(blogsDto);


            var result = await _dbContext.AddAsync(blogs);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> updateBlogs(BlogsDto blogsDto, Guid blog_id, Guid user_id)
        {
            //include user also while finding blogs
            var blog = await _dbContext.Blogs.Include(b => b.user).FirstOrDefaultAsync(b => b.id == blog_id);

            if (blog == null) return false;

            //if user not found in blogs than return false
            if (blog.user != null && Guid.Parse(blog.user.Id) != user_id) return false;


            //all good than update the value
            if (blogsDto.title != null)
            {
                blog.title = blogsDto.title;
            }

            if (blogsDto.content != null)
            {
                blog.content = blogsDto.content;

            }
            if (blogsDto.imageName != null)
            {
                blog.imageName = blogsDto.imageName;

            }


            blogsDto.modifiedDate = DateTime.Now.ToUniversalTime();
            await this._dbContext.SaveChangesAsync();

            return true;

        }





        public async Task<bool> deleteBlogs(Guid blog_id, Guid user_id)
        {
            //include user also while finding blogs
            var blog = await _dbContext.Blogs
                     .Include(b => b.comments)
                .Include(b => b.user).FirstOrDefaultAsync(b => b.id == blog_id);

            //if user not found in blogs than return false
            if (blog == null) return false;
            if (blog.user != null && Guid.Parse(blog.user.Id) != user_id) return false;

            // Remove associated comments
            _dbContext.Comments.RemoveRange(blog.comments);

            _dbContext.Remove(blog);
            await _dbContext.SaveChangesAsync();

            //checking again

            var blogChecking = await _dbContext.Blogs.FindAsync(blog_id);
            if (blogChecking == null)
            {
                return true;
            }
            return false;


        }

        public async Task<List<BlogsDto>> getAllBlogs()
        {
            List<Blogs> blogs = await _dbContext.Blogs.Include(c => c.user).ToListAsync();
            List<React> reacts = await _dbContext.Reacts.ToListAsync();
            if (reacts != null && reacts.Count > 0)
            {
                foreach (var blog in blogs)
                {

                    blog.react = await _dbContext.Reacts
                 .Where(r => r.blogId == blog.id)
                 .Include(r => r.user)
                 .ToListAsync();

                }
            }
                //converting entity to dto
                List<BlogsDto> blogsDtos = blogs.Select(ConvertEntityToDto).ToList();
            return blogsDtos;
        }

        public async Task<BlogsDto> getBlogsById(Guid blog_id, Guid user_id)
        {
            var blog = await _dbContext.Blogs.Include(b => b.user).FirstOrDefaultAsync(b => b.id == blog_id);

            if (blog == null) return null;

            if (blog.user != null && Guid.Parse(blog.user.Id) != user_id) return null;

            //converting entity to dto
            return ConvertEntityToDto(blog);
        }






        public BlogsDto ConvertEntityToDto(Blogs blogs)
        {
            if (blogs == null) return null;


            BlogsDto blogsDto = new BlogsDto
            {
                id = blogs.id,
                title = blogs.title,
                content = blogs.content,
                imageName = blogs.imageName,
                user = blogs.user,
                comments = blogs.comments,
                react = blogs.react,
                createdDate = blogs.createdDate,
                modifiedDate = blogs.modifiedDate
            };

            return blogsDto;
        }

        public Blogs ConvertDtoToEntity(BlogsDto blogsDto)
        {
            if (blogsDto == null) return null;


            Blogs blogs = new Blogs
            {
                id = blogsDto.id,
                title = blogsDto.title,
                content = blogsDto.content,
                imageName = blogsDto.imageName,
                user = blogsDto.user,
                comments = blogsDto.comments,
                react = blogsDto.react,
                createdDate = blogsDto.createdDate,
                modifiedDate = blogsDto.modifiedDate
            };

            return blogs;
        }



        public async Task<Dictionary<string, object>> getTop10BlogsAndBloggers()
        {
            List<Blogs> blogs = await _dbContext.Blogs.Include(b => b.user).ToListAsync();
            List<Comment> comments = await _dbContext.Comments.Include(b => b.user).ToListAsync();
            //doing select so the circular dependency is avoided
            List<React> reacts = await _dbContext.Reacts
                .Select(b => new React
                {
                    // Select only necessary properties
                    id = b.id,
                    type = b.type,
                    blogId = b.blogId,
                    // Include necessary navigation properties
                    user = b.user,

                }).
                ToListAsync();

            // Calculate popularity scores for each blog
            Dictionary<Blogs, int> blogPopularity = new Dictionary<Blogs, int>();
            foreach (var blog in blogs)
            {
                int popularityScore = 0;

                // Calculate upvote/downvote weightage based on reacts
                int upvoteCount = reacts.Count(react => react.blogId == blog.id && react.type.ToLower() == "upvote");
                int downvoteCount = reacts.Count(react => react.blogId == blog.id && react.type.ToLower() == "downvote");
                Console.WriteLine(blog.user.UserName + " upvote is " + upvoteCount);
                Console.WriteLine(blog.user.UserName + " downvoteCount is " + downvoteCount);
                popularityScore += (2 * upvoteCount) + (-1 * downvoteCount);

                // Calculate comment weightage
                int commentCount = comments.Count(comment => comment.blogs.id == blog.id);
                Console.WriteLine(blog.user.UserName + " commentCount is " + commentCount);
                popularityScore += commentCount;
                Console.WriteLine(blog.user.UserName + " popularityScore is " + popularityScore);


                blogPopularity.Add(blog, popularityScore);
            }

            // Sort blogs by popularity score
            var sortedBlogs = blogPopularity.OrderByDescending(pair => pair.Value).Take(10).Select(pair => pair.Key).ToList();

            var sortedBlogDtos = sortedBlogs.Select(item =>
            {
                BlogsDto blogsDto = ConvertEntityToDto(item);
                blogsDto.popularityScore = blogPopularity[item];
                return blogsDto;
            }).ToList();

            // Calculate popularity of users based on their contributions to blogs
            Dictionary<User, int> userPopularity = new Dictionary<User, int>();
            foreach (var blog in sortedBlogs)
            {
                if (!userPopularity.ContainsKey(blog.user))
                {
                    // Initialize the user's popularity score if it doesn't exist
                    userPopularity[blog.user] = 0;
                }

                // Add the blog's popularity score to the user's total
                userPopularity[blog.user] += blogPopularity[blog];


                Console.WriteLine($"Blog ID: {blog.id},  {blogPopularity[blog]}  : {blog.title}, User ID: {blog.user.Id}, User Name: {blog.user.UserName}," +
                    $" Popularity Score: {userPopularity[blog.user]}");
            }

            // Sort users by popularity
            var sortedUsers = userPopularity.OrderByDescending(pair => pair.Value).Take(10).Select(pair => pair.Key).ToList();
            List<UserDto> userDtos = sortedUsers.Select(item =>
            {
                return new UserDto(item.Id, item.UserName, item.Email, userPopularity[item]);
            }).ToList();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("blogs", sortedBlogDtos);
            dictionary.Add("users", userDtos);
            return dictionary;
        }

        public async Task<int> getNoOfBlog()
        {
            return await _dbContext.Blogs.CountAsync();
        }

        public async Task<int> getNoOfBlogByDate(string from, string to)
        {
            //changing string to DateTime type
            DateTime fromDate = DateTime.SpecifyKind(DateTime.Parse(from), DateTimeKind.Utc);
            DateTime toDate = DateTime.SpecifyKind(DateTime.Parse(to), DateTimeKind.Utc);


            return await _dbContext.Blogs.Where(blog =>
            blog.createdDate >= fromDate && blog.modifiedDate
            <= toDate)
              .CountAsync();
        }

        public async Task<List<BlogsDto>> getPaginatedBlogs(int pageSize, int pageNumber)
        {
            int skip = (pageNumber) * pageSize;

            // paginating
            var blogs = await _dbContext.Blogs
                    .Include(b => b.user)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();


            List<React> reacts = await _dbContext.Reacts.ToListAsync();
            //react may be empty
            if (reacts != null && reacts.Count > 0)
            {
                foreach (var blog in blogs)
                {

                    blog.react = await _dbContext.Reacts
                 .Where(r => r.blogId == blog.id)
                 .Include(r => r.user)
                 .ToListAsync();




                }
            }



            return blogs.Select(ConvertEntityToDto).ToList();


        }

        public async Task<List<BlogsDto>> getBlogofUser(string userId)
        {

            var blogs = await _dbContext.Blogs.Include(b => b.user).Where(b => b.user.Id == userId)
                .ToListAsync();
            List<React> reacts = await _dbContext.Reacts.ToListAsync();

            //react may be empty
            if (reacts != null && reacts.Count > 0)
            {
                foreach (var blog in blogs)
                {
                    blog.react = await _dbContext.Reacts
                        .Where(r => r.blogId == blog.id)
                        .Include(r => r.user)
                        .ToListAsync();
                }
            }


            return blogs.Select(ConvertEntityToDto).ToList();

        }

        public async Task<List<BlogsDto>> getRecentBlog()
        {

            // order by descending and converting
            List<Blogs> blogs = await _dbContext.Blogs.Include(b => b.react).OrderByDescending(b => b.createdDate).ToListAsync();

            List<React> reacts = await _dbContext.Reacts.ToListAsync();
            if (reacts != null && reacts.Count > 0)
            {
                foreach (var blog in blogs)
                {

                    blog.react = await _dbContext.Reacts
                 .Where(r => r.blogId == blog.id)
                 .Include(r => r.user)
                 .ToListAsync();

                }
            }

                return blogs.Select(ConvertEntityToDto).ToList();


        }

        public async Task<List<BlogsDto>> searchBlog(string title)
        {

            List<BlogsDto> blogsDtos = _dbContext.Blogs
                 .Where(b => b.title.ToUpper().Contains(title.ToUpper()))
                .Include(b => b.user)
                .Include(b => b.react)

                .Select(ConvertEntityToDto)
                .ToList();
            List<React> reacts = await _dbContext.Reacts.ToListAsync();
            //react may be empty
            if (reacts != null && reacts.Count > 0)
            {
                foreach (var blog in blogsDtos)
                {

                    blog.react = await _dbContext.Reacts
                 .Where(r => r.blogId == blog.id)
                 .Include(r => r.user)
                 .ToListAsync();

                }
            }

            return blogsDtos;
        }
    }
}