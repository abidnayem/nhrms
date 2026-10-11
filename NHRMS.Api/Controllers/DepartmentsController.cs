using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Departments;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = await _departmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, department);
    }
}
