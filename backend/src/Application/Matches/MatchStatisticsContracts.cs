using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Application.Matches;

public static class MatchStatisticFieldCodes
{
    public const string Goals = "goals";
    public const string Assists = "assists";
    public const string YellowCards = "yellowCards";
    public const string RedCards = "redCards";
    public const string Shots = "shots";
    public const string ShotsOnTarget = "shotsOnTarget";
    public const string PassesAttempted = "passesAttempted";
    public const string PassesCompleted = "passesCompleted";
    public const string KeyPasses = "keyPasses";
    public const string DuelsAttempted = "duelsAttempted";
    public const string DuelsWon = "duelsWon";
    public const string FoulsCommitted = "foulsCommitted";
    public const string FoulsWon = "foulsWon";
    public const string Offsides = "offsides";
    public const string BallRecoveries = "ballRecoveries";
    public const string PossessionLosses = "possessionLosses";
    public const string Saves = "saves";
    public const string GoalsConceded = "goalsConceded";
    public const string CleanSheet = "cleanSheet";
    public const string PenaltySaves = "penaltySaves";
}
public sealed record MatchStatisticsProfile(IReadOnlySet<string> PlayerFields, IReadOnlySet<string> GoalkeeperFields);
public interface IMatchStatisticsProfileService { MatchStatisticsProfile Get(TeamTrackingLevel level); }
public sealed class MatchStatisticsProfileService : IMatchStatisticsProfileService
{
    private static readonly string[] BasicPlayer = [MatchStatisticFieldCodes.Goals, MatchStatisticFieldCodes.Assists, MatchStatisticFieldCodes.YellowCards, MatchStatisticFieldCodes.RedCards];
    private static readonly string[] StandardPlayer = [.. BasicPlayer, MatchStatisticFieldCodes.Shots, MatchStatisticFieldCodes.ShotsOnTarget, MatchStatisticFieldCodes.FoulsCommitted, MatchStatisticFieldCodes.FoulsWon, MatchStatisticFieldCodes.Offsides];
    private static readonly string[] FullPlayer = [.. StandardPlayer, MatchStatisticFieldCodes.PassesAttempted, MatchStatisticFieldCodes.PassesCompleted, MatchStatisticFieldCodes.KeyPasses, MatchStatisticFieldCodes.DuelsAttempted, MatchStatisticFieldCodes.DuelsWon, MatchStatisticFieldCodes.BallRecoveries, MatchStatisticFieldCodes.PossessionLosses];
    private static readonly string[] BasicGoalkeeper = [MatchStatisticFieldCodes.Saves, MatchStatisticFieldCodes.GoalsConceded, MatchStatisticFieldCodes.CleanSheet];
    private static readonly string[] FullGoalkeeper = [.. BasicGoalkeeper, MatchStatisticFieldCodes.PenaltySaves];
    public MatchStatisticsProfile Get(TeamTrackingLevel level) => level switch
    {
        TeamTrackingLevel.BASIC => new(BasicPlayer.ToHashSet(), BasicGoalkeeper.ToHashSet()),
        TeamTrackingLevel.STANDARD => new(StandardPlayer.ToHashSet(), FullGoalkeeper.ToHashSet()),
        TeamTrackingLevel.FULL => new(FullPlayer.ToHashSet(), FullGoalkeeper.ToHashSet()),
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}

public sealed record SaveMatchReportStatisticsRequest(IReadOnlyList<SavePlayerMatchStatisticsRequest> PlayerStatistics, IReadOnlyList<SaveGoalkeeperMatchStatisticsRequest> GoalkeeperStatistics);
public sealed record SavePlayerMatchStatisticsRequest(Guid PlayerMatchAppearanceId, int? Goals, int? Assists, int? YellowCards, int? RedCards, int? Shots, int? ShotsOnTarget, int? PassesAttempted, int? PassesCompleted, int? KeyPasses, int? DuelsAttempted, int? DuelsWon, int? FoulsCommitted, int? FoulsWon, int? Offsides, int? BallRecoveries, int? PossessionLosses);
public sealed record SaveGoalkeeperMatchStatisticsRequest(Guid PlayerMatchAppearanceId, int? Saves, int? GoalsConceded, bool? CleanSheet, int? PenaltySaves);
public sealed record MatchStatisticsAppearanceResponse(Guid PlayerMatchAppearanceId, Guid PlayerId, string FirstName, string LastName, string? PreferredName, int MinutesPlayed);
public sealed record PlayerMatchStatisticsResponse(Guid PlayerMatchAppearanceId, int? Goals, int? Assists, int? YellowCards, int? RedCards, int? Shots, int? ShotsOnTarget, int? PassesAttempted, int? PassesCompleted, int? KeyPasses, int? DuelsAttempted, int? DuelsWon, int? FoulsCommitted, int? FoulsWon, int? Offsides, int? BallRecoveries, int? PossessionLosses, bool IsComplete);
public sealed record GoalkeeperMatchStatisticsResponse(Guid PlayerMatchAppearanceId, int? Saves, int? GoalsConceded, bool? CleanSheet, int? PenaltySaves, bool IsComplete);
public sealed record MatchReportStatisticsResponse(Guid ReportId, Guid MatchId, MatchReportStatus ReportStatus, TeamTrackingLevel? AppliedTrackingLevel, IReadOnlyList<string> EnabledPlayerFields, IReadOnlyList<string> EnabledGoalkeeperFields, IReadOnlyList<MatchStatisticsAppearanceResponse> Appearances, IReadOnlyList<PlayerMatchStatisticsResponse> PlayerStatistics, IReadOnlyList<GoalkeeperMatchStatisticsResponse> GoalkeeperStatistics, bool IsComplete, bool CanEditStatistics);
public interface IMatchStatisticsService
{
    Task<MatchReportStatisticsResponse?> GetAsync(Guid reportId, CancellationToken ct);
    Task<Result<MatchReportStatisticsResponse>> SaveAsync(Guid reportId, SaveMatchReportStatisticsRequest request, CancellationToken ct);
    Task<Result> EnsureSubmissionCompleteAsync(MatchReportAggregate aggregate, CancellationToken ct);
}
public interface IMatchStatisticsRepository
{
    Task<MatchStatisticsAggregate?> GetAggregateAsync(Guid reportId, CancellationToken ct);
    Task<MatchStatisticsReadModel?> GetReadAsync(Guid reportId, IReadOnlyCollection<Guid>? scope, CancellationToken ct);
    void Add(PlayerMatchStats stats);
    void Add(GoalkeeperMatchStats stats);
    void Remove(GoalkeeperMatchStats stats); Task SaveChangesAsync(CancellationToken ct);
}
public interface IMatchStatisticsCleanup
{
    Task RemoveForAppearancesAsync(IReadOnlyCollection<Guid> appearanceIds, CancellationToken ct);
}
public sealed record MatchStatisticsAggregate(MatchReport Report, Match Match, TeamTrackingLevel TeamTrackingLevel, IReadOnlyList<PlayerMatchAppearance> Appearances, IReadOnlyList<PlayerMatchStats> PlayerStatistics, IReadOnlyList<GoalkeeperMatchStats> GoalkeeperStatistics);
public sealed record MatchStatisticsAppearanceReadModel(PlayerMatchAppearance Appearance, string FirstName, string LastName, string? PreferredName);
public sealed record MatchStatisticsReadModel(MatchReport Report, Match Match, TeamTrackingLevel TeamTrackingLevel, IReadOnlyList<MatchStatisticsAppearanceReadModel> Appearances, IReadOnlyList<PlayerMatchStats> PlayerStatistics, IReadOnlyList<GoalkeeperMatchStats> GoalkeeperStatistics);
internal static class MatchStatisticsErrors
{
    public static readonly Error NotFound = new("not_found", "The requested match report was not found.");
    public static readonly Error Forbidden = new("forbidden", "You do not have access to this match report.");
    public static readonly Error Validation = new("statistics_validation_failed", "The statistics snapshot is invalid or incomplete.");
    public static readonly Error Conflict = new("report_workflow_locked", "Statistics are locked while the report is under review, verified, or archived.");
}
