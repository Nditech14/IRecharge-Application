using Azure.Communication.Email;
using Infrastructure.Untilities.Communication.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Untilities.Communication.Implemenatation
{
    public class EmailService : IEmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _senderEmail;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            var connectionString = configuration["AzureCommunicationService:ConnectionString"];
            _emailClient = new EmailClient(connectionString);
            _senderEmail = configuration["AzureCommunicationService:sender"];
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlContent, string plainTextContent)
        {
            try
            {

                //var reciepientEmail = GetUserEmailFromHttpContext();
                var emailSendOperation = await _emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    _senderEmail,
                    recipientEmail,
                    subject,
                    htmlContent,
                    plainTextContent
                );

                return emailSendOperation.HasCompleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        private string GetUserEmailFromHttpContext()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "emails" || c.Type == "email" || c.Type == "preferred_username")?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new UnauthorizedAccessException("Email not found in the user's claims.");
            }

            return userEmail;
        }
    }
}
