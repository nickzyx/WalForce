using Microsoft.Extensions.Options;
using WebServer.Configuration;
using WebServer.Domain.Models;

namespace WebServer.Features.Auth;

public sealed class JwtTokenService(IOptions<AuthOptions> authOptions) : IJwtTokenService
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    public string CreateToken(UserRecord user) => BearerTokenCodec.CreateToken(user, _authOptions);
}
