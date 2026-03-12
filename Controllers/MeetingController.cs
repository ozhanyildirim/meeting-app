using MeetingApp.Models;
using MeetingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
[ApiController]
[Route("api/meetings")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all meetings</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all meetings",
        Description = "Returns all non-cancelled meetings for the authenticated user")]
    [SwaggerResponse(200, "Success", typeof(List<MeetingResponse>))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<List<MeetingResponse>>> GetAll()
    {
        try
        {
            var meetings = await _meetingService.GetAllAsync(GetUserId());
            return Ok(meetings);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Get meeting by id</summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get meeting by id",
        Description = "Returns meeting details including document for the given id")]
    [SwaggerResponse(200, "Success", typeof(MeetingResponse))]
    [SwaggerResponse(404, "Meeting not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<MeetingResponse>> GetById(
        [SwaggerParameter(Description = "The unique identifier of the meeting", Required = true)] int id)
    {
        try
        {
            var meeting = await _meetingService.GetByIdAsync(id, GetUserId());
            return Ok(meeting);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Create a new meeting</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new meeting",
        Description = "Creates a new meeting. An optional document can be uploaded as base64")]
    [SwaggerResponse(200, "Meeting created successfully", typeof(MeetingResponse))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<MeetingResponse>> Create(
        [FromBody, SwaggerRequestBody(Description = "Meeting details", Required = true)] MeetingDto dto)
    {
        try
        {
            var meeting = await _meetingService.CreateAsync(dto, GetUserId());
            return Ok(meeting);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Update a meeting</summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update a meeting",
        Description = "Updates an existing meeting. A new document can be uploaded as base64")]
    [SwaggerResponse(200, "Meeting updated successfully", typeof(MeetingResponse))]
    [SwaggerResponse(400, "Invalid request or meeting is already cancelled")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<MeetingResponse>> Update(
        [SwaggerParameter(Description = "The unique identifier of the meeting to update", Required = true)] int id,
        [FromBody, SwaggerRequestBody(Description = "Updated meeting details", Required = true)] MeetingDto dto)
    {
        try
        {
            var meeting = await _meetingService.UpdateAsync(id, dto, GetUserId());
            return Ok(meeting);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Delete a meeting</summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete a meeting",
        Description = "Permanently deletes a meeting. The deletion is logged in the MeetingDeleteLogs table via trigger")]
    [SwaggerResponse(200, "Meeting deleted successfully")]
    [SwaggerResponse(400, "Meeting not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task Delete(
        [SwaggerParameter(Description = "The unique identifier of the meeting to delete", Required = true)] int id)
    {
            await _meetingService.DeleteAsync(id, GetUserId());
    }

    /// <summary>Cancel a meeting</summary>
    [HttpPatch("{id}/cancel")]
    [SwaggerOperation(
        Summary = "Cancel a meeting",
        Description = "Marks a meeting as cancelled. Cancelled meetings are permanently deleted by Hangfire job periodically")]
    [SwaggerResponse(200, "Meeting cancelled successfully")]
    [SwaggerResponse(400, "Meeting not found or already cancelled")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task Cancel(
        [SwaggerParameter(Description = "The unique identifier of the meeting to cancel", Required = true)] int id)
    {
            await _meetingService.CancelAsync(id, GetUserId());
    }
}