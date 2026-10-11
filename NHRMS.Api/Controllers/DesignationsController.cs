using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Designations;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DesignationsController : ControllerBase
{
    private readonly DesignationService _designationService;

    public DesignationsController(DesignationService designationService)
    {
        _designationService = designationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DesignationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var designations = await _designationService.GetAllAsync(cancellationToken);
        return Ok(designations);
    }

    [HttpPost]
    public async Task<ActionResult<DesignationDto>> Create([FromBody] CreateDesignationRequest request, CancellationToken cancellationToken)
    {
        var designation = await _designationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, designation);
    }
}
