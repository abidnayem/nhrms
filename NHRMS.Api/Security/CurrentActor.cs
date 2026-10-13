using System.Security.Claims;
using NHRMS.Application.Abstractions;

namespace NHRMS.Api.Security;

public sealed class CurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    public string Name => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? accessor.HttpContext?.User.Identity?.Name
        ?? "system";
}
