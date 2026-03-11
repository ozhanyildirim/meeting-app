using MeetingApp.Models;
using MeetingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    // Token'dan userId'yi al
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeetingDto dto)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MeetingDto dto)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _meetingService.DeleteAsync(id, GetUserId());
            return Ok("Toplantı silindi");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _meetingService.CancelAsync(id, GetUserId());
            return Ok("Toplantı iptal edildi");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}