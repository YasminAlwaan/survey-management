using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Application.Interfaces
{
    public interface ISurveyService
    {
        Task<Survey> CreateSurveyAsync(Survey survey);
        Task<Survey> GetSurveyByIdAsync(Guid id);
        Task<IEnumerable<Survey>> GetSurveysByDepartmentAsync(string department);
        Task<IEnumerable<Survey>> GetActiveSurveysAsync();
        Task UpdateSurveyAsync(Survey survey);
        Task DeleteSurveyAsync(Guid id);
        Task<bool> AssignSurveyToPatientAsync(Guid surveyId, string patientId);
        Task<SurveyResponse> SubmitSurveyResponseAsync(SurveyResponse response);
        Task<IEnumerable<SurveyResponse>> GetSurveyResponsesAsync(Guid surveyId);
        Task<Dictionary<string, object>> GetSurveyAnalyticsAsync(Guid surveyId);
        Task<bool> ValidateSurveyAsync(Survey survey);
        Task<byte[]> ExportSurveyResponsesAsync(Guid surveyId, string format);
    }
} 