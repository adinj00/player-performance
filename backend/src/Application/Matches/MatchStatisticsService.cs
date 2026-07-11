using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Application.Matches;

internal sealed class MatchStatisticsService(IMatchStatisticsRepository repository, ICurrentUserAccess currentUserAccess, ISystemClock clock, IMatchStatisticsProfileService profiles) : IMatchStatisticsService
{
    public async Task<MatchReportStatisticsResponse?> GetAsync(Guid reportId, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return null;
        var read = await repository.GetReadAsync(reportId, Scope(access), ct);
        if (read is null || !CanViewStatus(access, read.Report.Status))
            return null;
        return ToResponse(read, CanEdit(access) && IsEditable(read.Report));
    }
    public async Task<Result<MatchReportStatisticsResponse>> SaveAsync(Guid reportId, SaveMatchReportStatisticsRequest request, CancellationToken ct)
    {
        if (request.PlayerStatistics is null || request.GoalkeeperStatistics is null)
            return Result<MatchReportStatisticsResponse>.Failure(MatchStatisticsErrors.Validation);
        var access = await currentUserAccess.GetAsync(ct);
        var aggregate = await repository.GetAggregateAsync(reportId, ct);
        if (aggregate is null || !CanAccess(access, aggregate.Match.TeamId))
            return Result<MatchReportStatisticsResponse>.Failure(MatchStatisticsErrors.NotFound);
        if (!CanEdit(access))
            return Result<MatchReportStatisticsResponse>.Failure(MatchStatisticsErrors.Forbidden);
        if (!IsEditable(aggregate.Report) || aggregate.Match.IsArchived || aggregate.Match.Status != MatchStatus.PLAYED)
            return Result<MatchReportStatisticsResponse>.Failure(MatchStatisticsErrors.Conflict);
        var level = aggregate.Report.AppliedTrackingLevel ?? aggregate.TeamTrackingLevel;
        var profile = profiles.Get(level);
        try
        {
            ValidateSnapshot(aggregate, request, profile);
        }
        catch (ArgumentException)
        {
            return Result<MatchReportStatisticsResponse>.Failure(MatchStatisticsErrors.Validation);
        }
        aggregate.Report.ApplyTrackingLevel(level, clock.UtcNow);
        var existing = aggregate.PlayerStatistics.ToDictionary(x => x.PlayerMatchAppearanceId);
        foreach (var row in request.PlayerStatistics)
        {
            var values = PlayerValues(row);
            if (existing.TryGetValue(row.PlayerMatchAppearanceId, out var entity))
                entity.Update(values, clock.UtcNow);
            else
            {
                entity = PlayerMatchStats.Create(Guid.NewGuid(), aggregate.Report.Id, row.PlayerMatchAppearanceId, clock.UtcNow);
                entity.Update(values, clock.UtcNow);
                repository.Add(entity);
            }
        }
        var goalkeepers = aggregate.GoalkeeperStatistics.ToDictionary(x => x.PlayerMatchAppearanceId);
        var requestedGoalkeepers = request.GoalkeeperStatistics.Select(x => x.PlayerMatchAppearanceId).ToHashSet();
        foreach (var old in aggregate.GoalkeeperStatistics.Where(x => !requestedGoalkeepers.Contains(x.PlayerMatchAppearanceId)))
            repository.Remove(old);
        foreach (var row in request.GoalkeeperStatistics)
        {
            var values = new GoalkeeperMatchStatsValues(row.Saves, row.GoalsConceded, row.CleanSheet, row.PenaltySaves);
            if (goalkeepers.TryGetValue(row.PlayerMatchAppearanceId, out var entity))
                entity.Update(values, clock.UtcNow);
            else
            {
                entity = GoalkeeperMatchStats.Create(Guid.NewGuid(), aggregate.Report.Id, row.PlayerMatchAppearanceId, clock.UtcNow);
                entity.Update(values, clock.UtcNow);
                repository.Add(entity);
            }
        }
        await repository.SaveChangesAsync(ct);
        return Result<MatchReportStatisticsResponse>.Success((await GetAsync(reportId, ct))!);
    }
    public async Task<Result> EnsureSubmissionCompleteAsync(MatchReportAggregate aggregate, CancellationToken ct)
    {
        var statistics = await repository.GetAggregateAsync(aggregate.Report.Id, ct);
        if (statistics is null)
            return Result.Failure(MatchStatisticsErrors.Validation);
        var level = statistics.Report.AppliedTrackingLevel ?? statistics.TeamTrackingLevel;
        statistics.Report.ApplyTrackingLevel(level, clock.UtcNow);
        var profile = profiles.Get(level);
        try
        {
            foreach (var row in statistics.PlayerStatistics)
            {
                var values = new PlayerMatchStatsValues(row.Goals, row.Assists, row.YellowCards, row.RedCards, row.Shots, row.ShotsOnTarget, row.PassesAttempted, row.PassesCompleted, row.KeyPasses, row.DuelsAttempted, row.DuelsWon, row.FoulsCommitted, row.FoulsWon, row.Offsides, row.BallRecoveries, row.PossessionLosses);
                EnsureDisabledNull(values, profile.PlayerFields, PlayerFields);
                PlayerMatchStats.Validate(values);
            }
            foreach (var row in statistics.GoalkeeperStatistics)
            {
                var values = new GoalkeeperMatchStatsValues(row.Saves, row.GoalsConceded, row.CleanSheet, row.PenaltySaves);
                EnsureGoalkeeperDisabledNull(values, profile.GoalkeeperFields);
                if (values.Saves < 0 || values.GoalsConceded < 0 || values.PenaltySaves < 0)
                    throw new ArgumentException();
            }
        }
        catch (ArgumentException)
        {
            return Result.Failure(MatchStatisticsErrors.Validation);
        }
        var response = ToResponse(new(statistics.Report, statistics.Match, statistics.TeamTrackingLevel, statistics.Appearances.Select(x => new MatchStatisticsAppearanceReadModel(x, "", "", null)).ToArray(), statistics.PlayerStatistics, statistics.GoalkeeperStatistics), false);
        if (!response.IsComplete)
            return Result.Failure(MatchStatisticsErrors.Validation);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
    private void ValidateSnapshot(MatchStatisticsAggregate a, SaveMatchReportStatisticsRequest request, MatchStatisticsProfile profile)
    {
        var ids = a.Appearances.Select(x => x.Id).ToHashSet();
        var playerIds = request.PlayerStatistics.Select(x => x.PlayerMatchAppearanceId).ToArray();
        if (playerIds.Length != ids.Count || playerIds.Distinct().Count() != playerIds.Length || !playerIds.All(ids.Contains))
            throw new ArgumentException();
        var goalkeeperIds = request.GoalkeeperStatistics.Select(x => x.PlayerMatchAppearanceId).ToArray();
        if (goalkeeperIds.Distinct().Count() != goalkeeperIds.Length || !goalkeeperIds.All(ids.Contains))
            throw new ArgumentException();
        foreach (var row in request.PlayerStatistics)
        {
            var values = PlayerValues(row);
            EnsureDisabledNull(values, profile.PlayerFields, PlayerFields);
            PlayerMatchStats.Validate(values);
        }
        foreach (var row in request.GoalkeeperStatistics)
        {
            var values = new GoalkeeperMatchStatsValues(row.Saves, row.GoalsConceded, row.CleanSheet, row.PenaltySaves);
            EnsureGoalkeeperDisabledNull(values, profile.GoalkeeperFields);
            if (values.Saves < 0 || values.GoalsConceded < 0 || values.PenaltySaves < 0)
                throw new ArgumentException();
        }
    }
    private MatchReportStatisticsResponse ToResponse(MatchStatisticsReadModel read, bool canEdit)
    {
        var level = read.Report.AppliedTrackingLevel ?? read.TeamTrackingLevel;
        var profile = profiles.Get(level);
        var players = read.PlayerStatistics.ToDictionary(x => x.PlayerMatchAppearanceId);
        var goalkeepers = read.GoalkeeperStatistics.ToDictionary(x => x.PlayerMatchAppearanceId);
        var playerRows = read.Appearances.Select(a => ToPlayer(a.Appearance.Id, players.GetValueOrDefault(a.Appearance.Id), profile)).ToArray();
        var goalkeeperRows = read.GoalkeeperStatistics.Select(x => ToGoalkeeper(x, profile)).ToArray();
        return new(read.Report.Id, read.Match.Id, read.Report.Status, read.Report.AppliedTrackingLevel ?? read.TeamTrackingLevel, profile.PlayerFields.Order().ToArray(), profile.GoalkeeperFields.Order().ToArray(), read.Appearances.Select(x => new MatchStatisticsAppearanceResponse(x.Appearance.Id, x.Appearance.PlayerId, x.FirstName, x.LastName, x.PreferredName, x.Appearance.MinutesPlayed)).ToArray(), playerRows, goalkeeperRows, playerRows.All(x => x.IsComplete) && goalkeeperRows.Length > 0 && goalkeeperRows.All(x => x.IsComplete), canEdit);
    }
    private static PlayerMatchStatisticsResponse ToPlayer(Guid id, PlayerMatchStats? x, MatchStatisticsProfile p)
    {
        var v = x is null ? new PlayerMatchStatsValues(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null) : new(x.Goals, x.Assists, x.YellowCards, x.RedCards, x.Shots, x.ShotsOnTarget, x.PassesAttempted, x.PassesCompleted, x.KeyPasses, x.DuelsAttempted, x.DuelsWon, x.FoulsCommitted, x.FoulsWon, x.Offsides, x.BallRecoveries, x.PossessionLosses);
        return new(id, v.Goals, v.Assists, v.YellowCards, v.RedCards, v.Shots, v.ShotsOnTarget, v.PassesAttempted, v.PassesCompleted, v.KeyPasses, v.DuelsAttempted, v.DuelsWon, v.FoulsCommitted, v.FoulsWon, v.Offsides, v.BallRecoveries, v.PossessionLosses, FieldsComplete(v, p.PlayerFields, PlayerFields));
    }
    private static GoalkeeperMatchStatisticsResponse ToGoalkeeper(GoalkeeperMatchStats x, MatchStatisticsProfile p) => new(x.PlayerMatchAppearanceId, x.Saves, x.GoalsConceded, x.CleanSheet, x.PenaltySaves, (!p.GoalkeeperFields.Contains(MatchStatisticFieldCodes.Saves) || x.Saves.HasValue) && (!p.GoalkeeperFields.Contains(MatchStatisticFieldCodes.GoalsConceded) || x.GoalsConceded.HasValue) && (!p.GoalkeeperFields.Contains(MatchStatisticFieldCodes.CleanSheet) || x.CleanSheet.HasValue) && (!p.GoalkeeperFields.Contains(MatchStatisticFieldCodes.PenaltySaves) || x.PenaltySaves.HasValue));
    private static readonly string[] PlayerFields = [MatchStatisticFieldCodes.Goals, MatchStatisticFieldCodes.Assists, MatchStatisticFieldCodes.YellowCards, MatchStatisticFieldCodes.RedCards, MatchStatisticFieldCodes.Shots, MatchStatisticFieldCodes.ShotsOnTarget, MatchStatisticFieldCodes.PassesAttempted, MatchStatisticFieldCodes.PassesCompleted, MatchStatisticFieldCodes.KeyPasses, MatchStatisticFieldCodes.DuelsAttempted, MatchStatisticFieldCodes.DuelsWon, MatchStatisticFieldCodes.FoulsCommitted, MatchStatisticFieldCodes.FoulsWon, MatchStatisticFieldCodes.Offsides, MatchStatisticFieldCodes.BallRecoveries, MatchStatisticFieldCodes.PossessionLosses];
    private static PlayerMatchStatsValues PlayerValues(SavePlayerMatchStatisticsRequest x) => new(x.Goals, x.Assists, x.YellowCards, x.RedCards, x.Shots, x.ShotsOnTarget, x.PassesAttempted, x.PassesCompleted, x.KeyPasses, x.DuelsAttempted, x.DuelsWon, x.FoulsCommitted, x.FoulsWon, x.Offsides, x.BallRecoveries, x.PossessionLosses);
    private static bool FieldsComplete(PlayerMatchStatsValues v, IReadOnlySet<string> enabled, IReadOnlyList<string> codes) => v.Values().Zip(codes).All(x => !enabled.Contains(x.Second) || x.First.HasValue);
    private static void EnsureDisabledNull(PlayerMatchStatsValues values, IReadOnlySet<string> enabled, IReadOnlyList<string> codes)
    {
        if (values.Values().Zip(codes).Any(x => !enabled.Contains(x.Second) && x.First.HasValue))
            throw new ArgumentException();
    }
    private static void EnsureGoalkeeperDisabledNull(GoalkeeperMatchStatsValues v, IReadOnlySet<string> e)
    {
        if ((!e.Contains(MatchStatisticFieldCodes.Saves) && v.Saves.HasValue) || (!e.Contains(MatchStatisticFieldCodes.GoalsConceded) && v.GoalsConceded.HasValue) || (!e.Contains(MatchStatisticFieldCodes.CleanSheet) && v.CleanSheet.HasValue) || (!e.Contains(MatchStatisticFieldCodes.PenaltySaves) && v.PenaltySaves.HasValue))
            throw new ArgumentException();
    }
    private static bool IsEditable(MatchReport r) => r.Status is MatchReportStatus.DRAFT or MatchReportStatus.NEEDS_CORRECTION;
    private static bool CanRead(CurrentUserAccess a) => a.IsActive && a.HasAccessProfile;
    private static bool CanEdit(CurrentUserAccess a) => CanRead(a) && (a.IsAdmin || a.PrimaryRole == StaffRole.DATA_OPERATOR);
    private static bool CanAccess(CurrentUserAccess a, Guid teamId) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(teamId);
    private static IReadOnlyCollection<Guid>? Scope(CurrentUserAccess a) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS ? null : a.SelectedTeamIds;
    private static bool CanViewStatus(CurrentUserAccess a, MatchReportStatus s) => a.IsAdmin || a.PrimaryRole is StaffRole.DATA_OPERATOR or StaffRole.ANALYST || s is MatchReportStatus.VERIFIED or MatchReportStatus.ARCHIVED;
}
