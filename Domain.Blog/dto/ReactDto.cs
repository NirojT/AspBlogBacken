using Domain.Blog.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for representing reactions.
    public class ReactDto
    {
        // User ID who reacted.
        public string userId { get; set; }

        // Type of reaction.
        public string type { get; set; }

        // Indicates if the reaction is for a blog.
        public bool isInBlog { get; set; }

        // Foreign key property for blog.
        public Guid? blogId { get; set; }

        // Foreign key property for comment.
        public Guid? commentId { get; set; }
    }
}
