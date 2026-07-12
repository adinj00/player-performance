using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Matches;

internal sealed class MatchesService(IMatchesRepository repository, IMatchLineupRepository lineupRepository, IMatchStatisticsCleanup statisticsCleanup, IMatchReportWorkflowGuard workflowGuard, ICurrentUserAccess currentUserAccess, ISystemClock clock, IValidator<CreateMatchRequest> createValidator, IValidator<UpdateMatchRequest> updateValidator, IValidator<MatchListQuery> listValidator, IValidator<SaveMatchLineupRequest> lineupValidator) : IMatchesService
{
    public async Task<Result<PagedMatchListResponse>> ListAsync(MatchListQuery query, CancellationToken ct)
    {
        if (!(await listValidator.ValidateAsync(query, ct)).IsValid)
            return Result<PagedMatchListResponse>.Failure(MatchErrors.Validation);
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return Result<PagedMatchListResponse>.Failure(MatchErrors.Forbidden);
        if (query.TeamId is { } teamId && !CanAccess(access, teamId))
            return Result<PagedMatchListResponse>.Failure(MatchErrors.Forbidden);
        var scope = GetScope(access);
        var page = await repository.ListAsync(query, scope, ct);
        return Result<PagedMatchListResponse>.Success(new(page.Items.Select(ToResponse).ToArray(), query.Page, query.PageSize, page.TotalCount, page.TotalCount == 0 ? 0 : (int)Math.Ceiling(page.TotalCount / (double)query.PageSize)));
    }
    public async Task<MatchResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return null;
        var read = await repository.GetReadAsync(id, GetScope(access), ct);
        return read is null ? null : ToResponse(read);
    }
    public async Task<Result<MatchResponse>> CreateAsync(CreateMatchRequest request, CancellationToken ct)
    {
        if (!(await createValidator.ValidateAsync(request, ct)).IsValid)
            return Result<MatchResponse>.Failure(MatchErrors.Validation);
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanMutate(access) || !CanAccess(access, request.TeamId))
            return Result<MatchResponse>.Failure(MatchErrors.Forbidden);
        var references = await repository.GetActiveReferencesAsync(request.SeasonId, request.CompetitionId, request.TeamId, request.OpponentId, request.VenueId, ct);
        if (references is null || !IsInSeason(request.KickoffAtUtc, references))
            return Result<MatchResponse>.Failure(references is null ? MatchErrors.References : MatchErrors.Validation);
        if (await repository.ExactDuplicateExistsAsync(request.TeamId, request.OpponentId, request.KickoffAtUtc, ct))
            return Result<MatchResponse>.Failure(MatchErrors.Duplicate);
        var match = Match.Create(Guid.NewGuid(), request.SeasonId, request.CompetitionId, request.TeamId, request.OpponentId, request.VenueId, request.KickoffAtUtc, request.Round, request.LocationType, clock.UtcNow);
        repository.Add(match);
        await repository.SaveChangesAsync(ct);
        return Result<MatchResponse>.Success(await GetResponseAsync(match.Id, ct));
    }
    public async Task<Result<MatchResponse>> UpdateAsync(Guid id, UpdateMatchRequest request, CancellationToken ct)
    {
        if (id == Guid.Empty || !(await updateValidator.ValidateAsync(request, ct)).IsValid)
            return Result<MatchResponse>.Failure(MatchErrors.Validation);
        var access = await currentUserAccess.GetAsync(ct);
        var match = await repository.GetAsync(id, ct);
        if (match is null)
            return Result<MatchResponse>.Failure(MatchErrors.NotFound);
        if (!CanMutate(access) || !CanAccess(access, match.TeamId))
            return Result<MatchResponse>.Failure(MatchErrors.Forbidden);
        var workflow = await workflowGuard.EnsureEditableAsync(id, ct);
        if (!workflow.IsSuccess)
            return Result<MatchResponse>.Failure(MatchErrors.WorkflowLocked);
        var references = await repository.GetActiveReferencesAsync(request.SeasonId, request.CompetitionId, match.TeamId, request.OpponentId, request.VenueId, ct);
        if (references is null || !IsInSeason(request.KickoffAtUtc, references))
            return Result<MatchResponse>.Failure(references is null ? MatchErrors.References : MatchErrors.Validation);
        try
        { match.UpdateMetadata(request.SeasonId, request.CompetitionId, request.OpponentId, request.VenueId, request.KickoffAtUtc, request.Round, request.LocationType, request.Status, request.TeamScore, request.OpponentScore, clock.UtcNow); }
        catch (InvalidOperationException) { return Result<MatchResponse>.Failure(MatchErrors.Conflict); }
        catch (ArgumentOutOfRangeException) { return Result<MatchResponse>.Failure(MatchErrors.Validation); }
        await repository.SaveChangesAsync(ct);
        return Result<MatchResponse>.Success(await GetResponseAsync(match.Id, ct));
    }
    public Task<Result<MatchResponse>> ArchiveAsync(Guid id, CancellationToken ct) => ChangeArchiveStateAsync(id, true, ct);
    public Task<Result<MatchResponse>> RestoreAsync(Guid id, CancellationToken ct) => ChangeArchiveStateAsync(id, false, ct);
    public async Task<MatchLineupResponse?> GetLineupAsync(Guid id, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return null;
        var read = await lineupRepository.GetReadAsync(id, GetScope(access), ct);
        return read is null ? null : ToLineupResponse(read);
    }

    public async Task<Result<IReadOnlyList<EligibleLineupPlayerResponse>>> GetEligibleLineupPlayersAsync(Guid id, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        var aggregate = await lineupRepository.GetAsync(id, ct);
        if (aggregate is null || !CanAccess(access, aggregate.Match.TeamId))
            return Result<IReadOnlyList<EligibleLineupPlayerResponse>>.Failure(MatchErrors.NotFound);
        if (!CanMutate(access))
            return Result<IReadOnlyList<EligibleLineupPlayerResponse>>.Failure(MatchErrors.Forbidden);
        if (aggregate.Match.IsArchived || aggregate.Match.Status == MatchStatus.CANCELLED)
            return Result<IReadOnlyList<EligibleLineupPlayerResponse>>.Failure(MatchErrors.Conflict);

        var players = await lineupRepository.GetEligiblePlayersAsync(
            aggregate.Match.TeamId,
            DateOnly.FromDateTime(aggregate.Match.KickoffAtUtc),
            ct);
        return Result<IReadOnlyList<EligibleLineupPlayerResponse>>.Success(players);
    }

    public async Task<Result<MatchLineupResponse>> SaveLineupAsync(Guid id, SaveMatchLineupRequest request, CancellationToken ct)
    {
        if (id == Guid.Empty || !(await lineupValidator.ValidateAsync(request, ct)).IsValid)
            return Result<MatchLineupResponse>.Failure(MatchErrors.Validation);

        var access = await currentUserAccess.GetAsync(ct);
        var aggregate = await lineupRepository.GetAsync(id, ct);
        if (aggregate is null)
            return Result<MatchLineupResponse>.Failure(MatchErrors.NotFound);
        if (!CanMutate(access) || !CanAccess(access, aggregate.Match.TeamId))
            return Result<MatchLineupResponse>.Failure(MatchErrors.Forbidden);
        if (aggregate.Match.IsArchived || aggregate.Match.Status == MatchStatus.CANCELLED)
            return Result<MatchLineupResponse>.Failure(MatchErrors.Conflict);
        var workflow = await workflowGuard.EnsureEditableAsync(id, ct);
        if (!workflow.IsSuccess)
            return Result<MatchLineupResponse>.Failure(MatchErrors.WorkflowLocked);

        try
        {
            ValidateLineupSnapshot(aggregate.Match, request);
        }
        catch (InvalidOperationException)
        {
            return Result<MatchLineupResponse>.Failure(MatchErrors.Conflict);
        }

        var requestedPlayerIds = request.Entries.Select(x => x.PlayerId).ToHashSet();
        var existingPlayerIds = aggregate.Entries.Select(x => x.PlayerId).ToHashSet();
        var newPlayerIds = requestedPlayerIds.Except(existingPlayerIds).ToArray();
        var eligibility = await lineupRepository.GetNewPlayerEligibilityAsync(newPlayerIds, aggregate.Match.TeamId, DateOnly.FromDateTime(aggregate.Match.KickoffAtUtc), ct);
        if (eligibility.Values.Any(x => !x))
            return Result<MatchLineupResponse>.Failure(MatchErrors.Validation);

        await ApplyLineupSnapshotAsync(aggregate, request, ct);
        await lineupRepository.SaveChangesAsync(ct);
        return Result<MatchLineupResponse>.Success(ToLineupResponse((await lineupRepository.GetReadAsync(id, null, ct))!));
    }
    private async Task<Result<MatchResponse>> ChangeArchiveStateAsync(Guid id, bool archive, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!access.IsAdmin)
            return Result<MatchResponse>.Failure(MatchErrors.Forbidden);
        var match = await repository.GetAsync(id, ct);
        if (match is null)
            return Result<MatchResponse>.Failure(MatchErrors.NotFound);
        if (archive && !(await workflowGuard.EnsureMatchCanBeArchivedAsync(id, ct)).IsSuccess)
            return Result<MatchResponse>.Failure(MatchErrors.Conflict);
        if (archive)
            match.Archive(clock.UtcNow);
        else
            match.Restore(clock.UtcNow);
        await repository.SaveChangesAsync(ct);
        return Result<MatchResponse>.Success(await GetResponseAsync(match.Id, ct));
    }
    private static bool CanRead(CurrentUserAccess access) => access.IsActive && access.HasAccessProfile;
    private static bool CanMutate(CurrentUserAccess access) => CanRead(access) && (access.IsAdmin || access.PrimaryRole == StaffRole.DATA_OPERATOR);
    private static bool CanAccess(CurrentUserAccess access, Guid teamId) => access.IsAdmin || access.TeamScopeType == TeamScopeType.ALL_TEAMS || access.SelectedTeamIds.Contains(teamId);
    private static IReadOnlyCollection<Guid>? GetScope(CurrentUserAccess access) => access.IsAdmin || access.TeamScopeType == TeamScopeType.ALL_TEAMS ? null : access.SelectedTeamIds;
    private static bool IsInSeason(DateTime kickoffAtUtc, MatchReferences references) { var date = DateOnly.FromDateTime(kickoffAtUtc); return date >= references.SeasonStartDate && date <= references.SeasonEndDate; }
    private async Task<MatchResponse> GetResponseAsync(Guid id, CancellationToken ct) => ToResponse((await repository.GetReadAsync(id, null, ct))!);
    private static MatchResponse ToResponse(MatchReadModel x) => new(x.Match.Id, new(x.Match.SeasonId, x.SeasonName), new(x.Match.CompetitionId, x.CompetitionName), new(x.Match.TeamId, x.TeamName), new(x.Match.OpponentId, x.OpponentName), x.Match.VenueId is { } venueId ? new(venueId, x.VenueName!) : null, x.Match.KickoffAtUtc, x.Match.Round, x.Match.LocationType, x.Match.Status, x.Match.TeamScore, x.Match.OpponentScore, x.Match.IsArchived, x.Match.CreatedAtUtc, x.Match.UpdatedAtUtc);
    private static void ValidateLineupSnapshot(Match match, SaveMatchLineupRequest request)
    {
        var playerIds = request.Entries.Select(x => x.PlayerId).ToHashSet();
        if (playerIds.Count != request.Entries.Count || request.Entries.Any(x => !Enum.IsDefined(x.Role)) || request.CaptainPlayerId.HasValue && !playerIds.Contains(request.CaptainPlayerId.Value))
            throw new InvalidOperationException("The lineup is invalid.");

        if (match.Status is MatchStatus.SCHEDULED or MatchStatus.POSTPONED)
        {
            if (request.Appearances.Count != 0 || request.Substitutions.Count != 0)
                throw new InvalidOperationException("Preliminary lineups cannot contain concrete participation.");
            return;
        }

        if (match.Status != MatchStatus.PLAYED)
            throw new InvalidOperationException("The match status does not allow lineup mutation.");

        MatchParticipationRules.ValidatePlayedSnapshot(
            request.Entries.Select(x => new MatchLineupSnapshotEntry(x.PlayerId, x.Role)).ToArray(),
            request.CaptainPlayerId,
            request.Appearances.Select(x => new MatchAppearanceSnapshotEntry(x.PlayerId, x.MinutesPlayed)).ToArray(),
            request.Substitutions.Select(x => new MatchSubstitutionSnapshotEntry(x.PlayerOutId, x.PlayerInId, x.Minute, x.StoppageTimeMinute, x.Sequence)).ToArray());
    }

    private async Task ApplyLineupSnapshotAsync(MatchLineupAggregate aggregate, SaveMatchLineupRequest request, CancellationToken ct)
    {
        var lineup = aggregate.Lineup;
        if (lineup is null)
        {
            lineup = MatchLineup.Create(Guid.NewGuid(), aggregate.Match.Id, request.Formation, request.CaptainPlayerId, clock.UtcNow);
            lineupRepository.Add(lineup);
        }
        else
        {
            lineup.Update(request.Formation, request.CaptainPlayerId, clock.UtcNow);
        }

        var entries = aggregate.Entries.ToDictionary(x => x.PlayerId);
        foreach (var existing in aggregate.Entries.Where(x => !request.Entries.Any(y => y.PlayerId == x.PlayerId)))
            lineupRepository.RemoveEntry(existing);
        foreach (var entry in request.Entries)
        {
            if (entries.TryGetValue(entry.PlayerId, out var existing))
                existing.UpdateRole(entry.Role, clock.UtcNow);
            else
                lineupRepository.AddEntry(MatchLineupEntry.Create(Guid.NewGuid(), aggregate.Match.Id, entry.PlayerId, entry.Role, clock.UtcNow));
        }

        var appearances = aggregate.Appearances.ToDictionary(x => x.PlayerId);
        var removedAppearances = aggregate.Appearances.Where(x => !request.Appearances.Any(y => y.PlayerId == x.PlayerId)).ToArray();
        await statisticsCleanup.RemoveForAppearancesAsync(removedAppearances.Select(x => x.Id).ToArray(), ct);
        foreach (var existing in removedAppearances)
            lineupRepository.RemoveAppearance(existing);
        foreach (var appearance in request.Appearances)
        {
            if (appearances.TryGetValue(appearance.PlayerId, out var existing))
                existing.UpdateMinutes(appearance.MinutesPlayed, clock.UtcNow);
            else
                lineupRepository.AddAppearance(PlayerMatchAppearance.Create(Guid.NewGuid(), aggregate.Match.Id, appearance.PlayerId, appearance.MinutesPlayed, clock.UtcNow));
        }

        foreach (var substitution in aggregate.Substitutions)
            lineupRepository.RemoveSubstitution(substitution);
        foreach (var substitution in request.Substitutions)
            lineupRepository.AddSubstitution(MatchSubstitution.Create(Guid.NewGuid(), aggregate.Match.Id, substitution.PlayerOutId, substitution.PlayerInId, substitution.Minute, substitution.StoppageTimeMinute, substitution.Sequence, clock.UtcNow));
    }

    private static MatchLineupResponse ToLineupResponse(MatchLineupReadModel read)
    {
        var entries = read.Entries.Select(x => new MatchLineupEntryResponse(x.Entry.Id, new(x.Entry.PlayerId, x.FirstName, x.LastName, x.PreferredName), x.Entry.Role)).ToArray();
        var captain = read.Lineup?.CaptainPlayerId is { } captainId ? entries.SingleOrDefault(x => x.Player.Id == captainId)?.Player : null;
        return new(read.Match.Id, read.Match.TeamId, read.Match.Status, read.Match.IsArchived, read.Lineup?.Formation, captain, entries, read.Appearances.Select(x => new MatchAppearanceResponse(x.Appearance.Id, x.Appearance.PlayerId, x.Appearance.MinutesPlayed)).ToArray(), read.Substitutions.Select(x => new MatchSubstitutionResponse(x.Id, x.PlayerOutId, x.PlayerInId, x.Minute, x.StoppageTimeMinute, x.Sequence)).ToArray());
    }
}
