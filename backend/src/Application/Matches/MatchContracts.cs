using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.Application.Matches;

public sealed record CreateMatchRequest(Guid SeasonId, Guid CompetitionId, Guid TeamId, Guid OpponentId, Guid? VenueId, DateTime KickoffAtUtc, string? Round, MatchLocationType LocationType);
public sealed record UpdateMatchRequest(Guid SeasonId, Guid CompetitionId, Guid OpponentId, Guid? VenueId, DateTime KickoffAtUtc, string? Round, MatchLocationType LocationType, MatchStatus Status, int? TeamScore, int? OpponentScore);
public sealed record MatchListQuery(Guid? SeasonId, Guid? TeamId, Guid? CompetitionId, Guid? OpponentId, MatchStatus? Status, DateTime? DateFrom, DateTime? DateTo, bool IncludeArchived = false, int Page = 1, int PageSize = 25);
public sealed record MatchReferenceResponse(Guid Id, string Name);
public sealed record MatchResponse(Guid Id, MatchReferenceResponse Season, MatchReferenceResponse Competition, MatchReferenceResponse Team, MatchReferenceResponse Opponent, MatchReferenceResponse? Venue, DateTime KickoffAtUtc, string? Round, MatchLocationType LocationType, MatchStatus Status, int? TeamScore, int? OpponentScore, bool IsArchived, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
public sealed record PagedMatchListResponse(IReadOnlyList<MatchResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed record SaveMatchLineupRequest(string? Formation, Guid? CaptainPlayerId, IReadOnlyList<SaveMatchLineupEntryRequest> Entries, IReadOnlyList<SaveMatchAppearanceRequest> Appearances, IReadOnlyList<SaveMatchSubstitutionRequest> Substitutions);
public sealed record SaveMatchLineupEntryRequest(Guid PlayerId, MatchLineupRole Role);
public sealed record SaveMatchAppearanceRequest(Guid PlayerId, int MinutesPlayed);
public sealed record SaveMatchSubstitutionRequest(Guid PlayerOutId, Guid PlayerInId, int Minute, int? StoppageTimeMinute, int Sequence);
public sealed record MatchLineupPlayerResponse(Guid Id, string FirstName, string LastName, string? PreferredName);
public sealed record EligibleLineupPlayerResponse(Guid Id, string FirstName, string LastName, string? PreferredName, PlayerRecordStatus Status, DateOnly AssignmentStartDate, DateOnly? AssignmentEndDate);
public sealed record MatchLineupEntryResponse(Guid Id, MatchLineupPlayerResponse Player, MatchLineupRole Role);
public sealed record MatchAppearanceResponse(Guid Id, Guid PlayerId, int MinutesPlayed);
public sealed record MatchSubstitutionResponse(Guid Id, Guid PlayerOutId, Guid PlayerInId, int Minute, int? StoppageTimeMinute, int Sequence);
public sealed record MatchLineupResponse(Guid MatchId, Guid TeamId, MatchStatus MatchStatus, bool IsArchived, string? Formation, MatchLineupPlayerResponse? Captain, IReadOnlyList<MatchLineupEntryResponse> Entries, IReadOnlyList<MatchAppearanceResponse> Appearances, IReadOnlyList<MatchSubstitutionResponse> Substitutions);
public sealed class CreateMatchRequestValidator : AbstractValidator<CreateMatchRequest>
{
    public CreateMatchRequestValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty();
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.OpponentId).NotEmpty();
        RuleFor(x => x.KickoffAtUtc).Must(x => x != default && x.Kind == DateTimeKind.Utc);
        RuleFor(x => x.Round).MaximumLength(80);
        RuleFor(x => x.LocationType).IsInEnum();
    }
}
public sealed class UpdateMatchRequestValidator : AbstractValidator<UpdateMatchRequest>
{
    public UpdateMatchRequestValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty();
        RuleFor(x => x.CompetitionId).NotEmpty();
        RuleFor(x => x.OpponentId).NotEmpty();
        RuleFor(x => x.KickoffAtUtc).Must(x => x != default && x.Kind == DateTimeKind.Utc);
        RuleFor(x => x.Round).MaximumLength(80);
        RuleFor(x => x.LocationType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x).Must(x => x.TeamScore.HasValue == x.OpponentScore.HasValue && x.TeamScore is not < 0 && x.OpponentScore is not < 0 && (x.Status == MatchStatus.PLAYED) == x.TeamScore.HasValue);
    }
}
public sealed class MatchListQueryValidator : AbstractValidator<MatchListQuery>
{
    public MatchListQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x).Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo);
    }
}
public sealed class SaveMatchLineupRequestValidator : AbstractValidator<SaveMatchLineupRequest>
{
    public SaveMatchLineupRequestValidator()
    {
        RuleFor(x => x.Formation).MaximumLength(32);
        RuleFor(x => x.Entries).NotNull();
        RuleFor(x => x.Appearances).NotNull();
        RuleFor(x => x.Substitutions).NotNull();
        RuleForEach(x => x.Entries).ChildRules(x =>
        {
            x.RuleFor(y => y.PlayerId).NotEmpty();
            x.RuleFor(y => y.Role).IsInEnum();
        });
        RuleForEach(x => x.Appearances).ChildRules(x =>
        {
            x.RuleFor(y => y.PlayerId).NotEmpty();
            x.RuleFor(y => y.MinutesPlayed).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.Substitutions).ChildRules(x =>
        {
            x.RuleFor(y => y.PlayerOutId).NotEmpty();
            x.RuleFor(y => y.PlayerInId).NotEmpty();
            x.RuleFor(y => y.Minute).GreaterThanOrEqualTo(0);
            x.RuleFor(y => y.StoppageTimeMinute).GreaterThanOrEqualTo(0).When(y => y.StoppageTimeMinute.HasValue);
            x.RuleFor(y => y.Sequence).GreaterThan(0);
        });
    }
}
public interface IMatchesService
{
    Task<Result<PagedMatchListResponse>> ListAsync(MatchListQuery query, CancellationToken ct);
    Task<MatchResponse?> GetAsync(Guid id, CancellationToken ct); Task<Result<IReadOnlyList<EligibleLineupPlayerResponse>>> GetEligibleLineupPlayersAsync(Guid id, CancellationToken ct); Task<Result<MatchResponse>> CreateAsync(CreateMatchRequest request, CancellationToken ct);
    Task<Result<MatchResponse>> UpdateAsync(Guid id, UpdateMatchRequest request, CancellationToken ct);
    Task<Result<MatchResponse>> ArchiveAsync(Guid id, CancellationToken ct); Task<Result<MatchResponse>> RestoreAsync(Guid id, CancellationToken ct); Task<MatchLineupResponse?> GetLineupAsync(Guid id, CancellationToken ct); Task<Result<MatchLineupResponse>> SaveLineupAsync(Guid id, SaveMatchLineupRequest request, CancellationToken ct);
}
public interface IMatchesRepository
{
    Task<Match?> GetAsync(Guid id, CancellationToken ct);
    Task<MatchReadModel?> GetReadAsync(Guid id, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct);
    Task<PagedMatchReadModel> ListAsync(MatchListQuery query, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct);
    Task<bool> ExactDuplicateExistsAsync(Guid teamId, Guid opponentId, DateTime kickoffAtUtc, CancellationToken ct);
    Task<MatchReferences?> GetActiveReferencesAsync(Guid seasonId, Guid competitionId, Guid teamId,
    Guid opponentId, Guid? venueId, CancellationToken ct);
    void Add(Match match);
    Task SaveChangesAsync(CancellationToken ct);
}
public sealed record MatchReferences(DateOnly SeasonStartDate, DateOnly SeasonEndDate);
public sealed record MatchReadModel(Match Match, string SeasonName, string CompetitionName, string TeamName, string OpponentName, string? VenueName);
public sealed record PagedMatchReadModel(IReadOnlyList<MatchReadModel> Items, int TotalCount);
public interface IMatchLineupRepository
{
    Task<MatchLineupAggregate?> GetAsync(Guid matchId, CancellationToken ct); Task<MatchLineupReadModel?> GetReadAsync(Guid matchId, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, bool>> GetNewPlayerEligibilityAsync(IReadOnlyCollection<Guid> playerIds, Guid teamId, DateOnly matchDate, CancellationToken ct);
    Task<IReadOnlyList<EligibleLineupPlayerResponse>> GetEligiblePlayersAsync(Guid teamId, DateOnly matchDate, CancellationToken ct);
    void Add(MatchLineup lineup);
    void AddEntry(MatchLineupEntry entry);
    void AddAppearance(PlayerMatchAppearance appearance);
    void AddSubstitution(MatchSubstitution substitution);
    void RemoveEntry(MatchLineupEntry entry);
    void RemoveAppearance(PlayerMatchAppearance appearance);
    void RemoveSubstitution(MatchSubstitution substitution);
    Task SaveChangesAsync(CancellationToken ct);
}
public sealed record MatchLineupAggregate(Match Match, MatchLineup? Lineup, IReadOnlyList<MatchLineupEntry> Entries, IReadOnlyList<PlayerMatchAppearance> Appearances, IReadOnlyList<MatchSubstitution> Substitutions);
public sealed record MatchLineupReadModel(Match Match, MatchLineup? Lineup, IReadOnlyList<MatchLineupEntryReadModel> Entries, IReadOnlyList<MatchAppearanceReadModel> Appearances, IReadOnlyList<MatchSubstitution> Substitutions);
public sealed record MatchLineupEntryReadModel(MatchLineupEntry Entry, string FirstName, string LastName, string? PreferredName);
public sealed record MatchAppearanceReadModel(PlayerMatchAppearance Appearance);
internal static class MatchErrors
{
    public static readonly Error NotFound = new("not_found", "The requested match was not found.");
    public static readonly Error Forbidden = new("forbidden", "You do not have access to this match.");
    public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
    public static readonly Error References = new("invalid_references", "One or more selected match references are invalid or archived.");
    public static readonly Error Duplicate = new("duplicate_match", "An identical active match already exists.");
    public static readonly Error Conflict = new("match_conflict", "The requested match change is not allowed.");
    public static readonly Error WorkflowLocked = new("report_workflow_locked", "Match data is locked while its report is under review, verified, or archived.");
}
