using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Blog.dto
{
    // Data transfer object for handling password reset requests.
    public class ForgotPassword
    {
        // Token used for password reset verification.
        [Required]
        public string? token { get; set; }

        // New password to be set.
        [Required]
        [DataType(DataType.Password)]
        public string? password { get; set; }

        // Email address associated with the account.
        [Required]
        [EmailAddress]
        public string? email { get; set; }
    }
}
