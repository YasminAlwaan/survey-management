using System;
using System.Collections.Generic;

namespace SurveyManagement.Core.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public string ValidationRules { get; set; }
        public Guid SurveyId { get; set; }
        public Survey Survey { get; set; }
    }

    public enum QuestionType
    {
        Text,
        MultipleChoice,
        SingleChoice,
        Rating,
        Date,
        Boolean,
        Scale
    }
} 