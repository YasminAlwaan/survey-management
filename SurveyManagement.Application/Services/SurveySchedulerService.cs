using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Application.Services
{
    public class SurveySchedulerService
    {
        private readonly IRepository<Survey> _surveyRepository;
        private readonly IRepository<User> _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SurveySchedulerService> _logger;

        public SurveySchedulerService(
            IRepository<Survey> surveyRepository,
            IRepository<User> userRepository,
            INotificationService notificationService,
            ILogger<SurveySchedulerService> logger)
        {
            _surveyRepository = surveyRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ProcessScheduledSurveysAsync()
        {
            var now = DateTime.UtcNow;
            var surveys = await _surveyRepository.FindAsync(s => 
                s.Status == SurveyStatus.Active && 
                s.ScheduledDeliveryTime <= now);

            foreach (var survey in surveys)
            {
                try
                {
                    await ProcessSurveyDeliveryAsync(survey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing survey {survey.Id}");
                }
            }
        }

        private async Task ProcessSurveyDeliveryAsync(Survey survey)
        {
            if (survey.TriggerType == TriggerType.PostVisit)
            {
                await ProcessPostVisitSurveyAsync(survey);
            }
            else if (survey.IsRecurring)
            {
                await ProcessRecurringSurveyAsync(survey);
            }
            else
            {
                await ProcessOneTimeSurveyAsync(survey);
            }
        }

        private async Task ProcessPostVisitSurveyAsync(Survey survey)
        {
            // Get patients who had appointments recently
            var recentPatients = await GetRecentPatientsAsync(survey.TriggerMetadata);
            
            foreach (var patient in recentPatients)
            {
                await SendSurveyNotificationAsync(survey, patient);
            }
        }

        private async Task ProcessRecurringSurveyAsync(Survey survey)
        {
            if (survey.RecurrencePattern == null) return;

            var now = DateTime.UtcNow;
            var nextDelivery = CalculateNextDeliveryTime(survey);

            if (nextDelivery <= now)
            {
                await ProcessOneTimeSurveyAsync(survey);
                survey.ScheduledDeliveryTime = nextDelivery;
                await _surveyRepository.UpdateAsync(survey);
            }
        }

        private async Task ProcessOneTimeSurveyAsync(Survey survey)
        {
            var targetUsers = await GetTargetUsersAsync(survey);
            
            foreach (var user in targetUsers)
            {
                await SendSurveyNotificationAsync(survey, user);
            }
        }

        private async Task SendSurveyNotificationAsync(Survey survey, User user)
        {
            if (survey.NotificationSettings.SendEmail)
            {
                var emailBody = FormatTemplate(survey.NotificationSettings.EmailTemplate, user, survey);
                await _notificationService.SendEmailAsync(user.Email, survey.Title, emailBody);
            }

            if (survey.NotificationSettings.SendSms)
            {
                var smsBody = FormatTemplate(survey.NotificationSettings.SmsTemplate, user, survey);
                await _notificationService.SendSmsAsync(user.Username, smsBody);
            }
        }

        private async Task<IEnumerable<User>> GetRecentPatientsAsync(string triggerMetadata)
        {
            // Parse trigger metadata to get appointment completion time window
            var timeWindow = TimeSpan.Parse(triggerMetadata);
            var cutoffTime = DateTime.UtcNow - timeWindow;

            return await _userRepository.FindAsync(u => 
                u.Role == UserRole.Patient && 
                u.LastLoginDate >= cutoffTime);
        }

        private async Task<IEnumerable<User>> GetTargetUsersAsync(Survey survey)
        {
            if (string.IsNullOrEmpty(survey.Department))
            {
                return await _userRepository.FindAsync(u => u.Role == UserRole.Patient);
            }

            return await _userRepository.FindAsync(u => 
                u.Role == UserRole.Patient && 
                u.Department == survey.Department);
        }

        private DateTime CalculateNextDeliveryTime(Survey survey)
        {
            if (survey.RecurrencePattern == null) return DateTime.MaxValue;

            var now = DateTime.UtcNow;
            var lastDelivery = survey.ScheduledDeliveryTime ?? now;

            switch (survey.RecurrencePattern.Type)
            {
                case RecurrenceType.Daily:
                    return lastDelivery.AddDays(survey.RecurrencePattern.Interval);
                case RecurrenceType.Weekly:
                    return lastDelivery.AddDays(7 * survey.RecurrencePattern.Interval);
                case RecurrenceType.Monthly:
                    return lastDelivery.AddMonths(survey.RecurrencePattern.Interval);
                default:
                    return DateTime.MaxValue;
            }
        }

        private string FormatTemplate(string template, User user, Survey survey)
        {
            return template
                .Replace("{PatientName}", $"{user.FirstName} {user.LastName}")
                .Replace("{SurveyTitle}", survey.Title)
                .Replace("{SurveyLink}", $"https://yourdomain.com/surveys/{survey.Id}");
        }
    }
} 