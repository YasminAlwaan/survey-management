using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyManagement.Application.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveysController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,HealthcareAdmin")]
        public async Task<ActionResult<Survey>> CreateSurvey(Survey survey)
        {
            try
            {
                var createdSurvey = await _surveyService.CreateSurveyAsync(survey);
                return CreatedAtAction(nameof(GetSurvey), new { id = createdSurvey.Id }, createdSurvey);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Survey>> GetSurvey(Guid id)
        {
            try
            {
                var survey = await _surveyService.GetSurveyByIdAsync(id);
                return Ok(survey);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("department/{department}")]
        [Authorize(Roles = "Administrator,HealthcareAdmin,MedicalStaff")]
        public async Task<ActionResult<IEnumerable<Survey>>> GetSurveysByDepartment(string department)
        {
            var surveys = await _surveyService.GetSurveysByDepartmentAsync(department);
            return Ok(surveys);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Survey>>> GetActiveSurveys()
        {
            var surveys = await _surveyService.GetActiveSurveysAsync();
            return Ok(surveys);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,HealthcareAdmin")]
        public async Task<IActionResult> UpdateSurvey(Guid id, Survey survey)
        {
            if (id != survey.Id)
                return BadRequest();

            try
            {
                await _surveyService.UpdateSurveyAsync(survey);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,HealthcareAdmin")]
        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            try
            {
                await _surveyService.DeleteSurveyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/assign/{patientId}")]
        [Authorize(Roles = "Administrator,HealthcareAdmin,MedicalStaff")]
        public async Task<IActionResult> AssignSurveyToPatient(Guid id, string patientId)
        {
            try
            {
                await _surveyService.AssignSurveyToPatientAsync(id, patientId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("responses")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<SurveyResponse>> SubmitSurveyResponse(SurveyResponse response)
        {
            try
            {
                var submittedResponse = await _surveyService.SubmitSurveyResponseAsync(response);
                return Ok(submittedResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/responses")]
        [Authorize(Roles = "Administrator,HealthcareAdmin,MedicalStaff")]
        public async Task<ActionResult<IEnumerable<SurveyResponse>>> GetSurveyResponses(Guid id)
        {
            var responses = await _surveyService.GetSurveyResponsesAsync(id);
            return Ok(responses);
        }

        [HttpGet("{id}/analytics")]
        [Authorize(Roles = "Administrator,HealthcareAdmin,MedicalStaff")]
        public async Task<ActionResult<Dictionary<string, object>>> GetSurveyAnalytics(Guid id)
        {
            try
            {
                var analytics = await _surveyService.GetSurveyAnalyticsAsync(id);
                return Ok(analytics);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/export")]
        [Authorize(Roles = "Administrator,HealthcareAdmin")]
        public async Task<IActionResult> ExportSurveyResponses(Guid id, [FromQuery] string format = "csv")
        {
            try
            {
                var fileContent = await _surveyService.ExportSurveyResponsesAsync(id, format);
                string contentType = format.ToLower() == "json" ? "application/json" : "text/csv";
                string fileName = $"survey_{id}_responses.{format.ToLower()}";
                
                return File(fileContent, contentType, fileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 