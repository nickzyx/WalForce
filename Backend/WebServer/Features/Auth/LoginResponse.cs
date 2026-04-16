namespace WebServer.Features.Auth;

public sealed record LoginResponse(string AccessToken, AuthenticatedUserDto User);
