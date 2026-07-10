using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Application.Teams;

/// <summary>Payload for creating a club team selection.</summary>
public sealed record CreateTeamRequest(string? Name, TeamTrackingLevel TrackingLevel);
/// <summary>Payload for updating a non-archived club team selection.</summary>
public sealed record UpdateTeamRequest(string? Name, TeamTrackingLevel TrackingLevel);
/// <summary>Payload containing the complete non-archived team order.</summary>
public sealed record ReorderTeamsRequest(IReadOnlyList<Guid>? OrderedTeamIds);
/// <summary>Represents a club team selection returned by the API.</summary>
public sealed record TeamResponse(Guid Id, string Name, TeamTrackingLevel TrackingLevel, TeamStatus Status, int DisplayOrder, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= Settings.SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
        RuleFor(x => x.TrackingLevel).IsInEnum().WithErrorCode("invalid_tracking_level");
    }
}
public sealed class UpdateTeamRequestValidator : AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamRequestValidator()
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= Settings.SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
        RuleFor(x => x.TrackingLevel).IsInEnum().WithErrorCode("invalid_tracking_level");
    }
}
public sealed class ReorderTeamsRequestValidator : AbstractValidator<ReorderTeamsRequest>
{
    public ReorderTeamsRequestValidator()
    {
        RuleFor(x => x.OrderedTeamIds).NotNull().WithErrorCode("ordered_team_ids_required");
        RuleForEach(x => x.OrderedTeamIds!).NotEqual(Guid.Empty).WithErrorCode("invalid_team_id");
        RuleFor(x => x.OrderedTeamIds!).Must(ids => ids.Distinct().Count() == ids.Count).When(x => x.OrderedTeamIds is not null).WithErrorCode("duplicate_team_id");
    }
}

public interface ITeamsService
{
    Task<IReadOnlyList<TeamResponse>> ListAsync(bool includeArchived, CancellationToken ct); Task<TeamResponse?> GetAsync(Guid id, CancellationToken ct); Task<Result<TeamResponse>> CreateAsync(CreateTeamRequest request, CancellationToken ct); Task<Result<TeamResponse>> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken ct); Task<Result<TeamResponse>> ActivateAsync(Guid id, CancellationToken ct); Task<Result<TeamResponse>> DeactivateAsync(Guid id, CancellationToken ct); Task<Result<TeamResponse>> ArchiveAsync(Guid id, CancellationToken ct); Task<Result<TeamResponse>> RestoreAsync(Guid id, CancellationToken ct); Task<Result<IReadOnlyList<TeamResponse>>> ReorderAsync(ReorderTeamsRequest request, CancellationToken ct);
}
public interface ITeamsRepository
{
    Task<IReadOnlyList<Team>> ListAsync(bool includeArchived, CancellationToken ct); Task<IReadOnlyList<Team>> ListNonArchivedTrackedAsync(CancellationToken ct); Task<Team?> GetAsync(Guid id, CancellationToken ct); Task<bool> NameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct); void Add(Team team); Task SaveChangesAsync(CancellationToken ct);
}
public sealed class TeamDuplicateNameException : Exception
{
    public TeamDuplicateNameException() : base("A team with that name already exists.") { }
}
internal static class TeamErrors
{
    public static readonly Error NotFound = new("not_found", "The requested team was not found."); public static readonly Error DuplicateName = new("duplicate_name", "A team with that name already exists."); public static readonly Error Archived = new("archived_record", "Archived teams must be restored before they can be updated."); public static readonly Error Validation = new("validation_failed", "One or more fields are invalid."); public static readonly Error InvalidOrder = new("invalid_team_order", "The supplied team order must exactly match all current non-archived teams.");
}
