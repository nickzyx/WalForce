namespace WebServer.Domain.Models;

public sealed class UserRecord
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;
}
