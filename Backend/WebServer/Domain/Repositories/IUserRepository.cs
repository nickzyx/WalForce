using WebServer.Domain.Models;

namespace WebServer.Domain.Repositories;

public interface IUserRepository
{
    Task<UserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserRecord>> GetEmployeesAsync(CancellationToken cancellationToken);
}
