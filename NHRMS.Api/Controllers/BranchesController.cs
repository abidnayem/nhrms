using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Branches;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BranchesController : ControllerBase
{
    private readonly BranchService _branchService;

    public BranchesController(BranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    [Authorize(Policy = "OrganizationRead")]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetAll(CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllAsync(cancellationToken);
        return Ok(branches);
    }

    [HttpPost]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<ActionResult<BranchDto>> Create([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var branch = await _branchService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, branch);
    }
}
