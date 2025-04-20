using System;
using System.Collections.Generic;

namespace SurveyManagement.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public enum UserRole
    {
        Administrator,
        HealthcareAdmin,
        MedicalStaff,
        Patient,
        SystemIntegrator
    }

    public static class Permissions
    {
        public const string CreateSurvey = "Surveys.Create";
        public const string EditSurvey = "Surveys.Edit";
        public const string DeleteSurvey = "Surveys.Delete";
        public const string ViewSurvey = "Surveys.View";
        public const string AssignSurvey = "Surveys.Assign";
        public const string ViewResponses = "Responses.View";
        public const string ExportData = "Data.Export";
        public const string ManageUsers = "Users.Manage";
        public const string ViewAnalytics = "Analytics.View";
    }
} 