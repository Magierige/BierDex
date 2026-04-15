using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;


namespace BierDex.Controllers
{
    public class SmtpControler : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpControler(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_config["smtpCredentials:host"])
            {
                Port = int.Parse(_config["smtpCredentials:port"]),
                Credentials = new NetworkCredential(_config["smtpCredentials:username"], _config["smtpCredentials:password"]),
                EnableSsl = true,
            };

            var mail = new MailMessage("noreply@yourapp.com", email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
