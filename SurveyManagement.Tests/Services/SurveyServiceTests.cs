using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SurveyManagement.Application.Services;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;
using SurveyManagement.Infrastructure.Data;
using Xunit;

namespace SurveyManagement.Tests.Services
{
    public class SurveyServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<ILogger<SurveyService>> _loggerMock;

        public SurveyServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _notificationServiceMock = new Mock<INotificationService>();
            _auditServiceMock = new Mock<IAuditService>();
            _loggerMock = new Mock<ILogger<SurveyService>>();
        }

        [Fact]
        public async Task CreateSurvey_ValidSurvey_ReturnsCreatedSurvey()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = new SurveyService(
                new Repository<Survey>(context),
                new Repository<SurveyResponse>(context),
                new Repository<User>(context),
                _notificationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object);

            var survey = new Survey
            {
                Title = "Test Survey",
                Description = "Test Description",
                Department = "Test Department",
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "Test Question",
                        Type = QuestionType.Text,
                        IsRequired = true
                    }
                }
            };

            // Act
            var result = await service.CreateSurveyAsync(survey);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(SurveyStatus.Draft, result.Status);
            _auditServiceMock.Verify(x => x.LogAuditEventAsync(
                It.IsAny<string>(),
                "Create",
                "Survey",
                result.Id.ToString(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AssignSurveyToPatient_ValidData_ReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = new SurveyService(
                new Repository<Survey>(context),
                new Repository<SurveyResponse>(context),
                new Repository<User>(context),
                _notificationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object);

            var survey = new Survey
            {
                Title = "Test Survey",
                Status = SurveyStatus.Active
            };
            await context.Surveys.AddAsync(survey);

            var patient = new User
            {
                Username = "testpatient",
                Role = UserRole.Patient
            };
            await context.Users.AddAsync(patient);

            await context.SaveChangesAsync();

            // Act
            var result = await service.AssignSurveyToPatientAsync(survey.Id, patient.Username);

            // Assert
            Assert.True(result);
            _notificationServiceMock.Verify(x => x.SendEmailAsync(
                patient.Email,
                survey.Title,
                It.IsAny<string>()), Times.Once);
            _auditServiceMock.Verify(x => x.LogAuditEventAsync(
                It.IsAny<string>(),
                "Assign",
                "Survey",
                survey.Id.ToString(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetSurveyAnalytics_ValidSurvey_ReturnsAnalytics()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var service = new SurveyService(
                new Repository<Survey>(context),
                new Repository<SurveyResponse>(context),
                new Repository<User>(context),
                _notificationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object);

            var survey = new Survey
            {
                Title = "Test Survey",
                Status = SurveyStatus.Active,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "Rating Question",
                        Type = QuestionType.Rating,
                        IsRequired = true
                    }
                }
            };
            await context.Surveys.AddAsync(survey);

            var response = new SurveyResponse
            {
                SurveyId = survey.Id,
                Status = ResponseStatus.Completed,
                Responses = new List<QuestionResponse>
                {
                    new QuestionResponse
                    {
                        QuestionId = survey.Questions[0].Id,
                        Answer = "5"
                    }
                }
            };
            await context.SurveyResponses.AddAsync(response);

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetSurveyAnalyticsAsync(survey.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result["total_responses"]);
            Assert.Equal(100.0, result["completion_rate"]);
            Assert.NotNull(result["questions"]);
        }
    }
} 