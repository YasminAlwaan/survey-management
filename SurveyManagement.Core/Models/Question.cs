using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyManagement.Core.Models
{
    public class Question
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        public bool IsRequired { get; set; }

        [StringLength(1000)]
        public string HelpText { get; set; }

        [Column(TypeName = "jsonb")]
        public QuestionOptions Options { get; set; }

        public int Order { get; set; }

        public virtual Survey Survey { get; set; }
        public virtual ICollection<QuestionResponse> Responses { get; set; }
    }

    public class QuestionOptions
    {
        public List<string> Choices { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string Placeholder { get; set; }
        public bool AllowMultipleSelections { get; set; }
        public string ValidationRegex { get; set; }
        public string ValidationMessage { get; set; }
    }

    public enum QuestionType
    {
        Text,
        Number,
        Rating,
        MultipleChoice,
        Checkbox,
        Date,
        Time,
        DateTime,
        Email,
        Phone,
        Scale,
        Matrix
    }
} 