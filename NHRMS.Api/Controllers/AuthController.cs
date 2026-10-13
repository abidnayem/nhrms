using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NHRMS.Application.Features.Users;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserAccountService userAccountService,
    AdminBootstrapService bootstrapService,
    IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userAccountService.AuthenticateAsync(request.UserName, request.Password, cancellationToken);
        if (user is null)
            return Unauthorized(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Invalid user name or password." });
        return Ok(CreateToken(user));
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("bootstrap")]
    public async Task<ActionResult<AccessTokenResponse>> Bootstrap(
        BootstrapAdminRequest request,
        [FromHeader(Name = "X-Bootstrap-Key")] string? suppliedKey,
        CancellationToken cancellationToken)
    {
        var expectedKey = configuration["Authentication:BootstrapKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
            return NotFound();
        if (string.IsNullOrEmpty(suppliedKey) || !FixedTimeEquals(suppliedKey, expectedKey))
            return Unauthorized();

        var user = await bootstrapService.CreateFirstHrManagerAsync(request, cancellationToken);
        return Ok(CreateToken(user));
    }

    private AccessTokenResponse CreateToken(AuthenticatedUserDto user)
    {
        var key = configuration["Authentication:JwtKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(60);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.User.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.User.Id.ToString()),
            new(ClaimTypes.Name, user.User.UserName)
        };
        claims.AddRange(user.RoleCodes.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.PermissionCodes.Select(permission => new Claim("permission", permission)));
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("NHRMS", "NHRMS.Web", claims, expires: expiresAtUtc, signingCredentials: credentials);
        return new AccessTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            user.User.Id,
            user.User.UserName,
            user.RoleCodes,
            user.PermissionCodes);
    }

    private static bool FixedTimeEquals(string supplied, string expected)
    {
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record AccessTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string UserName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
