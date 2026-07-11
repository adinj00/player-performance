using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Players;

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
}

/// <summary>Represents a club player returned by the API.</summary>
public sealed record PlayerSummaryResponse(Guid Id, string FirstName, string LastName, string? PreferredName, string DisplayName, DateOnly? DateOfBirth, PlayerRecordStatus Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

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
    Task<PagedPlayers> ListAsync(PlayerListQuery query, CancellationToken ct);
    Task<Player?> GetAsync(Guid id, CancellationToken ct);
    void Add(Player player);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record PagedPlayers(IReadOnlyList<Player> Items, int TotalCount);

internal static class PlayerErrors
{
    public static readonly Error NotFound = new("not_found", "The requested player was not found.");
    public static readonly Error Archived = new("archived_record", "Archived players must be restored before they can be changed.");
    public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
}
