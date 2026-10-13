using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Payroll;

namespace NHRMS.Api.Controllers;

[ApiController]
[Authorize(Policy = "PayrollRun")]
[Route("api/[controller]")]
public sealed class PayrollCyclesController : ControllerBase
{
    private readonly PayrollCycleService _payrollCycleService;

    public PayrollCyclesController(PayrollCycleService payrollCycleService)
    {
        _payrollCycleService = payrollCycleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollCycleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var cycles = await _payrollCycleService.GetAllAsync(cancellationToken);
        return Ok(cycles);
    }

    [HttpPost]
    public async Task<ActionResult<PayrollCycleDto>> Create([FromBody] CreatePayrollCycleRequest request, CancellationToken cancellationToken)
    {
        var cycle = await _payrollCycleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, cycle);
    }
}
