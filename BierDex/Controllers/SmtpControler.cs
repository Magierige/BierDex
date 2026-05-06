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
            // Check of de mail over het resetten van het wachtwoord gaat
            if (subject.Contains("Reset your password") || htmlMessage.Contains("password using the following code"))
            {
                // De 'htmlMessage' die je nu krijgt is de lange token.
                // We halen eventuele witruimte weg en bouwen onze eigen link.
                string token = htmlMessage.Trim();

                // Let op: Vergeet niet de token te URL-encoden voor de link
                string encodedToken = WebUtility.UrlEncode(token);

                // Vervang 'localhost:7125' door de URL van je frontend-route
                string resetLink = $"{_config["smtpCredentials:linkSpa"]}/reset-password?email={email}&code={encodedToken}";

                htmlMessage = $@"
            <div style='font-family: sans-serif;'>
                <h2>Wachtwoord herstellen</h2>
                <p>Je hebt gevraagd om je wachtwoord te resetten voor BierDex.</p>
                <p>Klik op de onderstaande knop om een nieuw wachtwoord in te stellen:</p>
                <a href='{resetLink}' 
                   style='background-color: #ffc107; color: black; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                   Wachtwoord Resetten
                </a>
                <p style='margin-top: 20px; font-size: 0.8em; color: #666;'>
                    Werkt de knop niet? Kopieer dan deze link in je browser:<br>
                    {resetLink}
                </p>
            </div>";
            }

            // Je bestaande SMTP verzendcode
            var client = new SmtpClient(_config["smtpCredentials:host"])
            {
                Port = int.Parse(_config["smtpCredentials:port"]),
                Credentials = new NetworkCredential(_config["smtpCredentials:username"], _config["smtpCredentials:password"]),
                EnableSsl = true,
            };

            var mail = new MailMessage("noreply@bierdex.nl", email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
