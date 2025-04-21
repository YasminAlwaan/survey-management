using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using SurveyManagement.Application.DTOs;
using SurveyManagement.Application.Services;
using SurveyManagement.Core.Models;

namespace SurveyManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(
            ISurveyService surveyService,
            ILogger<SurveyController> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new survey
        /// </summary>
        /// <param name="request">Survey creation request</param>
        /// <returns>Created survey</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new survey",
            Description = "Creates a new survey with the specified configuration",
            OperationId = "CreateSurvey",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(201, "Survey created successfully", typeof(Survey))]
        [SwaggerResponse(400, "Invalid request data")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        public async Task<ActionResult<Survey>> CreateSurvey([FromBody] CreateSurveyRequest request)
        {
            try
            {
                var survey = await _surveyService.CreateSurveyAsync(request);
                return CreatedAtAction(nameof(GetSurvey), new { id = survey.Id }, survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                return BadRequest(new { error = "Failed to create survey" });
            }
        }

        /// <summary>
        /// Gets a survey by ID
        /// </summary>
        /// <param name="id">Survey ID</param>
        /// <returns>Survey details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get survey by ID",
            Description = "Retrieves a survey by its unique identifier",
            OperationId = "GetSurvey",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(200, "Survey retrieved successfully", typeof(Survey))]
        [SwaggerResponse(404, "Survey not found")]
        public async Task<ActionResult<Survey>> GetSurvey(Guid id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null)
                return NotFound();

            return Ok(survey);
        }

        /// <summary>
        /// Gets surveys by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of surveys</returns>
        [HttpGet("department/{department}")]
        [SwaggerOperation(
            Summary = "Get surveys by department",
            Description = "Retrieves all surveys associated with a specific department",
            OperationId = "GetSurveysByDepartment",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(200, "Surveys retrieved successfully", typeof(IEnumerable<Survey>))]
        public async Task<ActionResult<IEnumerable<Survey>>> GetSurveysByDepartment(string department)
        {
            var surveys = await _surveyService.GetSurveysByDepartmentAsync(department);
            return Ok(surveys);
        }

        /// <summary>
        /// Assigns a survey to a patient
        /// </summary>
        /// <param name="id">Survey ID</param>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Assignment result</returns>
        [HttpPost("{id}/assign/{patientId}")]
        [SwaggerOperation(
            Summary = "Assign survey to patient",
            Description = "Assigns a survey to a specific patient",
            OperationId = "AssignSurveyToPatient",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(200, "Survey assigned successfully")]
        [SwaggerResponse(400, "Invalid assignment request")]
        [SwaggerResponse(404, "Survey or patient not found")]
        public async Task<IActionResult> AssignSurveyToPatient(Guid id, string patientId)
        {
            var result = await _surveyService.AssignSurveyToPatientAsync(id, patientId);
            if (!result)
                return BadRequest(new { error = "Failed to assign survey" });

            return Ok(new { message = "Survey assigned successfully" });
        }

        /// <summary>
        /// Gets survey analytics
        /// </summary>
        /// <param name="id">Survey ID</param>
        /// <returns>Survey analytics</returns>
        [HttpGet("{id}/analytics")]
        [SwaggerOperation(
            Summary = "Get survey analytics",
            Description = "Retrieves analytics data for a specific survey",
            OperationId = "GetSurveyAnalytics",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(200, "Analytics retrieved successfully", typeof(SurveyAnalytics))]
        [SwaggerResponse(404, "Survey not found")]
        public async Task<ActionResult<SurveyAnalytics>> GetSurveyAnalytics(Guid id)
        {
            var analytics = await _surveyService.GetSurveyAnalyticsAsync(id);
            if (analytics == null)
                return NotFound();

            return Ok(analytics);
        }

        /// <summary>
        /// Updates survey status
        /// </summary>
        /// <param name="id">Survey ID</param>
        /// <param name="status">New status</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}/status")]
        [SwaggerOperation(
            Summary = "Update survey status",
            Description = "Updates the status of a specific survey",
            OperationId = "UpdateSurveyStatus",
            Tags = new[] { "Surveys" }
        )]
        [SwaggerResponse(200, "Status updated successfully")]
        [SwaggerResponse(400, "Invalid status update")]
        [SwaggerResponse(404, "Survey not found")]
        public async Task<IActionResult> UpdateSurveyStatus(Guid id, [FromBody] SurveyStatus status)
        {
            var result = await _surveyService.UpdateSurveyStatusAsync(id, status);
            if (!result)
                return BadRequest(new { error = "Failed to update survey status" });

            return Ok(new { message = "Survey status updated successfully" });
        }
    }
} 