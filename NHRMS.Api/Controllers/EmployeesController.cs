using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Employees;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [Authorize(Policy = "EmployeeRead")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return Ok(employees);
    }

    [HttpGet("directory")]
    [Authorize(Policy = "EmployeeRead")]
    public async Task<ActionResult<EmployeeDirectoryDto>> Search(
        [FromQuery] EmployeeDirectoryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "EmployeeWrite")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, employee);
    }
}
