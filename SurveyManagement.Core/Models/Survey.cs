using System;
using System.Collections.Generic;

namespace SurveyManagement.Core.Models
{
    public class Survey
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public string Department { get; set; }
        public SurveyStatus Status { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
        public TriggerType? TriggerType { get; set; }
        public string TriggerMetadata { get; set; }
    }

    public enum SurveyStatus
    {
        Draft,
        Active,
        Paused,
        Expired,
        Archived
    }

    public enum TriggerType
    {
        PostVisit,
        Scheduled,
        Manual,
        EventBased
    }
} 