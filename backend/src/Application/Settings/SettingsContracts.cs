using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.Application.Settings;

public sealed record CreateSeasonRequest(string? Name, DateOnly? StartDate, DateOnly? EndDate);
public sealed record UpdateSeasonRequest(string? Name, DateOnly? StartDate, DateOnly? EndDate);
public sealed record CreateCompetitionRequest(string? Name);
public sealed record UpdateCompetitionRequest(string? Name);
public sealed record SeasonResponse(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsArchived, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
public sealed record CompetitionResponse(Guid Id, string Name, bool IsArchived, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public static class SettingsNameRules
{
    public const int NameMaxLength = 120;
    public static string Normalize(string name) => name.Trim().ToUpperInvariant();
}

public sealed class CreateSeasonRequestValidator : AbstractValidator<CreateSeasonRequest>
{
    public CreateSeasonRequestValidator()
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
        RuleFor(x => x.StartDate).NotNull().WithErrorCode("start_date_required");
        RuleFor(x => x.EndDate).NotNull().WithErrorCode("end_date_required");
        RuleFor(x => x).Must(x => x.StartDate is null || x.EndDate is null || x.StartDate <= x.EndDate).WithErrorCode("invalid_date_range");
    }
}
public sealed class UpdateSeasonRequestValidator : AbstractValidator<UpdateSeasonRequest>
{
    public UpdateSeasonRequestValidator()
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
        RuleFor(x => x.StartDate).NotNull().WithErrorCode("start_date_required");
        RuleFor(x => x.EndDate).NotNull().WithErrorCode("end_date_required");
        RuleFor(x => x).Must(x => x.StartDate is null || x.EndDate is null || x.StartDate <= x.EndDate).WithErrorCode("invalid_date_range");
    }
}
public sealed class CreateCompetitionRequestValidator : AbstractValidator<CreateCompetitionRequest>
{
    public CreateCompetitionRequestValidator() => RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
}
public sealed class UpdateCompetitionRequestValidator : AbstractValidator<UpdateCompetitionRequest>
{
    public UpdateCompetitionRequestValidator() => RuleFor(x => x.Name).Cascade(CascadeMode.Stop).Must(x => !string.IsNullOrWhiteSpace(x)).WithErrorCode("name_required").Must(x => x is null || x.Trim().Length <= SettingsNameRules.NameMaxLength).WithErrorCode("name_too_long");
}

public interface ISettingsService
{
    Task<IReadOnlyList<SeasonResponse>> ListSeasonsAsync(bool includeArchived, CancellationToken ct); Task<SeasonResponse?> GetSeasonAsync(Guid id, CancellationToken ct); Task<Result<SeasonResponse>> CreateSeasonAsync(CreateSeasonRequest request, CancellationToken ct); Task<Result<SeasonResponse>> UpdateSeasonAsync(Guid id, UpdateSeasonRequest request, CancellationToken ct); Task<Result<SeasonResponse>> ArchiveSeasonAsync(Guid id, CancellationToken ct); Task<Result<SeasonResponse>> RestoreSeasonAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CompetitionResponse>> ListCompetitionsAsync(bool includeArchived, CancellationToken ct); Task<CompetitionResponse?> GetCompetitionAsync(Guid id, CancellationToken ct); Task<Result<CompetitionResponse>> CreateCompetitionAsync(CreateCompetitionRequest request, CancellationToken ct); Task<Result<CompetitionResponse>> UpdateCompetitionAsync(Guid id, UpdateCompetitionRequest request, CancellationToken ct); Task<Result<CompetitionResponse>> ArchiveCompetitionAsync(Guid id, CancellationToken ct); Task<Result<CompetitionResponse>> RestoreCompetitionAsync(Guid id, CancellationToken ct);
}
public interface ISettingsRepository
{
    Task<IReadOnlyList<Season>> ListSeasonsAsync(bool includeArchived, CancellationToken ct); Task<Season?> GetSeasonAsync(Guid id, CancellationToken ct); Task<bool> SeasonNameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct); void Add(Season season);
    Task<IReadOnlyList<Competition>> ListCompetitionsAsync(bool includeArchived, CancellationToken ct); Task<Competition?> GetCompetitionAsync(Guid id, CancellationToken ct); Task<bool> CompetitionNameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct); void Add(Competition competition); Task SaveChangesAsync(CancellationToken ct);
}
public sealed class SettingsDuplicateNameException : Exception
{
    public SettingsDuplicateNameException() : base("A settings record with that name already exists.") { }
}
internal static class SettingsErrors
{
    public static readonly Error NotFound = new("not_found", "The requested settings record was not found."); public static readonly Error DuplicateName = new("duplicate_name", "A settings record with that name already exists."); public static readonly Error Archived = new("archived_record", "Archived records must be restored before they can be updated."); public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
}
