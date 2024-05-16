using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for representing user information.
    public class UserDto
    {
        // User ID.
        public string Id { get; set; }

        // User's username.
        public string UserName { get; set; }

        // Profile image of the user.
        public IFormFile image { get; set; }

        // Name of the image file.
        public string? imageName { get; set; }

        // User's email address.
        public string Email { get; set; }

        // User's popularity score.
        public int popularityScore { get; set; }

        // Constructor to initialize user properties.
        public UserDto(string id, string userName, string email, int popularityScore)
        {
            this.Id = id;
            this.UserName = userName;
            this.Email = email;
            this.popularityScore = popularityScore;
        }
    }
}
