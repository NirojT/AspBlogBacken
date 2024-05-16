using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Blog
{
    // Interface for managing email-related operations.
    public interface IEmailService
    {
        // Sends an email with the specified parameters.
        Task sendEmail(string toEmail, string subject, string body, bool isBodyHTML);

        // Sends a password reset link to the specified email address.
        Task sendPasswordResetLink(string toEmail, string url);
    }
}
