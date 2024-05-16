using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.entity
{
    // Entity class representing a user.
    public class User : IdentityUser
    {
        // Name of the image associated with the user.
        public string imageName { get; set; }
    }
}
