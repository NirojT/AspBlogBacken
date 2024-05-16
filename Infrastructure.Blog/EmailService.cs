using Application.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Blog
{
    public class EmailService : IEmailService
    {
        public Task sendEmail(string toEmail, string subject, string body, bool isBodyHTML)
        {
            string MailServer = "smtp.gmail.com";
            string FromEmail = "tmgnirah@gmail.com";
            string Password = "xmhh azgr bmkq cnxr";
            int Port = 587;
            var client = new SmtpClient(MailServer, Port)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true,
            };
            MailMessage mailMessage = new MailMessage(FromEmail, toEmail, subject, body)
            {
                IsBodyHtml = isBodyHTML
            };
            return client.SendMailAsync(mailMessage);
        }

        public async Task sendPasswordResetLink(string toEmail, string resetUrl)
        {
            Console.WriteLine("link is " + resetUrl);
            string subject = "Reset Your Password";
            string body = $"<p>Please click <a href='{resetUrl}'>here</a> to reset your password.</p>";

            string mailServer = "smtp.gmail.com";
            string fromEmail = "tmgnirah@gmail.com";
            string password = "xmhh azgr bmkq cnxr";
            int port = 587;

            using var client = new SmtpClient(mailServer, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
