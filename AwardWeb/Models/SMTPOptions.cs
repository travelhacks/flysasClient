namespace AwardWeb.Models
{
    public class SMTPOptions
    {
        public string Host { get; private set; }
        public string UserName { get; private set; }
        public string PassWord { get; private set; }
        public string From { get; private set; }        
    }
}
