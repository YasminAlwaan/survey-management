using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyManagement.Core.Models
{
    public class SurveyResponse
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ResponseStatus Status { get; set; }

        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> Metadata { get; set; }

        public virtual Survey Survey { get; set; }
        public virtual User Patient { get; set; }
        public virtual ICollection<QuestionResponse> QuestionResponses { get; set; }
    }

    public class QuestionResponse
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SurveyResponseId { get; set; }

        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public string Answer { get; set; }

        public DateTime AnsweredAt { get; set; }

        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> Metadata { get; set; }

        public virtual SurveyResponse SurveyResponse { get; set; }
        public virtual Question Question { get; set; }
    }

    public enum ResponseStatus
    {
        Started,
        InProgress,
        Completed,
        Abandoned,
        Expired
    }
} 