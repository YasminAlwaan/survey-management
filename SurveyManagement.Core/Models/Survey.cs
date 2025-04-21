using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyManagement.Core.Models
{
    public class Survey
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        public SurveyStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ScheduledDeliveryTime { get; set; }

        public bool IsRecurring { get; set; }

        [StringLength(50)]
        public string RecurrencePattern { get; set; }

        [Column(TypeName = "jsonb")]
        public NotificationSettings NotificationSettings { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<SurveyResponse> Responses { get; set; }
        public virtual ICollection<SurveyAssignment> Assignments { get; set; }
    }

    public class NotificationSettings
    {
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public string EmailTemplate { get; set; }
        public string SmsTemplate { get; set; }
        public int ReminderIntervalHours { get; set; }
        public int MaxReminders { get; set; }
    }

    public class SurveyAssignment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public DateTime AssignedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public virtual Survey Survey { get; set; }
        public virtual User Patient { get; set; }
    }

    public enum SurveyStatus
    {
        Draft,
        Active,
        Paused,
        Completed,
        Archived
    }
} 