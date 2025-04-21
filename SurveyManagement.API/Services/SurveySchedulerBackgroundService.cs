using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SurveyManagement.Application.Services;

namespace SurveyManagement.API.Services
{
    public class SurveySchedulerBackgroundService : BackgroundService
    {
        private readonly SurveySchedulerService _schedulerService;
        private readonly ILogger<SurveySchedulerBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public SurveySchedulerBackgroundService(
            SurveySchedulerService schedulerService,
            ILogger<SurveySchedulerBackgroundService> logger)
        {
            _schedulerService = schedulerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Survey Scheduler Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _schedulerService.ProcessScheduledSurveysAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing scheduled surveys.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Survey Scheduler Background Service is stopping.");
        }
    }
} 