using WebServer.Domain.Models;

namespace WebServer.Domain.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyList<ShiftRecord>> GetEmployeeScheduleAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken cancellationToken);

    Task<IReadOnlyList<ShiftRecord>> GetTeamScheduleAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}
