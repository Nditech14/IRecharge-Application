﻿namespace Infrastructure.Untilities.Communication.Abstraction
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlContent, string plainTextContent);
    }
}