using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using CsvHelper;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using SurveyManagement.Application.Interfaces;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Application.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly IRepository<Survey> _surveyRepository;
        private readonly IRepository<SurveyResponse> _responseRepository;
        private readonly IRepository<User> _userRepository;

        public SurveyService(
            IRepository<Survey> surveyRepository,
            IRepository<SurveyResponse> responseRepository,
            IRepository<User> userRepository)
        {
            _surveyRepository = surveyRepository;
            _responseRepository = responseRepository;
            _userRepository = userRepository;
        }

        public async Task<Survey> CreateSurveyAsync(Survey survey)
        {
            if (!await ValidateSurveyAsync(survey))
                throw new ArgumentException("Invalid survey data");

            survey.Id = Guid.NewGuid();
            survey.CreatedDate = DateTime.UtcNow;
            survey.Status = SurveyStatus.Draft;

            return await _surveyRepository.AddAsync(survey);
        }

        public async Task<Survey> GetSurveyByIdAsync(Guid id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
                throw new KeyNotFoundException($"Survey with ID {id} not found");
            return survey;
        }

        public async Task<IEnumerable<Survey>> GetSurveysByDepartmentAsync(string department)
        {
            return await _surveyRepository.FindAsync(s => s.Department == department);
        }

        public async Task<IEnumerable<Survey>> GetActiveSurveysAsync()
        {
            return await _surveyRepository.FindAsync(s => 
                s.Status == SurveyStatus.Active && 
                (!s.ExpiryDate.HasValue || s.ExpiryDate > DateTime.UtcNow));
        }

        public async Task UpdateSurveyAsync(Survey survey)
        {
            if (!await ValidateSurveyAsync(survey))
                throw new ArgumentException("Invalid survey data");

            await _surveyRepository.UpdateAsync(survey);
        }

        public async Task DeleteSurveyAsync(Guid id)
        {
            var survey = await GetSurveyByIdAsync(id);
            await _surveyRepository.DeleteAsync(survey);
        }

        public async Task<bool> AssignSurveyToPatientAsync(Guid surveyId, string patientId)
        {
            var survey = await GetSurveyByIdAsync(surveyId);
            var patient = await _userRepository.FindAsync(u => u.Username == patientId && u.Role == UserRole.Patient);
            
            if (!patient.Any())
                throw new KeyNotFoundException($"Patient {patientId} not found");

            // Create a new survey response in 'Started' state
            var response = new SurveyResponse
            {
                Id = Guid.NewGuid(),
                SurveyId = surveyId,
                RespondentId = patientId,
                Status = ResponseStatus.Started,
                SubmissionDate = DateTime.UtcNow
            };

            await _responseRepository.AddAsync(response);
            return true;
        }

        public async Task<SurveyResponse> SubmitSurveyResponseAsync(SurveyResponse response)
        {
            var survey = await GetSurveyByIdAsync(response.SurveyId);
            
            // Validate required questions
            foreach (var question in survey.Questions.Where(q => q.IsRequired))
            {
                if (!response.Responses.Any(r => r.QuestionId == question.Id && !string.IsNullOrEmpty(r.Answer)))
                    throw new ArgumentException($"Required question {question.Text} not answered");
            }

            response.Status = ResponseStatus.Completed;
            response.SubmissionDate = DateTime.UtcNow;
            
            return await _responseRepository.AddAsync(response);
        }

        public async Task<IEnumerable<SurveyResponse>> GetSurveyResponsesAsync(Guid surveyId)
        {
            return await _responseRepository.FindAsync(r => r.SurveyId == surveyId);
        }

        public async Task<Dictionary<string, object>> GetSurveyAnalyticsAsync(Guid surveyId)
        {
            var responses = await GetSurveyResponsesAsync(surveyId);
            var survey = await GetSurveyByIdAsync(surveyId);
            var analytics = new Dictionary<string, object>();

            // Basic analytics
            analytics["total_responses"] = responses.Count();
            analytics["completion_rate"] = responses.Count(r => r.Status == ResponseStatus.Completed) * 100.0 / responses.Count();
            
            // Question-specific analytics
            var questionAnalytics = new Dictionary<Guid, object>();
            foreach (var question in survey.Questions)
            {
                var questionResponses = responses
                    .SelectMany(r => r.Responses)
                    .Where(r => r.QuestionId == question.Id);

                switch (question.Type)
                {
                    case QuestionType.Rating:
                    case QuestionType.Scale:
                        var numericAnswers = questionResponses
                            .Select(r => double.TryParse(r.Answer, out double n) ? n : 0)
                            .ToList();
                        questionAnalytics[question.Id] = new
                        {
                            average = numericAnswers.Any() ? numericAnswers.Average() : 0,
                            min = numericAnswers.Any() ? numericAnswers.Min() : 0,
                            max = numericAnswers.Any() ? numericAnswers.Max() : 0
                        };
                        break;

                    case QuestionType.MultipleChoice:
                    case QuestionType.SingleChoice:
                        var optionCounts = questionResponses
                            .GroupBy(r => r.Answer)
                            .ToDictionary(g => g.Key, g => g.Count());
                        questionAnalytics[question.Id] = optionCounts;
                        break;

                    default:
                        questionAnalytics[question.Id] = questionResponses.Count();
                        break;
                }
            }
            analytics["questions"] = questionAnalytics;

            return analytics;
        }

        public async Task<bool> ValidateSurveyAsync(Survey survey)
        {
            if (string.IsNullOrEmpty(survey.Title))
                return false;

            if (!survey.Questions.Any())
                return false;

            foreach (var question in survey.Questions)
            {
                if (string.IsNullOrEmpty(question.Text))
                    return false;

                if ((question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.SingleChoice) 
                    && !question.Options.Any())
                    return false;
            }

            return true;
        }

        public async Task<byte[]> ExportSurveyResponsesAsync(Guid surveyId, string format)
        {
            var responses = await GetSurveyResponsesAsync(surveyId);
            var survey = await GetSurveyByIdAsync(surveyId);

            switch (format.ToLower())
            {
                case "csv":
                    using (var memoryStream = new MemoryStream())
                    using (var writer = new StreamWriter(memoryStream))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        // Write headers
                        csv.WriteField("ResponseId");
                        csv.WriteField("RespondentId");
                        csv.WriteField("SubmissionDate");
                        csv.WriteField("Status");
                        foreach (var question in survey.Questions.OrderBy(q => q.OrderIndex))
                        {
                            csv.WriteField(question.Text);
                        }
                        csv.NextRecord();

                        // Write data
                        foreach (var response in responses)
                        {
                            csv.WriteField(response.Id);
                            csv.WriteField(response.RespondentId);
                            csv.WriteField(response.SubmissionDate);
                            csv.WriteField(response.Status);

                            foreach (var question in survey.Questions.OrderBy(q => q.OrderIndex))
                            {
                                var answer = response.Responses
                                    .FirstOrDefault(r => r.QuestionId == question.Id)?.Answer ?? "";
                                csv.WriteField(answer);
                            }
                            csv.NextRecord();
                        }

                        writer.Flush();
                        return memoryStream.ToArray();
                    }

                case "json":
                    var jsonData = JsonConvert.SerializeObject(responses, Formatting.Indented);
                    return Encoding.UTF8.GetBytes(jsonData);

                default:
                    throw new ArgumentException($"Unsupported export format: {format}");
            }
        }
    }
} 