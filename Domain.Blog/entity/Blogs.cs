using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Blog.entity
{
    // Entity class representing a blog.
    public class Blogs
    {
        // Unique identifier of the blog.
        [Key]
        public Guid id { get; set; }

        // Title of the blog.
        [Column("title")]
        public string title { get; set; }

        // Content of the blog.
        [MaxLength(300)]
        public string content { get; set; }

        // Name of the image associated with the blog.
        public string imageName { get; set; }

        // Reference to the user who authored the blog.
        [JsonIgnore]
        public User? user { get; set; }

        // List of comments on the blog.
        [JsonIgnore]
        public List<Comment>? comments { get; set; }

        // List of reactions on the blog.
        [JsonIgnore]
        public List<React?> react { get; set; }

        // Date when the blog was created.
        public DateTime createdDate { get; set; }

        // Date when the blog was last modified.
        public DateTime modifiedDate { get; set; }
    }
}
