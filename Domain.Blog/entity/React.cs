using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.entity
{
    // Entity class representing a reaction.
    public class React
    {
        // Unique identifier of the reaction.
        [Key]
        public Guid id { get; set; }

        // Type of the reaction.
        public string type { get; set; }

        // Indicates if the reaction is for a blog.
        public bool isInBlog { get; set; }

        // Reference to the user who reacted.
        public User user { get; set; }

        // Foreign key property for blog.
        public Guid? blogId { get; set; }

        // Reference to the comment the reaction belongs to.
        public Comment? comment { get; set; }

        // Date when the reaction was created.
        public DateTime createdDate { get; set; }

        // Date when the reaction was last modified.
        public DateTime modifiedDate { get; set; }

        // Implicit conversion operator.
        public static implicit operator React(List<React> v)
        {
            throw new NotImplementedException();
        }
    }
}
