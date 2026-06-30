namespace CMS.Backend.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderName { get; set; } = "TechZone Shop";
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
    }
}
