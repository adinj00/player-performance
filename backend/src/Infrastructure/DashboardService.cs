using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Application.Dashboard;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Imports;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Medical;
using PlayerPerformance.Domain.Physical;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Dashboard;

internal sealed class DashboardService(AppDbContext db, ICurrentUserAccess userAccess, ISystemClock clock, IPhysicalMetricCatalog metrics, IMatchStatisticsProfileService statisticProfiles) : IDashboardService
{
    public async Task<DashboardContextOptionsResponse?> GetContextOptionsAsync(CancellationToken ct)
    {
        var access = await userAccess.GetAsync(ct);
        if (!Readable(access))
            return null;
        var teams = db.Teams.AsNoTracking().Where(t => t.Status != TeamStatus.ARCHIVED);
        if (!AllTeams(access))
            teams = teams.Where(t => access.SelectedTeamIds.Contains(t.Id));
        return new(await teams.OrderBy(t => t.DisplayOrder).ThenBy(t => t.Id).Select(t => new DashboardTeamOption(t.Id, t.Name, t.Status, t.DisplayOrder)).ToListAsync(ct),
            await db.Seasons.AsNoTracking().OrderByDescending(s => s.StartDate).ThenBy(s => s.Id).Select(s => new DashboardSeasonOption(s.Id, s.Name, s.StartDate, s.EndDate, s.IsArchived)).ToListAsync(ct));
    }

    public async Task<DashboardReadResult> GetOverviewAsync(Guid teamId, Guid seasonId, CancellationToken ct)
    {
        if (teamId == Guid.Empty || seasonId == Guid.Empty)
            return Fail("invalid_context");
        var access = await userAccess.GetAsync(ct);
        if (!Readable(access))
            return Fail("forbidden");
        if (!AllTeams(access) && !access.SelectedTeamIds.Contains(teamId))
            return Fail("forbidden");
        var context = await (from t in db.Teams.AsNoTracking() join s in db.Seasons.AsNoTracking() on 1 equals 1 where t.Id == teamId && t.Status != TeamStatus.ARCHIVED && s.Id == seasonId select new { t, s }).SingleOrDefaultAsync(ct);
        if (context is null)
            return Fail("not_found");
        var now = clock.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var full = access.IsAdmin || access.PrimaryRole is StaffRole.DATA_OPERATOR or StaffRole.ANALYST;
        var matches = await (from m in db.Matches.AsNoTracking()
                             join c in db.Competitions.AsNoTracking() on m.CompetitionId equals c.Id
                             join o in db.Opponents.AsNoTracking() on m.OpponentId equals o.Id
                             join r in db.MatchReports.AsNoTracking() on m.Id equals r.MatchId into reports
                             from r in reports.DefaultIfEmpty()
                             where m.TeamId == teamId && m.SeasonId == seasonId && m.Status == MatchStatus.PLAYED && !m.IsArchived
                             orderby m.KickoffAtUtc descending, m.Id descending
                             select new { m, c.Name, OpponentName = o.Name, Report = r }).Take(5).ToListAsync(ct);
        var recent = matches.Select(x => new DashboardRecentMatchResponse(x.m.Id, x.m.KickoffAtUtc, x.Name, x.OpponentName, x.m.LocationType, x.m.TeamScore!.Value, x.m.OpponentScore!.Value, Result(x.m.TeamScore.Value, x.m.OpponentScore.Value), x.Report is not null && (full || x.Report.Status is MatchReportStatus.VERIFIED or MatchReportStatus.ARCHIVED) ? x.Report.Status : null)).ToArray();
        var form = new DashboardTeamFormResponse(recent.Length, recent.Count(x => x.Result == "WIN"), recent.Count(x => x.Result == "DRAW"), recent.Count(x => x.Result == "LOSS"), recent.Sum(x => x.TeamScore), recent.Sum(x => x.OpponentScore), recent.Select(x => new DashboardFormMatchResponse(x.Id, x.Result)).ToArray());
        var workflow = await Workflow(teamId, seasonId, full, ct);
        var availability = await Availability(teamId, today, ct);
        var leaders = await Leaders(teamId, seasonId, ct);
        var workloads = await Workloads(teamId, context.s.StartDate, context.s.EndDate, ct);
        var alerts = await Alerts(teamId, seasonId, context.s.StartDate, context.s.EndDate, full, CanImport(access), workflow, availability, workloads, ct);
        var response = new DashboardOverviewResponse(new(new(context.t.Id, context.t.Name, context.t.Status, context.t.DisplayOrder), new(context.s.Id, context.s.Name, context.s.StartDate, context.s.EndDate, context.s.IsArchived), now), recent, form, workflow, availability, leaders, workloads, alerts, now);
        return new(response, null);
    }

    private async Task<DashboardReportWorkflowResponse> Workflow(Guid team, Guid season, bool full, CancellationToken ct)
    {
        var q = from m in db.Matches.AsNoTracking() join r in db.MatchReports.AsNoTracking() on m.Id equals r.MatchId into rs from r in rs.DefaultIfEmpty() where m.TeamId == team && m.SeasonId == season && !m.IsArchived select new { m, r };
        if (!full)
        { var final = await q.Where(x => x.r != null && (x.r.Status == MatchReportStatus.VERIFIED || x.r.Status == MatchReportStatus.ARCHIVED)).GroupBy(x => x.r!.Status).Select(g => new DashboardReportStatusCount(g.Key, g.Count())).ToListAsync(ct); return new("FINAL_ONLY", final, null, null); }
        var counts = await q.Where(x => x.r != null).GroupBy(x => x.r!.Status).Select(g => new DashboardReportStatusCount(g.Key, g.Count())).ToListAsync(ct);
        var played = await q.CountAsync(x => x.m.Status == MatchStatus.PLAYED, ct);
        var missing = await q.CountAsync(x => x.m.Status == MatchStatus.PLAYED && x.r == null, ct);
        return new("FULL_WORKFLOW", counts, played, missing);
    }
    private async Task<DashboardAvailabilityResponse> Availability(Guid team, DateOnly today, CancellationToken ct)
    {
        var rows = await (from a in db.PlayerTeamAssignments.AsNoTracking() join p in db.Players.AsNoTracking() on a.PlayerId equals p.Id join av in db.PlayerAvailabilities.AsNoTracking() on new { a.PlayerId, a.TeamId } equals new { av.PlayerId, av.TeamId } into avs from av in avs.DefaultIfEmpty() join r in db.PlayerAvailabilityRevisions.AsNoTracking() on av.CurrentRevisionId equals r.Id into rs from r in rs.DefaultIfEmpty() where a.TeamId == team && a.StartDate <= today && (a.EndDate == null || a.EndDate >= today) && p.Status != PlayerRecordStatus.ARCHIVED select r == null ? AvailabilityStatus.UNKNOWN : r.Status).ToListAsync(ct);
        return new("CURRENT_TEAM_SNAPSHOT", today, rows.Count, rows.Count(x => x == AvailabilityStatus.AVAILABLE), rows.Count(x => x == AvailabilityStatus.LIMITED), rows.Count(x => x == AvailabilityStatus.UNAVAILABLE), rows.Count(x => x == AvailabilityStatus.REHAB), rows.Count(x => x == AvailabilityStatus.UNKNOWN));
    }
    private async Task<DashboardStatisticsLeadersResponse> Leaders(Guid team, Guid season, CancellationToken ct)
    {
        var reports = from r in db.MatchReports.AsNoTracking() join m in db.Matches.AsNoTracking() on r.MatchId equals m.Id where m.TeamId == team && m.SeasonId == season && !m.IsArchived && m.Status == MatchStatus.PLAYED && (r.Status == MatchReportStatus.VERIFIED || r.Status == MatchReportStatus.ARCHIVED) select new { r, m };
        var reportCount = await reports.CountAsync(ct);
        if (reportCount == 0)
            return new([], "NO_FINAL_REPORTS");
        var data = await (from s in db.PlayerMatchStats.AsNoTracking() join a in db.PlayerMatchAppearances.AsNoTracking() on s.PlayerMatchAppearanceId equals a.Id join p in db.Players.AsNoTracking() on a.PlayerId equals p.Id join r in reports on s.MatchReportId equals r.r.Id select new { s, a, p, r.r.AppliedTrackingLevel }).ToListAsync(ct);
        var gk = await (from s in db.GoalkeeperMatchStats.AsNoTracking() join a in db.PlayerMatchAppearances.AsNoTracking() on s.PlayerMatchAppearanceId equals a.Id join p in db.Players.AsNoTracking() on a.PlayerId equals p.Id join r in reports on s.MatchReportId equals r.r.Id select new { s, a, p, r.r.AppliedTrackingLevel }).ToListAsync(ct);
        var definitions = new LeaderMetric[] { new("GOALS", MatchStatisticFieldCodes.Goals, x => x.Goals, null), new("ASSISTS", MatchStatisticFieldCodes.Assists, x => x.Assists, null), new("SHOTS_ON_TARGET", MatchStatisticFieldCodes.ShotsOnTarget, x => x.ShotsOnTarget, null), new("KEY_PASSES", MatchStatisticFieldCodes.KeyPasses, x => x.KeyPasses, null), new("DUELS_WON", MatchStatisticFieldCodes.DuelsWon, x => x.DuelsWon, null), new("BALL_RECOVERIES", MatchStatisticFieldCodes.BallRecoveries, x => x.BallRecoveries, null), new("SAVES", MatchStatisticFieldCodes.Saves, null, x => x.Saves), new("CLEAN_SHEETS", MatchStatisticFieldCodes.CleanSheet, null, x => x.CleanSheet == true ? 1 : 0) };
        var groups = new List<DashboardLeaderGroupResponse>();
        foreach (var d in definitions)
        {
            var source = d.GoalkeeperValue is not null ? gk.Where(x => IsEnabled(x.AppliedTrackingLevel, d.FieldCode, true)).Select(x => (Id: x.p.Id, Name: x.p.PreferredName ?? (x.p.FirstName + " " + x.p.LastName), Appearance: x.a.Id, Match: x.a.MatchId, Value: d.GoalkeeperValue(x.s) ?? 0)) : data.Where(x => IsEnabled(x.AppliedTrackingLevel, d.FieldCode, false)).Select(x => (Id: x.p.Id, Name: x.p.PreferredName ?? (x.p.FirstName + " " + x.p.LastName), Appearance: x.a.Id, Match: x.a.MatchId, Value: d.PlayerValue!(x.s) ?? 0));
            var ordered = source.GroupBy(x => new { x.Id, x.Name }).Select(x => new { x.Key, Value = x.Sum(y => y.Value), Appearance = x.Select(y => y.Appearance).Distinct().Count(), Match = x.Select(y => y.Match).Distinct().Count() }).Where(x => x.Value > 0).OrderByDescending(x => x.Value).ThenBy(x => x.Key.Name).ThenBy(x => x.Key.Id).ToList();
            if (ordered.Count == 0)
                continue;
            var rows = ordered.Take(3).Select((x, i) => new DashboardLeaderResponse(1 + ordered.Take(i).Count(y => y.Value > x.Value), new(x.Key.Id, x.Key.Name), x.Value, x.Appearance, x.Match)).ToArray();
            groups.Add(new(d.Code, "INTEGER", reportCount, rows, ordered.Count > 3 && ordered[3].Value == rows[^1].Value));
        }
        return new(groups, groups.Count == 0 ? "NO_POSITIVE_LEADER_VALUES" : null);
    }
    private async Task<DashboardPhysicalWorkloadResponse> Workloads(Guid team, DateOnly start, DateOnly end, CancellationToken ct)
    {
        var aggregateDefinitions = metrics.All.Where(x => x.AggregationKind is PhysicalMetricAggregationKind.SUM or PhysicalMetricAggregationKind.MAX).ToArray();
        var metricCodes = aggregateDefinitions.Select(x => x.Code).ToArray();
        var source = from workload in db.PlayerPhysicalWorkloads.AsNoTracking()
                     join revision in db.PhysicalWorkloadRevisions.AsNoTracking() on workload.CurrentRevisionId equals revision.Id
                     join value in db.PhysicalMetricValues.AsNoTracking() on revision.Id equals value.PhysicalWorkloadRevisionId
                     where workload.TeamId == team && workload.OccurredOn >= start && workload.OccurredOn <= end && metricCodes.Contains(value.MetricCode)
                     select new { workload, value, ContextType = workload.TrainingSessionParticipantId.HasValue ? "TRAINING" : "MATCH" };
        var groups = source.GroupBy(x => new { x.ContextType, x.value.MetricCode, x.value.UnitCode, x.value.ThresholdValue, x.value.ThresholdUnitCode, x.value.ThresholdDirection, x.value.ThresholdScope, x.value.MethodKey, x.value.MethodVersion })
            .Select(g => new
            {
                g.Key,
                Sum = g.Sum(x => x.value.Value),
                Max = g.Max(x => x.value.Value),
                WorkloadCount = g.Select(x => x.workload.Id).Distinct().Count(),
                PlayerCount = g.Select(x => x.workload.PlayerId).Distinct().Count(),
                ContextCount = g.Select(x => x.workload.Id).Distinct().Count(),
                FirstOccurredOn = g.Min(x => x.workload.OccurredOn),
                LastOccurredOn = g.Max(x => x.workload.OccurredOn)
            });
        var availableGroupCount = await groups.CountAsync(ct);
        if (availableGroupCount == 0)
            return new([], false, 0, 0, false, 0, "NO_CONFIRMED_WORKLOADS");
        var splitMetricContextCount = await groups.GroupBy(x => new { x.Key.ContextType, x.Key.MetricCode }).CountAsync(x => x.Count() > 1, ct);
        var bounded = await groups.OrderBy(x => x.Key.MetricCode == "TOTAL_DISTANCE_METERS" ? 0 : x.Key.MetricCode == "HIGH_SPEED_RUNNING_DISTANCE_METERS" ? 1 : x.Key.MetricCode == "SPRINT_DISTANCE_METERS" ? 2 : x.Key.MetricCode == "SPRINT_COUNT" ? 3 : x.Key.MetricCode == "MAX_SPEED_METERS_PER_SECOND" ? 4 : x.Key.MetricCode == "ACCELERATION_COUNT" ? 5 : x.Key.MetricCode == "DECELERATION_COUNT" ? 6 : x.Key.MetricCode == "PLAYER_LOAD_ARBITRARY_UNITS" ? 7 : 8).ThenBy(x => x.Key.ContextType).ThenByDescending(x => x.WorkloadCount).ThenBy(x => x.Key.ThresholdValue).ThenBy(x => x.Key.MethodKey).Take(24).ToListAsync(ct);
        var output = bounded.Select(x =>
        {
            var definition = aggregateDefinitions.Single(d => d.Code == x.Key.MetricCode);
            return new DashboardWorkloadGroupResponse(x.Key.ContextType, x.Key.MetricCode, ComparabilityKey(x.Key.ThresholdValue, x.Key.ThresholdUnitCode, x.Key.ThresholdDirection, x.Key.ThresholdScope, x.Key.MethodKey, x.Key.MethodVersion), x.Key.UnitCode, new(x.Key.ThresholdValue, x.Key.ThresholdUnitCode, x.Key.ThresholdDirection, x.Key.ThresholdScope), new(x.Key.MethodKey, x.Key.MethodVersion), definition.AggregationKind, definition.AggregationKind == PhysicalMetricAggregationKind.SUM ? x.Sum : x.Max, x.WorkloadCount, x.PlayerCount, x.ContextCount, x.FirstOccurredOn, x.LastOccurredOn);
        }).ToArray();
        return new(output, availableGroupCount > output.Length, availableGroupCount, output.Length, splitMetricContextCount > 0, splitMetricContextCount, null);
    }
    private async Task<IReadOnlyList<DashboardQualityAlertResponse>> Alerts(Guid team, Guid season, DateOnly start, DateOnly end, bool full, bool imports, DashboardReportWorkflowResponse workflow, DashboardAvailabilityResponse availability, DashboardPhysicalWorkloadResponse workloads, CancellationToken ct)
    {
        var r = new List<DashboardQualityAlertResponse>();
        if (full)
        {
            foreach (var x in workflow.StatusCounts)
            {
                var item = x.Status switch { MatchReportStatus.DRAFT => ("REPORT_DRAFT", "INFO"), MatchReportStatus.READY_FOR_REVIEW => ("REPORT_READY_FOR_REVIEW", "WARNING"), MatchReportStatus.NEEDS_CORRECTION => ("REPORT_NEEDS_CORRECTION", "WARNING"), _ => (null, null) };
                if (item.Item1 is not null)
                    r.Add(new(item.Item1, item.Item2!, x.Count, "MATCH_REPORTS", true));
            }
            if (workflow.MissingReportCount > 0)
                r.Add(new("PLAYED_MATCH_WITHOUT_REPORT", "ERROR", workflow.MissingReportCount.Value, "MATCH_REPORTS", true));
        }
        if (imports)
        {
            var from = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var until = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var q = db.ImportJobs.AsNoTracking().Where(i => i.TeamId == team && (i.MatchId == null && i.TrainingSessionId == null && i.CreatedAtUtc >= from && i.CreatedAtUtc < until || i.MatchId != null && db.Matches.Any(m => m.Id == i.MatchId && m.SeasonId == season) || i.TrainingSessionId != null && db.TrainingSessions.Any(t => t.Id == i.TrainingSessionId && t.SessionDate >= start && t.SessionDate <= end)));
            var failed = await q.CountAsync(i => i.Status == ImportJobStatus.FAILED, ct);
            var validation = await q.CountAsync(i => i.Status == ImportJobStatus.VALIDATION_FAILED, ct);
            if (failed > 0)
                r.Add(new("IMPORT_FAILED", "ERROR", failed, "IMPORTS", true));
            if (validation > 0)
                r.Add(new("IMPORT_VALIDATION_FAILED", "WARNING", validation, "IMPORTS", true));
        }
        if (availability.UnknownCount > 0)
            r.Add(new("AVAILABILITY_UNKNOWN", "WARNING", availability.UnknownCount, "AVAILABILITY", false));
        if (workloads.SplitMetricContextCount > 0)
            r.Add(new("WORKLOAD_COMPARABILITY_SPLIT", "INFO", workloads.SplitMetricContextCount, "PHYSICAL_WORKLOADS", true));
        return r.OrderBy(x => x.Severity == "ERROR" ? 0 : x.Severity == "WARNING" ? 1 : 2).ThenBy(x => x.Code).Take(10).ToArray();
    }
    private bool IsEnabled(PlayerPerformance.Domain.Teams.TeamTrackingLevel? level, string fieldCode, bool goalkeeper) => level.HasValue && (goalkeeper ? statisticProfiles.Get(level.Value).GoalkeeperFields : statisticProfiles.Get(level.Value).PlayerFields).Contains(fieldCode);
    private sealed record LeaderMetric(string Code, string FieldCode, Func<PlayerMatchStats, int?>? PlayerValue, Func<GoalkeeperMatchStats, int?>? GoalkeeperValue);
    private static string ComparabilityKey(decimal? thresholdValue, PhysicalMetricUnit? thresholdUnitCode, ThresholdDirection? thresholdDirection, ThresholdScope? thresholdScope, string? methodKey, string? methodVersion) => $"{thresholdValue}|{thresholdUnitCode}|{thresholdDirection}|{thresholdScope}|{methodKey}|{methodVersion}";
    private static bool Readable(CurrentUserAccess a) => a.IsActive && a.HasAccessProfile;
    private static bool AllTeams(CurrentUserAccess a) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS;
    private static bool CanImport(CurrentUserAccess a) => a.IsAdmin || a.PrimaryRole == StaffRole.DATA_OPERATOR && a.EffectivePermissions.CanImportData;
    private static DashboardReadResult Fail(string code) => new(null, new(code));
    private static string Result(int a, int b) => a > b ? "WIN" : a < b ? "LOSS" : "DRAW";
}
