using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Physical;
using PlayerPerformance.Domain.Teams;
using System.Text.Json.Serialization;

namespace PlayerPerformance.Application.Dashboard;

public sealed record DashboardTeamOption(Guid Id, string Name, TeamStatus Status, int DisplayOrder);
public sealed record DashboardSeasonOption(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsArchived);
public sealed record DashboardContextOptionsResponse(IReadOnlyList<DashboardTeamOption> Teams, IReadOnlyList<DashboardSeasonOption> Seasons);
public sealed record DashboardContextResponse(DashboardTeamOption Team, DashboardSeasonOption Season, DateTime GeneratedAtUtc);
public sealed record DashboardRecentMatchResponse(Guid Id, DateTime KickoffAtUtc, string CompetitionName, string OpponentName, MatchLocationType LocationType, int TeamScore, int OpponentScore, string Result, MatchReportStatus? ReportStatus);
public sealed record DashboardFormMatchResponse(Guid MatchId, string Result);
public sealed record DashboardTeamFormResponse(int ConsideredMatchCount, int Wins, int Draws, int Losses, int GoalsFor, int GoalsAgainst, IReadOnlyList<DashboardFormMatchResponse> Form);
public sealed record DashboardReportStatusCount(MatchReportStatus Status, int Count);
public sealed record DashboardReportWorkflowResponse(
    string VisibilityMode,
    IReadOnlyList<DashboardReportStatusCount> StatusCounts,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? PlayedMatchCount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MissingReportCount);
public sealed record DashboardAvailabilityResponse(string Scope, DateOnly AsOfDate, int TotalPlayers, int AvailableCount, int LimitedCount, int UnavailableCount, int RehabCount, int UnknownCount);
public sealed record DashboardPlayerResponse(Guid Id, string DisplayName);
public sealed record DashboardLeaderResponse(int Rank, DashboardPlayerResponse Player, int Value, int AppearanceCount, int MatchCount);
public sealed record DashboardLeaderGroupResponse(string MetricCode, string ValueKind, int EligibleReportCount, IReadOnlyList<DashboardLeaderResponse> Leaders, bool HasAdditionalTies);
public sealed record DashboardStatisticsLeadersResponse(IReadOnlyList<DashboardLeaderGroupResponse> Groups, string? EmptyReason);
public sealed record DashboardThresholdContext(decimal? Value, PhysicalMetricUnit? UnitCode, ThresholdDirection? Direction, ThresholdScope? Scope);
public sealed record DashboardMethodContext(string? Key, string? Version);
public sealed record DashboardWorkloadGroupResponse(string ContextType, string MetricCode, string ComparabilityKey, PhysicalMetricUnit UnitCode, DashboardThresholdContext ThresholdContext, DashboardMethodContext MethodContext, PhysicalMetricAggregationKind AggregationKind, decimal AggregateValue, int WorkloadCount, int PlayerCount, int ContextCount, DateOnly FirstOccurredOn, DateOnly LastOccurredOn);
public sealed record DashboardPhysicalWorkloadResponse(IReadOnlyList<DashboardWorkloadGroupResponse> Groups, bool Truncated, int AvailableGroupCount, int ReturnedGroupCount, bool HasMultipleComparabilityContexts, int SplitMetricContextCount, string? EmptyReason);
public sealed record DashboardQualityAlertResponse(string Code, string Severity, int Count, string Destination, bool IsSeasonScoped);
public sealed record DashboardOverviewResponse(DashboardContextResponse Context, IReadOnlyList<DashboardRecentMatchResponse> RecentMatches, DashboardTeamFormResponse TeamForm, DashboardReportWorkflowResponse ReportWorkflow, DashboardAvailabilityResponse Availability, DashboardStatisticsLeadersResponse StatisticsLeaders, DashboardPhysicalWorkloadResponse PhysicalWorkload, IReadOnlyList<DashboardQualityAlertResponse> QualityAlerts, DateTime GeneratedAtUtc);
public interface IDashboardService
{
    Task<DashboardContextOptionsResponse?> GetContextOptionsAsync(CancellationToken ct);
    Task<DashboardReadResult> GetOverviewAsync(Guid teamId, Guid seasonId, CancellationToken ct);
}
public sealed record DashboardReadResult(DashboardOverviewResponse? Response, DashboardReadFailure? Failure);
public sealed record DashboardReadFailure(string Code);
