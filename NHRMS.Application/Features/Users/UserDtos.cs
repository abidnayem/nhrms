namespace NHRMS.Application.Features.Users;

public sealed class CreateUserAccountRequest
{
    public Guid EmployeeId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

public sealed class UserAccountDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<Guid> RoleIds { get; set; } = Array.Empty<Guid>();
}

public sealed record AuthenticatedUserDto(
    UserAccountDto User,
    IReadOnlyList<string> RoleCodes,
    IReadOnlyList<string> PermissionCodes);

public sealed class BootstrapAdminRequest
{
    public string CompanyCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
}
