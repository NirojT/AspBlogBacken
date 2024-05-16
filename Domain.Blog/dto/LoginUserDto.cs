using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for representing user login information.
    public class LoginUserDto
    {
        // Email address of the user.
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        // Password of the user.
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
