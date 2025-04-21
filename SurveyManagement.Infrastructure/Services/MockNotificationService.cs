using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SurveyManagement.Core.Interfaces;

namespace SurveyManagement.Infrastructure.Services
{
    public class MockNotificationService : INotificationService
    {
        private readonly ILogger<MockNotificationService> _logger;

        public MockNotificationService(ILogger<MockNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation($"Mock Email sent to {to} with subject: {subject}");
            await Task.CompletedTask;
        }

        public async Task SendSmsAsync(string to, string message)
        {
            _logger.LogInformation($"Mock SMS sent to {to} with message: {message}");
            await Task.CompletedTask;
        }
    }
} 