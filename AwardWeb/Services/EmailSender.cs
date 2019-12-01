using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AwardWeb.Services
{

    public class EmailSender : IEmailSender
    {
        private readonly Models.SMTPOptions options;

        public EmailSender(Microsoft.Extensions.Options.IOptionsSnapshot<Models.SMTPOptions> options)
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
