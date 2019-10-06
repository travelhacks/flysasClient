using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace AwardWeb.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly Models.SMTPOptions options;

        public AuthMessageSender(Microsoft.Extensions.Options.IOptions<Models.SMTPOptions> options)
        {
            this.options = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(options.From);
            SmtpClient client = new SmtpClient(options.Host);
            client.UseDefaultCredentials = false;
            client.Port = 587;
            //client.EnableSsl = true;
            client.Credentials = new NetworkCredential(options.UserName, options.PassWord);//x987%&bvcDF webb passwd
            
            mailMessage.To.Add(email);
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;
            client.Send(mailMessage);
            return Task.FromResult(0);
        }
     

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
