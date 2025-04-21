using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;
using SurveyManagement.Infrastructure.Data;

namespace SurveyManagement.Infrastructure.Repositories
{
    public class OptimizedSurveyRepository : ISurveyRepository
    {
        private readonly ApplicationDbContext _context;

        public OptimizedSurveyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Survey> GetByIdAsync(Guid id)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Responses)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Patient)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Survey>> GetByDepartmentAsync(string department)
        {
            return await _context.Surveys
                .Where(s => s.Department == department)
                .Include(s => s.Questions)
                .Include(s => s.Assignments)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Survey>> GetActiveSurveysAsync()
        {
            return await _context.Surveys
                .Where(s => s.Status == SurveyStatus.Active)
                .Include(s => s.Questions)
                .Include(s => s.Assignments)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SurveyAnalytics> GetSurveyAnalyticsAsync(Guid surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Responses)
                .Include(s => s.Assignments)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return null;

            var totalAssignments = survey.Assignments.Count;
            var completedAssignments = survey.Assignments.Count(a => a.CompletedAt.HasValue);
            var completionRate = totalAssignments > 0 
                ? (double)completedAssignments / totalAssignments * 100 
                : 0;

            var questionAnalytics = survey.Questions.Select(q => new QuestionAnalytics
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                TotalResponses = q.Responses.Count,
                AverageRating = q.Type == QuestionType.Rating 
                    ? q.Responses.Average(r => double.Parse(r.Answer)) 
                    : null,
                ResponseDistribution = q.Type == QuestionType.MultipleChoice || q.Type == QuestionType.Checkbox
                    ? q.Responses.GroupBy(r => r.Answer)
                        .ToDictionary(g => g.Key, g => g.Count())
                    : null
            }).ToList();

            return new SurveyAnalytics
            {
                SurveyId = surveyId,
                TotalAssignments = totalAssignments,
                CompletedAssignments = completedAssignments,
                CompletionRate = completionRate,
                QuestionAnalytics = questionAnalytics
            };
        }

        public async Task<bool> AssignSurveyToPatientAsync(Guid surveyId, string patientId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Assignments)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null || survey.Status != SurveyStatus.Active)
                return false;

            if (survey.Assignments.Any(a => a.PatientId == patientId && !a.CompletedAt.HasValue))
                return false;

            var assignment = new SurveyAssignment
            {
                SurveyId = surveyId,
                PatientId = patientId,
                AssignedAt = DateTime.UtcNow
            };

            _context.SurveyAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateSurveyStatusAsync(Guid surveyId, SurveyStatus newStatus)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);
            if (survey == null)
                return false;

            survey.Status = newStatus;
            survey.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
    }
} 