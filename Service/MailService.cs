using System.Net;
using System.Net.Mail;
using where_we_go.Config;
using where_we_go.Models.DTOs.Mail;

namespace where_we_go.Service
{
    public interface IMailService
    {
        Task<string> SendEmailAsync(SendMailRequestDto request);
    }

    public class MailService : IMailService
    {
        private readonly string _mailHost;
        private readonly int _mailPort;
        private readonly string _mailUsername;
        private readonly string _mailPassword;
        private readonly string _mailFromEmail;
        private readonly string _mailFromName;
        private readonly bool _mailEnableSsl;

        public MailService()
        {
            _mailHost = GlobalConfig.GetRequiredEnv(GlobalConfig.MailHost);
            _mailUsername = GlobalConfig.GetRequiredEnv(GlobalConfig.MailUsername);
            _mailPassword = GlobalConfig.GetRequiredEnv(GlobalConfig.MailPassword);
            _mailFromEmail = GlobalConfig.GetRequiredEnv(GlobalConfig.MailFromEmail);
            _mailFromName = GlobalConfig.GetEnvOrDefault(GlobalConfig.MailFromName, "where-we-go");
            _mailPort = GlobalConfig.GetRequiredIntEnv(GlobalConfig.MailPort);
            _mailEnableSsl = GlobalConfig.GetBoolEnvOrDefault(GlobalConfig.MailEnableSsl, true);
        }

        public async Task<string> SendEmailAsync(SendMailRequestDto request)
        {
            try
            {
                using MailMessage mail = new()
                {
                    From = new MailAddress(_mailFromEmail, _mailFromName),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = request.IsBodyHtml
                };
                mail.To.Add(request.ToEmail);

                using SmtpClient smtp = new(_mailHost, _mailPort)
                {
                    Credentials = new NetworkCredential(_mailUsername, _mailPassword),
                    EnableSsl = _mailEnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                await smtp.SendMailAsync(mail);
                return "Email sent";
            }
            catch (Exception err)
            {
                return $"Error sending email: {err.Message}";
            }
        }
    }
}