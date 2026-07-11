using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Players;

public sealed class PlayersService(IPlayersRepository repository, IPlayerTeamAssignmentsRepository assignmentsRepository, ICurrentUserAccess currentUserAccess, ISystemClock clock, IValidator<CreatePlayerRequest> createValidator, IValidator<UpdatePlayerRequest> updateValidator, IValidator<PlayerListQuery> listValidator) : IPlayersService
{
    public async Task<Result<PagedPlayerListResponse>> ListAsync(PlayerListQuery query, CancellationToken ct)
    {
        if (!(await listValidator.ValidateAsync(query, ct)).IsValid)
            return Result<PagedPlayerListResponse>.Failure(PlayerErrors.Validation);

        var normalizedQuery = query with { Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim() };
        if (normalizedQuery.Status == PlayerRecordStatus.ARCHIVED && !normalizedQuery.IncludeArchived)
            normalizedQuery = normalizedQuery with { IncludeArchived = true };

        var access = await currentUserAccess.GetAsync(ct);
        if (!access.IsActive || !access.HasAccessProfile)
            return Result<PagedPlayerListResponse>.Failure(PlayerErrors.Forbidden);
        if (normalizedQuery.TeamId is { } teamId && access.TeamScopeType == TeamScopeType.SELECTED_TEAMS && !access.SelectedTeamIds.Contains(teamId))
            return Result<PagedPlayerListResponse>.Failure(PlayerErrors.Forbidden);

        var today = DateOnly.FromDateTime(clock.UtcNow);
        var scope = access.IsAdmin || access.TeamScopeType == TeamScopeType.ALL_TEAMS ? null : access.SelectedTeamIds;
        var page = await repository.ListAsync(normalizedQuery, today, scope, ct);
        var totalPages = page.TotalCount == 0 ? 0 : (int) Math.Ceiling(page.TotalCount / (double) normalizedQuery.PageSize);
        return Result<PagedPlayerListResponse>.Success(new(page.Items.Select(x => ToResponse(x.Player, x.CurrentAssignments)).ToList(), normalizedQuery.Page, normalizedQuery.PageSize, page.TotalCount, totalPages));
    }

    public async Task<PlayerSummaryResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!access.IsActive || !access.HasAccessProfile)
            return null;
        var scope = access.IsAdmin || access.TeamScopeType == TeamScopeType.ALL_TEAMS ? null : access.SelectedTeamIds;
        var player = await repository.GetReadAsync(id, DateOnly.FromDateTime(clock.UtcNow), scope, ct);
        return player is null ? null : ToResponse(player.Player, player.CurrentAssignments);
    }

    public async Task<Result<PlayerSummaryResponse>> CreateAsync(CreatePlayerRequest request, CancellationToken ct)
    {
        if (!(await createValidator.ValidateAsync(request, ct)).IsValid)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.Validation);

        var player = Player.Create(Guid.NewGuid(), request.FirstName!, request.LastName!, request.PreferredName, request.DateOfBirth, clock.UtcNow);
        repository.Add(player);
        await repository.SaveChangesAsync(ct);
        return Result<PlayerSummaryResponse>.Success(ToResponse(player, []));
    }

    public async Task<Result<PlayerSummaryResponse>> UpdateAsync(Guid id, UpdatePlayerRequest request, CancellationToken ct)
    {
        if (id == Guid.Empty || !(await updateValidator.ValidateAsync(request, ct)).IsValid)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.Validation);

        var player = await repository.GetAsync(id, ct);
        if (player is null)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.NotFound);
        if (player.Status == PlayerRecordStatus.ARCHIVED)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.Archived);

        player.UpdateProfile(request.FirstName!, request.LastName!, request.PreferredName, request.DateOfBirth, clock.UtcNow);
        await repository.SaveChangesAsync(ct);
        return Result<PlayerSummaryResponse>.Success(ToResponse(player, await repository.GetCurrentAssignmentsAsync(player.Id, DateOnly.FromDateTime(clock.UtcNow), ct)));
    }

    public Task<Result<PlayerSummaryResponse>> ActivateAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (player, now) => player.Activate(now), false, ct);
    public Task<Result<PlayerSummaryResponse>> DeactivateAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (player, now) => player.Deactivate(now), true, ct);
    public Task<Result<PlayerSummaryResponse>> ArchiveAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (player, now) => player.Archive(now), true, ct);
    public Task<Result<PlayerSummaryResponse>> RestoreAsync(Guid id, CancellationToken ct) => ChangeStateAsync(id, static (player, now) => player.Restore(now), false, ct);

    private async Task<Result<PlayerSummaryResponse>> ChangeStateAsync(Guid id, Action<Player, DateTime> transition, bool blocksCurrentAssignments, CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.Validation);

        var player = await repository.GetAsync(id, ct);
        if (player is null)
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.NotFound);
        if (blocksCurrentAssignments && await assignmentsRepository.HasCurrentAssignmentsAsync(id, DateOnly.FromDateTime(clock.UtcNow), ct))
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.PlayerHasCurrentAssignments);

        try
        {
            transition(player, clock.UtcNow);
        }
        catch (InvalidOperationException)
        {
            return Result<PlayerSummaryResponse>.Failure(PlayerErrors.Archived);
        }

        await repository.SaveChangesAsync(ct);
        return Result<PlayerSummaryResponse>.Success(ToResponse(player, await repository.GetCurrentAssignmentsAsync(player.Id, DateOnly.FromDateTime(clock.UtcNow), ct)));
    }

    private static PlayerSummaryResponse ToResponse(Player player, IReadOnlyList<CurrentPlayerTeamAssignment> assignments) => new(player.Id, player.FirstName, player.LastName, player.PreferredName, player.PreferredName ?? $"{player.FirstName} {player.LastName}", player.DateOfBirth, player.Status, assignments.Select(x => new CurrentPlayerTeamAssignmentResponse(x.TeamId, x.TeamName, x.StartDate, x.EndDate)).ToList(), player.CreatedAtUtc, player.UpdatedAtUtc);
}
