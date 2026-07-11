using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.Application.Settings;

internal sealed class SettingsService(ISettingsRepository repository, ISystemClock clock, IValidator<CreateSeasonRequest> createSeasonValidator, IValidator<UpdateSeasonRequest> updateSeasonValidator, IValidator<CreateCompetitionRequest> createCompetitionValidator, IValidator<UpdateCompetitionRequest> updateCompetitionValidator, IValidator<CreateVenueRequest> createVenueValidator, IValidator<UpdateVenueRequest> updateVenueValidator, IValidator<CreateOpponentRequest> createOpponentValidator, IValidator<UpdateOpponentRequest> updateOpponentValidator) : ISettingsService
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
    public async Task<IReadOnlyList<VenueResponse>> ListVenuesAsync(bool includeArchived, CancellationToken ct) => (await repository.ListVenuesAsync(includeArchived, ct)).Select(ToResponse).ToArray();
    public async Task<VenueResponse?> GetVenueAsync(Guid id, CancellationToken ct) => (await repository.GetVenueAsync(id, ct)) is { } item ? ToResponse(item) : null;
    public async Task<Result<VenueResponse>> CreateVenueAsync(CreateVenueRequest request, CancellationToken ct)
    {
        if (!(await createVenueValidator.ValidateAsync(request, ct)).IsValid)
            return Result<VenueResponse>.Failure(SettingsErrors.Validation);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.VenueNameExistsAsync(normalized, null, ct))
            return Result<VenueResponse>.Failure(SettingsErrors.DuplicateName);
        var entity = Venue.Create(Guid.NewGuid(), name, normalized, clock.UtcNow);
        repository.Add(entity);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<VenueResponse>> UpdateVenueAsync(Guid id, UpdateVenueRequest request, CancellationToken ct)
    {
        if (!(await updateVenueValidator.ValidateAsync(request, ct)).IsValid)
            return Result<VenueResponse>.Failure(SettingsErrors.Validation);
        var entity = await repository.GetVenueAsync(id, ct);
        if (entity is null)
            return Result<VenueResponse>.Failure(SettingsErrors.NotFound);
        if (entity.IsArchived)
            return Result<VenueResponse>.Failure(SettingsErrors.Archived);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.VenueNameExistsAsync(normalized, id, ct))
            return Result<VenueResponse>.Failure(SettingsErrors.DuplicateName);
        entity.Update(name, normalized, clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<VenueResponse>> ArchiveVenueAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetVenueAsync(id, ct);
        if (entity is null)
            return Result<VenueResponse>.Failure(SettingsErrors.NotFound);
        entity.Archive(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<VenueResponse>> RestoreVenueAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetVenueAsync(id, ct);
        if (entity is null)
            return Result<VenueResponse>.Failure(SettingsErrors.NotFound);
        entity.Restore(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<IReadOnlyList<OpponentResponse>> ListOpponentsAsync(bool includeArchived, CancellationToken ct) => (await repository.ListOpponentsAsync(includeArchived, ct)).Select(ToResponse).ToArray();
    public async Task<OpponentResponse?> GetOpponentAsync(Guid id, CancellationToken ct) => (await repository.GetOpponentAsync(id, ct)) is { } item ? ToResponse(item) : null;
    public async Task<Result<OpponentResponse>> CreateOpponentAsync(CreateOpponentRequest request, CancellationToken ct)
    {
        if (!(await createOpponentValidator.ValidateAsync(request, ct)).IsValid)
            return Result<OpponentResponse>.Failure(SettingsErrors.Validation);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.OpponentNameExistsAsync(normalized, null, ct))
            return Result<OpponentResponse>.Failure(SettingsErrors.DuplicateName);
        var entity = Opponent.Create(Guid.NewGuid(), name, normalized, clock.UtcNow);
        repository.Add(entity);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<OpponentResponse>> UpdateOpponentAsync(Guid id, UpdateOpponentRequest request, CancellationToken ct)
    {
        if (!(await updateOpponentValidator.ValidateAsync(request, ct)).IsValid)
            return Result<OpponentResponse>.Failure(SettingsErrors.Validation);
        var entity = await repository.GetOpponentAsync(id, ct);
        if (entity is null)
            return Result<OpponentResponse>.Failure(SettingsErrors.NotFound);
        if (entity.IsArchived)
            return Result<OpponentResponse>.Failure(SettingsErrors.Archived);
        var name = request.Name!.Trim();
        var normalized = SettingsNameRules.Normalize(name);
        if (await repository.OpponentNameExistsAsync(normalized, id, ct))
            return Result<OpponentResponse>.Failure(SettingsErrors.DuplicateName);
        entity.Update(name, normalized, clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<OpponentResponse>> ArchiveOpponentAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetOpponentAsync(id, ct);
        if (entity is null)
            return Result<OpponentResponse>.Failure(SettingsErrors.NotFound);
        entity.Archive(clock.UtcNow);
        return await SaveAsync(entity, ct);
    }
    public async Task<Result<OpponentResponse>> RestoreOpponentAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetOpponentAsync(id, ct);
        if (entity is null)
            return Result<OpponentResponse>.Failure(SettingsErrors.NotFound);
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
    private async Task<Result<VenueResponse>> SaveAsync(Venue entity, CancellationToken ct)
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<VenueResponse>.Success(ToResponse(entity));
        }
        catch (SettingsDuplicateNameException) { return Result<VenueResponse>.Failure(SettingsErrors.DuplicateName); }
    }
    private async Task<Result<OpponentResponse>> SaveAsync(Opponent entity, CancellationToken ct)
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result<OpponentResponse>.Success(ToResponse(entity));
        }
        catch (SettingsDuplicateNameException) { return Result<OpponentResponse>.Failure(SettingsErrors.DuplicateName); }
    }
    private static SeasonResponse ToResponse(Season x) => new(x.Id, x.Name, x.StartDate, x.EndDate, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static CompetitionResponse ToResponse(Competition x) => new(x.Id, x.Name, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static VenueResponse ToResponse(Venue x) => new(x.Id, x.Name, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static OpponentResponse ToResponse(Opponent x) => new(x.Id, x.Name, x.IsArchived, x.CreatedAtUtc, x.UpdatedAtUtc);
}
