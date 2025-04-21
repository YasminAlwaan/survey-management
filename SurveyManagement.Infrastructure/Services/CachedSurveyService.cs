using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Infrastructure.Services
{
    public class CachedSurveyService : ISurveyService
    {
        private readonly ISurveyService _decorated;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedSurveyService> _logger;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public CachedSurveyService(
            ISurveyService decorated,
            IMemoryCache cache,
            ILogger<CachedSurveyService> logger)
        {
            _decorated = decorated;
            _cache = cache;
            _logger = logger;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));
        }

        public async Task<Survey> GetSurveyByIdAsync(Guid id)
        {
            var cacheKey = $"survey_{id}";
            
            if (_cache.TryGetValue(cacheKey, out Survey cachedSurvey))
            {
                _logger.LogInformation($"Retrieved survey {id} from cache");
                return cachedSurvey;
            }

            var survey = await _decorated.GetSurveyByIdAsync(id);
            if (survey != null)
            {
                _cache.Set(cacheKey, survey, _cacheOptions);
                _logger.LogInformation($"Cached survey {id}");
            }

            return survey;
        }

        public async Task<IEnumerable<Survey>> GetSurveysByDepartmentAsync(string department)
        {
            var cacheKey = $"surveys_department_{department}";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Survey> cachedSurveys))
            {
                _logger.LogInformation($"Retrieved surveys for department {department} from cache");
                return cachedSurveys;
            }

            var surveys = await _decorated.GetSurveysByDepartmentAsync(department);
            if (surveys != null && surveys.Any())
            {
                _cache.Set(cacheKey, surveys, _cacheOptions);
                _logger.LogInformation($"Cached surveys for department {department}");
            }

            return surveys;
        }

        public async Task<IEnumerable<Survey>> GetActiveSurveysAsync()
        {
            const string cacheKey = "active_surveys";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Survey> cachedSurveys))
            {
                _logger.LogInformation("Retrieved active surveys from cache");
                return cachedSurveys;
            }

            var surveys = await _decorated.GetActiveSurveysAsync();
            if (surveys != null && surveys.Any())
            {
                _cache.Set(cacheKey, surveys, _cacheOptions);
                _logger.LogInformation("Cached active surveys");
            }

            return surveys;
        }

        public async Task<SurveyAnalytics> GetSurveyAnalyticsAsync(Guid surveyId)
        {
            var cacheKey = $"analytics_{surveyId}";
            
            if (_cache.TryGetValue(cacheKey, out SurveyAnalytics cachedAnalytics))
            {
                _logger.LogInformation($"Retrieved analytics for survey {surveyId} from cache");
                return cachedAnalytics;
            }

            var analytics = await _decorated.GetSurveyAnalyticsAsync(surveyId);
            if (analytics != null)
            {
                _cache.Set(cacheKey, analytics, _cacheOptions);
                _logger.LogInformation($"Cached analytics for survey {surveyId}");
            }

            return analytics;
        }

        public async Task<bool> AssignSurveyToPatientAsync(Guid surveyId, string patientId)
        {
            var result = await _decorated.AssignSurveyToPatientAsync(surveyId, patientId);
            if (result)
            {
                // Invalidate relevant caches
                _cache.Remove($"survey_{surveyId}");
                _cache.Remove("active_surveys");
                _cache.Remove($"analytics_{surveyId}");
                _logger.LogInformation($"Invalidated caches after assigning survey {surveyId} to patient {patientId}");
            }
            return result;
        }

        public async Task<bool> UpdateSurveyStatusAsync(Guid surveyId, SurveyStatus newStatus)
        {
            var result = await _decorated.UpdateSurveyStatusAsync(surveyId, newStatus);
            if (result)
            {
                // Invalidate relevant caches
                _cache.Remove($"survey_{surveyId}");
                _cache.Remove("active_surveys");
                _cache.Remove($"analytics_{surveyId}");
                _logger.LogInformation($"Invalidated caches after updating status of survey {surveyId}");
            }
            return result;
        }

        public async Task<Survey> CreateSurveyAsync(Survey survey)
        {
            var result = await _decorated.CreateSurveyAsync(survey);
            if (result != null)
            {
                // Invalidate relevant caches
                _cache.Remove("active_surveys");
                _logger.LogInformation($"Invalidated caches after creating new survey");
            }
            return result;
        }
    }
} 