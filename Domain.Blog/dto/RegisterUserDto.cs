using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for registering a new user.
    public class RegisterUserDto
    {
        // Username of the user.
        [Required]
        public string? userName { get; set; }

        // Profile image of the user.
        public IFormFile image { get; set; }

        // Name of the image file.
        public string? imageName { get; set; }

        // Email address of the user.
        [Required]
        [EmailAddress]
        public string? email { get; set; }

        // Password of the user.
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
