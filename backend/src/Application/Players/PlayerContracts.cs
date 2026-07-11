using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Application.Players;

public static class PlayerNameRules
{
    public const int NameMaxLength = 100;
}

/// <summary>Payload for creating a club player record.</summary>
public sealed record CreatePlayerRequest(string? FirstName, string? LastName, string? PreferredName, DateOnly? DateOfBirth) : IPlayerProfileRequest;

/// <summary>Payload for updating a non-archived club player profile.</summary>
public sealed record UpdatePlayerRequest(string? FirstName, string? LastName, string? PreferredName, DateOnly? DateOfBirth) : IPlayerProfileRequest;

/// <summary>Filtering and paging options for club player records.</summary>
public sealed record PlayerListQuery
{
    public string? Search { get; init; }
    public PlayerRecordStatus? Status { get; init; }
    public bool IncludeArchived { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public Guid? TeamId { get; init; }
}

/// <summary>Represents a club player returned by the API.</summary>
public sealed record PlayerSummaryResponse(Guid Id, string FirstName, string LastName, string? PreferredName, string DisplayName, DateOnly? DateOfBirth, PlayerRecordStatus Status, IReadOnlyList<CurrentPlayerTeamAssignmentResponse> CurrentAssignments, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

/// <summary>Compact current team assignment included with player read models.</summary>
public sealed record CurrentPlayerTeamAssignmentResponse(Guid TeamId, string TeamName, DateOnly StartDate, DateOnly? EndDate);

/// <summary>Payload for assigning a player to a team selection.</summary>
public sealed record CreatePlayerTeamAssignmentRequest(Guid TeamId, DateOnly StartDate, DateOnly? EndDate);

/// <summary>Payload for ending an open player team assignment.</summary>
public sealed record EndPlayerTeamAssignmentRequest(DateOnly EndDate);

/// <summary>Represents a time-bound player team assignment returned by the API.</summary>
public sealed record PlayerTeamAssignmentResponse(Guid Id, Guid PlayerId, Guid TeamId, string TeamName, DateOnly StartDate, DateOnly? EndDate, PlayerAssignmentTimingState TimingState, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

/// <summary>Represents a paged result of club player records.</summary>
public sealed record PagedPlayerListResponse(IReadOnlyList<PlayerSummaryResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed class CreatePlayerRequestValidator : AbstractValidator<CreatePlayerRequest>
{
    public CreatePlayerRequestValidator() => Configure(this);

    internal static void Configure<T>(AbstractValidator<T> validator) where T : IPlayerProfileRequest
    {
        validator.RuleFor(x => x.FirstName).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("first_name_required").Must(x => x is null || x.Trim().Length <= PlayerNameRules.NameMaxLength).WithErrorCode("first_name_too_long");
        validator.RuleFor(x => x.LastName).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("last_name_required").Must(x => x is null || x.Trim().Length <= PlayerNameRules.NameMaxLength).WithErrorCode("last_name_too_long");
        validator.RuleFor(x => x.PreferredName).Must(x => x is null || x.Trim().Length <= PlayerNameRules.NameMaxLength).WithErrorCode("preferred_name_too_long");
        validator.RuleFor(x => x.DateOfBirth).Must(x => !x.HasValue || x.Value <= DateOnly.FromDateTime(DateTime.UtcNow)).WithErrorCode("date_of_birth_in_future");
    }
}

public sealed class UpdatePlayerRequestValidator : AbstractValidator<UpdatePlayerRequest>
{
    public UpdatePlayerRequestValidator() => CreatePlayerRequestValidator.Configure(this);
}

public sealed class PlayerListQueryValidator : AbstractValidator<PlayerListQuery>
{
    public PlayerListQueryValidator()
    {
        RuleFor(x => x.Search).Must(x => x is null || x.Trim().Length <= PlayerNameRules.NameMaxLength).WithErrorCode("search_too_long");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("page_out_of_range");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("page_size_out_of_range");
        RuleFor(x => x.TeamId).Must(x => !x.HasValue || x.Value != Guid.Empty).WithErrorCode("invalid_team_id");
    }
}

public interface IPlayerProfileRequest
{
    string? FirstName { get; }
    string? LastName { get; }
    string? PreferredName { get; }
    DateOnly? DateOfBirth { get; }
}

public interface IPlayersService
{
    Task<Result<PagedPlayerListResponse>> ListAsync(PlayerListQuery query, CancellationToken ct);
    Task<PlayerSummaryResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> CreateAsync(CreatePlayerRequest request, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> UpdateAsync(Guid id, UpdatePlayerRequest request, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> ActivateAsync(Guid id, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> DeactivateAsync(Guid id, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> ArchiveAsync(Guid id, CancellationToken ct);
    Task<Result<PlayerSummaryResponse>> RestoreAsync(Guid id, CancellationToken ct);
}

public interface IPlayersRepository
{
    Task<PagedPlayers> ListAsync(PlayerListQuery query, DateOnly today, IReadOnlyList<Guid>? selectedTeamIds, CancellationToken ct);
    Task<Player?> GetAsync(Guid id, CancellationToken ct);
    Task<PlayerReadModel?> GetReadAsync(Guid id, DateOnly today, IReadOnlyList<Guid>? selectedTeamIds, CancellationToken ct);
    Task<IReadOnlyList<CurrentPlayerTeamAssignment>> GetCurrentAssignmentsAsync(Guid playerId, DateOnly today, CancellationToken ct);
    void Add(Player player);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record PagedPlayers(IReadOnlyList<PlayerReadModel> Items, int TotalCount);
public sealed record PlayerReadModel(Player Player, IReadOnlyList<CurrentPlayerTeamAssignment> CurrentAssignments);
public sealed record CurrentPlayerTeamAssignment(Guid TeamId, string TeamName, DateOnly StartDate, DateOnly? EndDate);

public interface IPlayerTeamAssignmentsService
{
    Task<IReadOnlyList<PlayerTeamAssignmentResponse>?> ListAsync(Guid playerId, CancellationToken ct);
    Task<Result<PlayerTeamAssignmentResponse>> CreateAsync(Guid playerId, CreatePlayerTeamAssignmentRequest request, CancellationToken ct);
    Task<Result<PlayerTeamAssignmentResponse>> EndAsync(Guid playerId, Guid assignmentId, EndPlayerTeamAssignmentRequest request, CancellationToken ct);
}

public interface IPlayerTeamAssignmentsRepository
{
    Task<Player?> GetPlayerAsync(Guid playerId, CancellationToken ct);
    Task<Team?> GetTeamAsync(Guid teamId, CancellationToken ct);
    Task<PlayerTeamAssignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken ct);
    Task<bool> HasOverlapAsync(Guid playerId, Guid teamId, DateOnly startDate, DateOnly? endDate, Guid? excludingAssignmentId, CancellationToken ct);
    Task<bool> HasCurrentAssignmentsAsync(Guid playerId, DateOnly today, CancellationToken ct);
    Task<IReadOnlyList<PlayerTeamAssignmentReadModel>> ListAsync(Guid playerId, CancellationToken ct);
    void Add(PlayerTeamAssignment assignment);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record PlayerTeamAssignmentReadModel(PlayerTeamAssignment Assignment, string TeamName);

internal static class PlayerErrors
{
    public static readonly Error NotFound = new("not_found", "The requested player was not found.");
    public static readonly Error Archived = new("archived_record", "Archived players must be restored before they can be changed.");
    public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
    public static readonly Error Forbidden = new("forbidden", "You do not have access to this player.");
    public static readonly Error AssignmentOverlap = new("player_assignment_overlap", "The assignment overlaps an existing assignment for this team.");
    public static readonly Error AssignmentAlreadyEnded = new("player_assignment_already_ended", "The player assignment has already ended.");
    public static readonly Error PlayerHasCurrentAssignments = new("player_has_current_assignments", "Current player assignments must be ended first.");
    public static readonly Error PlayerNotActive = new("player_not_active", "Only active players can receive a team assignment.");
    public static readonly Error TeamNotActive = new("team_not_active", "Only active teams can receive player assignments.");
}
