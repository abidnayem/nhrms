using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Users;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly UserAccountService _userAccountService;

    public UsersController(UserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userAccountService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserAccountDto>> Create([FromBody] CreateUserAccountRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, user);
    }

    [HttpPost("assign-role")]
    public async Task<ActionResult<UserAccountDto>> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountService.AssignRoleAsync(request, cancellationToken);
        return Ok(user);
    }
}
