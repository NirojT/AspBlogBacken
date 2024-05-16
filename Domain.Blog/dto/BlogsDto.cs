using Domain.Blog.entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Domain.Blog.dto
{
    // Data transfer object for representing blog information.
    public class BlogsDto
    {
        // Unique identifier of the blog.
        public Guid id { get; set; }

        // Title of the blog.
        public string? title { get; set; }

        // Content of the blog.
        public string? content { get; set; }

        // Image associated with the blog.
        [FromForm(Name = "image")]
        public IFormFile? image { get; set; }

        // Name of the image file.
        public string? imageName { get; set; }

        // Popularity score of the blog.
        public int popularityScore { get; set; }

        // Reference to the user who authored the blog.
        public User? user { get; set; }

        // List of comments on the blog.
        public List<Comment>? comments { get; set; }

        // List of reactions on the blog.
        public List<React>? react { get; set; }

        // Date when the blog was created.
        public DateTime createdDate { get; set; }

        // Date when the blog was last modified.
        public DateTime modifiedDate { get; set; }
    }
}
