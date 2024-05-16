using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for changing password.
    public class ChangePasswordDto
    {
        // Username of the user.
        public string UserName { get; set; }

        // Current password of the user.
        public string CurrentPassword { get; set; }

        // New password to be set.
        public string NewPassword { get; set; }
    }
}
