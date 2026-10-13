using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Attendance;

namespace NHRMS.Api.Controllers;

[ApiController]
[Authorize(Policy = "AttendanceManage")]
[Route("api/[controller]")]
public sealed class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.GetAllAsync(cancellationToken);
        return Ok(attendance);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDto>> Create([FromBody] CreateAttendanceRequest request, CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, attendance);
    }
}
