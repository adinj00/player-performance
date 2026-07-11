using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Matches;

internal sealed class MatchesService(IMatchesRepository repository, ICurrentUserAccess currentUserAccess, ISystemClock clock, IValidator<CreateMatchRequest> createValidator, IValidator<UpdateMatchRequest> updateValidator, IValidator<MatchListQuery> listValidator) : IMatchesService
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
        return Result<PagedMatchListResponse>.Success(new(page.Items.Select(ToResponse).ToArray(), query.Page, query.PageSize, page.TotalCount, page.TotalCount == 0 ? 0 : (int) Math.Ceiling(page.TotalCount / (double) query.PageSize)));
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
    private async Task<Result<MatchResponse>> ChangeArchiveStateAsync(Guid id, bool archive, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!access.IsAdmin)
            return Result<MatchResponse>.Failure(MatchErrors.Forbidden);
        var match = await repository.GetAsync(id, ct);
        if (match is null)
            return Result<MatchResponse>.Failure(MatchErrors.NotFound);
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
}
