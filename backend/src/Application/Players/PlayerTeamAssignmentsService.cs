using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Players;

public sealed class PlayerTeamAssignmentsService(IPlayerTeamAssignmentsRepository repository, ICurrentUserAccess currentUserAccess, ISystemClock clock, IValidator<CreatePlayerTeamAssignmentRequest> createValidator, IValidator<EndPlayerTeamAssignmentRequest> endValidator) : IPlayerTeamAssignmentsService
{
    public async Task<IReadOnlyList<PlayerTeamAssignmentResponse>?> ListAsync(Guid playerId, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!access.IsActive || !access.HasAccessProfile)
            return null;
        var player = await repository.GetPlayerAsync(playerId, ct);
        if (player is null)
            return null;
        var assignments = await repository.ListAsync(playerId, ct);
        var today = DateOnly.FromDateTime(clock.UtcNow);
        if (!access.IsAdmin && access.TeamScopeType == TeamScopeType.SELECTED_TEAMS && !assignments.Any(x => IsCurrent(x.Assignment, today) && access.SelectedTeamIds.Contains(x.Assignment.TeamId)))
            return null;
        return assignments.OrderByDescending(x => x.Assignment.StartDate).ThenByDescending(x => x.Assignment.EndDate is null).ThenByDescending(x => x.Assignment.EndDate).ThenBy(x => x.Assignment.Id).Select(x => ToResponse(x, today)).ToList();
    }

    public async Task<Result<PlayerTeamAssignmentResponse>> CreateAsync(Guid playerId, CreatePlayerTeamAssignmentRequest request, CancellationToken ct)
    {
        if (playerId == Guid.Empty || !(await createValidator.ValidateAsync(request, ct)).IsValid)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.Validation);
        var player = await repository.GetPlayerAsync(playerId, ct);
        if (player is null)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.NotFound);
        if (player.Status != PlayerRecordStatus.ACTIVE)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.PlayerNotActive);
        var team = await repository.GetTeamAsync(request.TeamId, ct);
        if (team is null)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.NotFound);
        if (team.Status != TeamStatus.ACTIVE)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.TeamNotActive);
        if (await repository.HasOverlapAsync(playerId, request.TeamId, request.StartDate, request.EndDate, null, ct))
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.AssignmentOverlap);
        var assignment = PlayerTeamAssignment.Create(Guid.NewGuid(), playerId, request.TeamId, request.StartDate, request.EndDate, clock.UtcNow);
        repository.Add(assignment);
        await repository.SaveChangesAsync(ct);
        return Result<PlayerTeamAssignmentResponse>.Success(ToResponse(new(assignment, team.Name), DateOnly.FromDateTime(clock.UtcNow)));
    }

    public async Task<Result<PlayerTeamAssignmentResponse>> EndAsync(Guid playerId, Guid assignmentId, EndPlayerTeamAssignmentRequest request, CancellationToken ct)
    {
        if (playerId == Guid.Empty || assignmentId == Guid.Empty || !(await endValidator.ValidateAsync(request, ct)).IsValid)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.Validation);
        var assignment = await repository.GetAssignmentAsync(assignmentId, ct);
        if (assignment is null || assignment.PlayerId != playerId)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.NotFound);
        if (assignment.EndDate is not null)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.AssignmentAlreadyEnded);
        if (request.EndDate < assignment.StartDate)
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.Validation);
        if (await repository.HasOverlapAsync(playerId, assignment.TeamId, assignment.StartDate, request.EndDate, assignmentId, ct))
            return Result<PlayerTeamAssignmentResponse>.Failure(PlayerErrors.AssignmentOverlap);
        assignment.End(request.EndDate, clock.UtcNow);
        await repository.SaveChangesAsync(ct);
        var team = await repository.GetTeamAsync(assignment.TeamId, ct);
        return Result<PlayerTeamAssignmentResponse>.Success(ToResponse(new(assignment, team!.Name), DateOnly.FromDateTime(clock.UtcNow)));
    }

    private static bool IsCurrent(PlayerTeamAssignment assignment, DateOnly today) => assignment.StartDate <= today && (assignment.EndDate is null || assignment.EndDate >= today);
    private static PlayerTeamAssignmentResponse ToResponse(PlayerTeamAssignmentReadModel model, DateOnly today) => new(model.Assignment.Id, model.Assignment.PlayerId, model.Assignment.TeamId, model.TeamName, model.Assignment.StartDate, model.Assignment.EndDate, model.Assignment.StartDate > today ? PlayerAssignmentTimingState.UPCOMING : model.Assignment.EndDate is { } end && end < today ? PlayerAssignmentTimingState.PAST : PlayerAssignmentTimingState.CURRENT, model.Assignment.CreatedAtUtc, model.Assignment.UpdatedAtUtc);
}

public sealed class CreatePlayerTeamAssignmentRequestValidator : AbstractValidator<CreatePlayerTeamAssignmentRequest>
{
    public CreatePlayerTeamAssignmentRequestValidator()
    {
        RuleFor(x => x.TeamId).NotEqual(Guid.Empty).WithErrorCode("invalid_team_id");
        RuleFor(x => x.EndDate).Must((request, endDate) => endDate is null || endDate >= request.StartDate).WithErrorCode("end_date_before_start_date");
    }
}

public sealed class EndPlayerTeamAssignmentRequestValidator : AbstractValidator<EndPlayerTeamAssignmentRequest> { }
