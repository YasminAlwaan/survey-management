using System;
using System.Collections.Generic;

namespace SurveyManagement.Core.Models
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Survey Survey { get; set; }
        public string RespondentId { get; set; }
        public DateTime SubmissionDate { get; set; }
        public List<QuestionResponse> Responses { get; set; } = new List<QuestionResponse>();
        public ResponseStatus Status { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    public class QuestionResponse
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string Answer { get; set; }
        public Guid SurveyResponseId { get; set; }
        public SurveyResponse SurveyResponse { get; set; }
    }

    public enum ResponseStatus
    {
        Started,
        PartiallyCompleted,
        Completed,
        Abandoned
    }
} 