using System.Security.Claims;

namespace WebServer.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var value =
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("Authenticated user is missing a valid identifier claim.");
    }
}
