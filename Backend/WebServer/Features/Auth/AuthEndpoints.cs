using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate a mock employee or manager user.")
            .WithDescription("Returns a bearer token and lightweight user payload when the seeded email and password are valid.");

        return endpoints;
    }

    private static async Task<Results<Ok<LoginResponse>, ValidationProblem, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        IUserRepository users,
        IPasswordHasher<UserRecord> passwordHasher,
        IJwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await users.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return TypedResults.Unauthorized();
        }

        var response = new LoginResponse(
            tokenService.CreateToken(user),
            new AuthenticatedUserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role));

        return TypedResults.Ok(response);
    }
}
