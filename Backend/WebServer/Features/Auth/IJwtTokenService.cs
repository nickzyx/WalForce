using WebServer.Domain.Models;

namespace WebServer.Features.Auth;

public interface IJwtTokenService
{
    string CreateToken(UserRecord user);
}
