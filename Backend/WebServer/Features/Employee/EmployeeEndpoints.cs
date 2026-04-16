using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebServer.Domain;
using WebServer.Domain.Repositories;
using WebServer.Extensions;

namespace WebServer.Features.Employee;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/me")
            .WithTags("Me")
            .RequireAuthorization();

        group.MapGet("/profile", GetProfileAsync)
            .WithName("GetCurrentUserProfile")
            .WithSummary("Get the current authenticated user profile.");

        group.MapGet("/schedule", GetScheduleAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Employee))
            .WithName("GetMySchedule")
            .WithSummary("Get the current employee schedule for a date range.");

        group.MapGet("/availability", GetAvailabilityAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Employee))
            .WithName("GetMyAvailability")
            .WithSummary("Get the current employee weekly availability.");

        group.MapPut("/availability", PutAvailabilityAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Employee))
            .WithName("PutMyAvailability")
            .WithSummary("Replace the current employee weekly availability.");

        return endpoints;
    }

    private static async Task<Results<Ok<ProfileDto>, NotFound>> GetProfileAsync(
        ClaimsPrincipal principal,
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(principal.GetRequiredUserId(), cancellationToken);
        return user is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new ProfileDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role, user.Title));
    }

    private static async Task<Results<Ok<IReadOnlyList<ScheduleShiftDto>>, ValidationProblem>> GetScheduleAsync(
        DateOnly from,
        DateOnly to,
        ClaimsPrincipal principal,
        IScheduleRepository schedules,
        CancellationToken cancellationToken)
    {
        if (to < from)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["to"] = ["The 'to' date must be on or after the 'from' date."]
            });
        }

        var shifts = await schedules.GetEmployeeScheduleAsync(principal.GetRequiredUserId(), from, to, cancellationToken);
        var response = shifts
            .Select(shift => new ScheduleShiftDto(
                shift.Id,
                shift.Date,
                shift.StartTime,
                shift.EndTime,
                shift.RoleLabel,
                shift.Note))
            .ToArray();

        return TypedResults.Ok<IReadOnlyList<ScheduleShiftDto>>(response);
    }

    private static async Task<Ok<WeeklyAvailabilityDto>> GetAvailabilityAsync(
        ClaimsPrincipal principal,
        IAvailabilityRepository availabilityRepository,
        CancellationToken cancellationToken)
    {
        var userId = principal.GetRequiredUserId();
        var availability = await availabilityRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? AvailabilityMappings.Empty(userId);

        return TypedResults.Ok(availability.ToDto());
    }

    private static async Task<Ok<WeeklyAvailabilityDto>> PutAvailabilityAsync(
        UpdateAvailabilityRequest request,
        ClaimsPrincipal principal,
        IAvailabilityRepository availabilityRepository,
        CancellationToken cancellationToken)
    {
        var availability = AvailabilityMappings.ToRecord(principal.GetRequiredUserId(), request);
        var updated = await availabilityRepository.UpsertAsync(availability, cancellationToken);

        return TypedResults.Ok(updated.ToDto());
    }
}
