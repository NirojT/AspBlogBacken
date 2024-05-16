using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for updating user information.
    public class UpdateUserDto
    {
        // Email address of the user.
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Username of the user.
        public string Username { get; set; }

        // Password of the user.
        public string Password { get; set; }
    }
}
