using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace AwardWeb.Services
{
  
    public class AuthMessageSender : IEmailSender
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
            client.Credentials = new NetworkCredential(options.UserName, options.PassWord);
            
            mailMessage.To.Add(email);
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;
            client.Send(mailMessage);
            return Task.FromResult(0);
        }
      
    }
}
