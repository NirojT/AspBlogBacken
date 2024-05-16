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
    // Entity class representing a comment.
    public class Comment
    {
        // Unique identifier of the comment.
        [Key]
        public Guid id { get; set; }

        // Content of the comment.
        public string content { get; set; }

        // Reference to the user who authored the comment.
        public User? user { get; set; }

        // Reference to the parent comment if it's a reply.
        public string? referenceCommentId { get; set; }

        // Reference to the blog the comment belongs to.
        [JsonIgnore]
        public Blogs? blogs { get; set; }

        // List of reactions on the comment.
        public List<React?> reacts { get; set; }

        // Date when the comment was created.
        public DateTime createdDate { get; set; }

        // Date when the comment was last modified.
        public DateTime modifiedDate { get; set; }
    }
}
