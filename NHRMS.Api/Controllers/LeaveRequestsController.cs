using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Leave;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LeaveRequestsController : ControllerBase
{
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveRequestsController(LeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll(CancellationToken cancellationToken)
    {
        var leaveRequests = await _leaveRequestService.GetAllAsync(cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequest request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, leaveRequest);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<LeaveRequestDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateLeaveRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return Ok(leaveRequest);
    }
}

public sealed class UpdateLeaveRequestStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
