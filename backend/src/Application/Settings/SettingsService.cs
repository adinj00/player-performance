using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.Application.Settings;

internal sealed class SettingsService(ISettingsRepository repository, ISystemClock clock, IValidator<CreateSeasonRequest> createSeasonValidator, IValidator<UpdateSeasonRequest> updateSeasonValidator, IValidator<CreateCompetitionRequest> createCompetitionValidator, IValidator<UpdateCompetitionRequest> updateCompetitionValidator) : ISettingsService
{
    public async Task<IReadOnlyList<SeasonResponse>> ListSeasonsAsync(bool includeArchived, CancellationToken ct) => (await repository.ListSeasonsAsync(includeArchived, ct)).Select(ToResponse).ToArray();
    public async Task<SeasonResponse?> GetSeasonAsync(Guid id, CancellationToken ct) => (await repository.GetSeasonAsync(id, ct)) is { } item ? ToResponse(item) : null;
    public async Task<Result<SeasonResponse>> CreateSeasonAsync(CreateSeasonRequest request, CancellationToken ct)
    {
        if (!(await createSeasonValidator.ValidateAsync(request, ct)).IsValid)
            return Result<SeasonResponse>.Failure(SettingsErrors.Validation);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.SeasonNameExistsAsync(normalized, null, ct))
            return Result<SeasonResponse>.Failure(SettingsErrors.DuplicateName);
        var entity = Season.Create(Guid.NewGuid(), name, normalized, request.StartDate!.Value, request.EndDate!.Value, clock.UtcNow);
        repository.Add(entity);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<SeasonResponse>> UpdateSeasonAsync(Guid id, UpdateSeasonRequest request, CancellationToken ct)
    {
        if (!(await updateSeasonValidator.ValidateAsync(request, ct)).IsValid)
            return Result<SeasonResponse>.Failure(SettingsErrors.Validation);
        var entity = await repository.GetSeasonAsync(id, ct);
        if (entity is null)
            return Result<SeasonResponse>.Failure(SettingsErrors.NotFound);
        if (entity.IsArchived)
            return Result<SeasonResponse>.Failure(SettingsErrors.Archived);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.SeasonNameExistsAsync(normalized, id, ct))
            return Result<SeasonResponse>.Failure(SettingsErrors.DuplicateName);
        entity.Update(name, normalized, request.StartDate!.Value, request.EndDate!.Value, clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<SeasonResponse>> ArchiveSeasonAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetSeasonAsync(id, ct);
        if (entity is null)
            return Result<SeasonResponse>.Failure(SettingsErrors.NotFound);
        entity.Archive(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<SeasonResponse>> RestoreSeasonAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetSeasonAsync(id, ct);
        if (entity is null)
            return Result<SeasonResponse>.Failure(SettingsErrors.NotFound);
        entity.Restore(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<IReadOnlyList<CompetitionResponse>> ListCompetitionsAsync(bool includeArchived, CancellationToken ct) => (await repository.ListCompetitionsAsync(includeArchived, ct)).Select(ToResponse).ToArray();
    public async Task<CompetitionResponse?> GetCompetitionAsync(Guid id, CancellationToken ct) => (await repository.GetCompetitionAsync(id, ct)) is { } item ? ToResponse(item) : null;
    public async Task<Result<CompetitionResponse>> CreateCompetitionAsync(CreateCompetitionRequest request, CancellationToken ct)
    {
        if (!(await createCompetitionValidator.ValidateAsync(request, ct)).IsValid)
            return Result<CompetitionResponse>.Failure(SettingsErrors.Validation);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.CompetitionNameExistsAsync(normalized, null, ct))
            return Result<CompetitionResponse>.Failure(SettingsErrors.DuplicateName);
        var entity = Competition.Create(Guid.NewGuid(), name, normalized, clock.UtcNow);
        repository.Add(entity);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<CompetitionResponse>> UpdateCompetitionAsync(Guid id, UpdateCompetitionRequest request, CancellationToken ct)
    {
        if (!(await updateCompetitionValidator.ValidateAsync(request, ct)).IsValid)
            return Result<CompetitionResponse>.Failure(SettingsErrors.Validation);
        var entity = await repository.GetCompetitionAsync(id, ct);
        if (entity is null)
            return Result<CompetitionResponse>.Failure(SettingsErrors.NotFound);
        if (entity.IsArchived)
            return Result<CompetitionResponse>.Failure(SettingsErrors.Archived);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.CompetitionNameExistsAsync(normalized, id, ct))
            return Result<CompetitionResponse>.Failure(SettingsErrors.DuplicateName);
        entity.Update(name, normalized, clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<CompetitionResponse>> ArchiveCompetitionAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetCompetitionAsync(id, ct);
        if (entity is null)
            return Result<CompetitionResponse>.Failure(SettingsErrors.NotFound);
        entity.Archive(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<CompetitionResponse>> RestoreCompetitionAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetCompetitionAsync(id, ct);
        if (entity is null)
            return Result<CompetitionResponse>.Failure(SettingsErrors.NotFound);
        entity.Restore(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    private async Task<Result<SeasonResponse>> SaveAsync(Season entity, CancellationToken ct)
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<SeasonResponse>.Success(ToResponse(entity));
        }
        catch (SettingsDuplicateNameException) { return Result<SeasonResponse>.Failure(SettingsErrors.DuplicateName); }
    }
    private async Task<Result<CompetitionResponse>> SaveAsync(Competition entity, CancellationToken ct)
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<CompetitionResponse>.Success(ToResponse(entity));
        }
        catch (SettingsDuplicateNameException) { return Result<CompetitionResponse>.Failure(SettingsErrors.DuplicateName); }
    }
    private static SeasonResponse ToResponse(Season x) => new(x.Id, x.Name, x.StartDate, x.EndDate, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static CompetitionResponse ToResponse(Competition x) => new(x.Id, x.Name, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
}
