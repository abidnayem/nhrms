using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Employees;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/disciplinary-cases")]
public sealed class DisciplinaryCasesController(DisciplinaryService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "DisciplineRead")]
    public async Task<ActionResult<IReadOnlyList<DisciplinaryCaseDto>>> Get(Guid employeeId, CancellationToken cancellationToken)
        => Ok(await service.GetByEmployeeAsync(employeeId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "DisciplineManage")]
    public async Task<ActionResult<DisciplinaryCaseDto>> Create(Guid employeeId, CreateDisciplinaryCaseRequest request, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { employeeId }, item);
    }

    [HttpPut("{caseId:guid}/decision")]
    [Authorize(Policy = "DisciplineDecide")]
    public async Task<ActionResult<DisciplinaryCaseDto>> Decide(Guid employeeId, Guid caseId, DecideDisciplinaryCaseRequest request, CancellationToken cancellationToken)
        => Ok(await service.DecideAsync(employeeId, caseId, request, cancellationToken));
}
