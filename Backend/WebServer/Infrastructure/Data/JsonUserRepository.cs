using WebServer.Domain;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class JsonUserRepository(JsonFileDataStore dataStore) : IUserRepository
{
    private const string UsersFileName = "users.json";

    public async Task<UserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var users = await dataStore.ReadListAsync<UserRecord>(UsersFileName, cancellationToken);
        return users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var users = await dataStore.ReadListAsync<UserRecord>(UsersFileName, cancellationToken);
        return users.FirstOrDefault(user => user.Id == id);
    }

    public async Task<IReadOnlyList<UserRecord>> GetEmployeesAsync(CancellationToken cancellationToken)
    {
        var users = await dataStore.ReadListAsync<UserRecord>(UsersFileName, cancellationToken);
        return users
            .Where(user => string.Equals(user.Role, Roles.Employee, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToArray();
    }
}
