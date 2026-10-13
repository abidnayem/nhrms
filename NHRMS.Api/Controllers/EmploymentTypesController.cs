using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.EmploymentTypes;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmploymentTypesController : ControllerBase
{
    private readonly EmploymentTypeService _employmentTypeService;

    public EmploymentTypesController(EmploymentTypeService employmentTypeService)
    {
        _employmentTypeService = employmentTypeService;
    }

    [HttpGet]
    [Authorize(Policy = "OrganizationRead")]
    public async Task<ActionResult<IEnumerable<EmploymentTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var employmentTypes = await _employmentTypeService.GetAllAsync(cancellationToken);
        return Ok(employmentTypes);
    }

    [HttpPost]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<ActionResult<EmploymentTypeDto>> Create([FromBody] CreateEmploymentTypeRequest request, CancellationToken cancellationToken)
    {
        var employmentType = await _employmentTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, employmentType);
    }
}
