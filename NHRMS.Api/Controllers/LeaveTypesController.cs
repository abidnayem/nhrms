using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Leave;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LeaveTypesController : ControllerBase
{
    private readonly LeaveTypeService _leaveTypeService;

    public LeaveTypesController(LeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var leaveTypes = await _leaveTypeService.GetAllAsync(cancellationToken);
        return Ok(leaveTypes);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveTypeDto>> Create([FromBody] CreateLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, leaveType);
    }
}
