using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Employees;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/employment-changes")]
public sealed class EmployeeChangesController(EmploymentChangeService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "EmployeeRead")]
    public async Task<ActionResult<IReadOnlyList<EmploymentChangeDto>>> Get(Guid employeeId, CancellationToken cancellationToken)
        => Ok(await service.GetByEmployeeAsync(employeeId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "EmploymentChangeManage")]
    public async Task<ActionResult<EmploymentChangeDto>> CreateRequest(Guid employeeId, RequestEmploymentChange request, CancellationToken cancellationToken)
    {
        var change = await service.RequestAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { employeeId }, change);
    }

    [HttpPut("{changeId:guid}/decision")]
    [Authorize(Policy = "EmploymentChangeApprove")]
    public async Task<ActionResult<EmploymentChangeDto>> Decide(Guid employeeId, Guid changeId, DecideEmploymentChange request, CancellationToken cancellationToken)
        => Ok(await service.DecideAsync(employeeId, changeId, request, cancellationToken));

    [HttpPost("apply-due")]
    [Authorize(Policy = "EmploymentChangeManage")]
    public async Task<ActionResult<object>> ApplyDue(Guid employeeId, CancellationToken cancellationToken)
        => Ok(new { appliedCount = await service.ApplyDueAsync(employeeId, cancellationToken) });
}
