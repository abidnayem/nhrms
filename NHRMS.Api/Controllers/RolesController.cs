using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Roles;

namespace NHRMS.Api.Controllers;

[ApiController]
[Authorize(Policy = "UserAdmin")]
[Route("api/[controller]")]
public sealed class RolesController : ControllerBase
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _roleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, role);
    }
}
