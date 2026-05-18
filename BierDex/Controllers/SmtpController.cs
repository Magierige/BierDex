using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;


namespace BierDex.Controllers
{
    public class SmtpController : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpController(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (subject.Contains("Reset Password") || htmlMessage.Contains("reset your password"))
            {
                // 1. Extract the token (everything after the last colon and space)
                string token = htmlMessage.Contains(":")
                    ? htmlMessage.Split(':').Last().Trim()
                    : htmlMessage.Trim();

                // 2. Encode ONLY the token
                string encodedToken = WebUtility.UrlEncode(token);

                // 3. Construct the clean link
                string resetLink = $"{_config["smtpCredentials:linkSpa"]}/reset-password?email={WebUtility.UrlEncode(email)}&code={encodedToken}";

                htmlMessage = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                <h2 style='color: #333;'>Wachtwoord herstellen</h2>
                <p>Je hebt gevraagd om je wachtwoord te resetten voor <strong>BierDex</strong>.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetLink}' 
                       style='background-color: #F2A900; color: white; padding: 12px 25px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                       Wachtwoord Resetten
                    </a>
                </div>
                <p style='font-size: 0.85em; color: #666;'>
                    Werkt de knop niet? Kopieer dan deze link in je browser:<br>
                    <span style='color: #D18F00; word-break: break-all;'>{resetLink}</span>
                </p>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='font-size: 0.75em; color: #999;'>Heb je dit niet aangevraagd? Dan kun je deze e-mail veilig negeren.</p>
            </div>";
            }

            // SMTP Sending logic...
            using (var client = new SmtpClient(_config["smtpCredentials:host"]))
            {
                client.Port = int.Parse(_config["smtpCredentials:port"]);
                client.Credentials = new NetworkCredential(_config["smtpCredentials:username"], _config["smtpCredentials:password"]);
                client.EnableSsl = true;

                var mail = new MailMessage("noreply@bierdex.nl", email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mail);
            }
        }
    }
}