using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace SurveyManagement.API.Controllers
{
    [ApiController]
    [Route("api/docs")]
    public class ApiDocumentationController : ControllerBase
    {
        [HttpGet("integration")]
        public IActionResult GetIntegrationExamples()
        {
            var examples = new
            {
                CreateSurvey = new
                {
                    Description = "Create a new survey with scheduling and notifications",
                    Endpoint = "POST /api/surveys",
                    Example = new
                    {
                        Title = "Post-Visit Patient Satisfaction Survey",
                        Description = "Survey to collect feedback after patient visits",
                        Department = "Cardiology",
                        TriggerType = "PostVisit",
                        TriggerMetadata = "24:00:00", // 24 hours after visit
                        NotificationSettings = new
                        {
                            SendEmail = true,
                            SendSms = true,
                            EmailTemplate = "Dear {PatientName}, please complete our {SurveyTitle} survey: {SurveyLink}",
                            SmsTemplate = "Please complete our {SurveyTitle} survey: {SurveyLink}",
                            ReminderIntervalHours = 24,
                            MaxReminders = 2
                        },
                        Questions = new[]
                        {
                            new
                            {
                                Text = "How satisfied were you with your visit?",
                                Type = "Rating",
                                IsRequired = true,
                                Options = new[] { "1", "2", "3", "4", "5" }
                            },
                            new
                            {
                                Text = "What could we improve?",
                                Type = "Text",
                                IsRequired = false
                            }
                        }
                    }
                },
                AssignSurvey = new
                {
                    Description = "Assign a survey to a specific patient",
                    Endpoint = "POST /api/surveys/{surveyId}/assign/{patientId}",
                    Example = new
                    {
                        SurveyId = "123e4567-e89b-12d3-a456-426614174000",
                        PatientId = "patient123"
                    }
                },
                SubmitResponse = new
                {
                    Description = "Submit a survey response",
                    Endpoint = "POST /api/surveys/responses",
                    Example = new
                    {
                        SurveyId = "123e4567-e89b-12d3-a456-426614174000",
                        RespondentId = "patient123",
                        Responses = new[]
                        {
                            new
                            {
                                QuestionId = "456e4567-e89b-12d3-a456-426614174000",
                                Answer = "5"
                            },
                            new
                            {
                                QuestionId = "789e4567-e89b-12d3-a456-426614174000",
                                Answer = "The staff was very helpful"
                            }
                        }
                    }
                },
                GetAnalytics = new
                {
                    Description = "Get survey analytics and response data",
                    Endpoint = "GET /api/surveys/{id}/analytics",
                    ExampleResponse = new
                    {
                        total_responses = 150,
                        completion_rate = 85.5,
                        questions = new
                        {
                            "456e4567-e89b-12d3-a456-426614174000" = new
                            {
                                average = 4.2,
                                min = 1,
                                max = 5
                            }
                        }
                    }
                }
            };

            return Ok(examples);
        }

        [HttpGet("security")]
        public IActionResult GetSecurityDocumentation()
        {
            var securityInfo = new
            {
                Authentication = "JWT Bearer Token Authentication",
                Authorization = new
                {
                    Roles = new[]
                    {
                        new { Role = "Administrator", Description = "Full system access" },
                        new { Role = "HealthcareAdmin", Description = "Survey management and analytics" },
                        new { Role = "MedicalStaff", Description = "Survey assignment and response viewing" },
                        new { Role = "Patient", Description = "Survey response submission" },
                        new { Role = "SystemIntegrator", Description = "API integration access" }
                    },
                    Permissions = new[]
                    {
                        "Surveys.Create",
                        "Surveys.Edit",
                        "Surveys.Delete",
                        "Surveys.View",
                        "Surveys.Assign",
                        "Responses.View",
                        "Data.Export",
                        "Users.Manage",
                        "Analytics.View"
                    }
                },
                AuditLogging = new
                {
                    Description = "All sensitive operations are logged with user, action, and timestamp",
                    LoggedEvents = new[]
                    {
                        "Survey creation",
                        "Survey modification",
                        "Survey deletion",
                        "Response submission",
                        "Data export",
                        "User access"
                    }
                }
            };

            return Ok(securityInfo);
        }
    }
} 