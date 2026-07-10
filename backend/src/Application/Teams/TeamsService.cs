using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Application.Teams;

public sealed class TeamsService(ITeamsRepository repository, ISystemClock clock, IValidator<CreateTeamRequest> createValidator, IValidator<UpdateTeamRequest> updateValidator, IValidator<ReorderTeamsRequest> reorderValidator) : ITeamsService
{
    public async Task<IReadOnlyList<TeamResponse>> ListAsync(bool includeArchived, CancellationToken ct) => (await repository.ListAsync(includeArchived, ct)).Select(ToResponse).ToList();
    public async Task<TeamResponse?> GetAsync(Guid id, CancellationToken ct) => (await repository.GetAsync(id, ct)) is { } team ? ToResponse(team) : null;
    public async Task<Result<TeamResponse>> CreateAsync(CreateTeamRequest request, CancellationToken ct)
    {
        if (!(await createValidator.ValidateAsync(request, ct)).IsValid)
            return Result<TeamResponse>.Failure(TeamErrors.Validation);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.NameExistsAsync(normalized, null, ct))
            return Result<TeamResponse>.Failure(TeamErrors.DuplicateName);
        var team = Team.Create(Guid.NewGuid(), name, normalized, request.TrackingLevel, (await repository.ListNonArchivedTrackedAsync(ct)).Count, clock.UtcNow);
        repository.Add(team);
        return await SaveAsync(team, ct);
    }
    public async Task<Result<TeamResponse>> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken ct)
    {
        if (id == Guid.Empty || !(await updateValidator.ValidateAsync(request, ct)).IsValid)
            return Result<TeamResponse>.Failure(TeamErrors.Validation);
        var team = await repository.GetAsync(id, ct);
        if (team is null)
            return Result<TeamResponse>.Failure(TeamErrors.NotFound);
        if (team.Status == TeamStatus.ARCHIVED)
            return Result<TeamResponse>.Failure(TeamErrors.Archived);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.NameExistsAsync(normalized, id, ct))
            return Result<TeamResponse>.Failure(TeamErrors.DuplicateName);
        team.Update(name, normalized, request.TrackingLevel, clock.UtcNow);
        return await SaveAsync(team, ct);
    }
    public Task<Result<TeamResponse>> ActivateAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (team, now) => team.Activate(now), false, ct);
    public Task<Result<TeamResponse>> DeactivateAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (team, now) => team.Deactivate(now), false, ct);
    public Task<Result<TeamResponse>> ArchiveAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (team, now) => team.Archive(now), true, ct);
    public async Task<Result<TeamResponse>> RestoreAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Result<TeamResponse>.Failure(TeamErrors.Validation);
        var team = await repository.GetAsync(id, ct);
        if (team is null)
            return Result<TeamResponse>.Failure(TeamErrors.NotFound);
        var nonArchived = await repository.ListNonArchivedTrackedAsync(ct);
        team.Restore(clock.UtcNow);
        if (team.Status != TeamStatus.ARCHIVED && !nonArchived.Contains(team))
            team.AssignDisplayOrder(nonArchived.Count, clock.UtcNow);
        return await SaveAsync(team, ct);
    }
    public async Task<Result<IReadOnlyList<TeamResponse>>> ReorderAsync(ReorderTeamsRequest request, CancellationToken ct)
    {
        if (!(await reorderValidator.ValidateAsync(request, ct)).IsValid)
            return Result<IReadOnlyList<TeamResponse>>.Failure(TeamErrors.Validation);
        var teams = await repository.ListNonArchivedTrackedAsync(ct);
        var orderedIds = request.OrderedTeamIds!;
        if (orderedIds.Count != teams.Count || !orderedIds.All(id => teams.Any(team => team.Id == id)))
            return Result<IReadOnlyList<TeamResponse>>.Failure(TeamErrors.InvalidOrder);
        var byId = teams.ToDictionary(x => x.Id);
        for (var index = 0; index < orderedIds.Count; index++)
            byId[orderedIds[index]].AssignDisplayOrder(index, clock.UtcNow);
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<IReadOnlyList<TeamResponse>>.Success((await repository.ListAsync(false, ct)).Select(ToResponse).ToList());
        }
        catch (TeamDuplicateNameException) { return Result<IReadOnlyList<TeamResponse>>.Failure(TeamErrors.DuplicateName); }
    }
    private async Task<Result<TeamResponse>> ChangeStateAsync(Guid id, Action<Team, DateTime> action, bool normalizeOrder, CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Result<TeamResponse>.Failure(TeamErrors.Validation);
        var team = await repository.GetAsync(id, ct);
        if (team is null)
            return Result<TeamResponse>.Failure(TeamErrors.NotFound);
        action(team, clock.UtcNow);
        if (normalizeOrder)
            await NormalizeNonArchivedOrderAsync(ct);
        return await SaveAsync(team, ct);
    }
    private async Task NormalizeNonArchivedOrderAsync(CancellationToken ct)
    {
        var teams = (await repository.ListNonArchivedTrackedAsync(ct)).Where(x => x.Status != TeamStatus.ARCHIVED).ToList();
        for (var i = 0; i < teams.Count; i++)
            teams[i].AssignDisplayOrder(i, clock.UtcNow);
    }
    private async Task<Result<TeamResponse>> SaveAsync(Team team, CancellationToken ct)
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<TeamResponse>.Success(ToResponse(team));
        }
        catch (TeamDuplicateNameException) { return Result<TeamResponse>.Failure(TeamErrors.DuplicateName); }
    }
    private static TeamResponse ToResponse(Team x) => new(x.Id, x.Name, x.TrackingLevel, x.Status, x.DisplayOrder, x.CreatedAtUtc, x.UpdatedAtUtc);
}
