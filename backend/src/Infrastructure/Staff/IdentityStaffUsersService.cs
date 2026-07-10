using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Users;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Users;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Staff;

internal sealed class IdentityStaffUsersService(AppDbContext db, UserManager<ApplicationUser> users, ISystemClock clock) : IStaffUsersService
{
    private const string InvitationProvider = "Default";
    private const string InvitationPurpose = "staff-invitation";

    public async Task<IReadOnlyList<StaffUserResponse>> ListAsync(string? search, StaffRole? role, string? status, TeamScopeType? scopeType, Guid? teamId, CancellationToken ct)
    {
        var query = db.StaffAccessProfiles.AsNoTracking().Join(db.Users.AsNoTracking(), p => p.UserId, u => u.Id, (p, u) => new { p, u });
        if (!string.IsNullOrWhiteSpace(search)) { var value = search.Trim().ToUpperInvariant(); query = query.Where(x => x.p.DisplayName.ToUpper().Contains(value) || (x.u.Email ?? "").ToUpper().Contains(value)); }
        if (role.HasValue) query = query.Where(x => x.p.PrimaryRole == role.Value);
        if (Enum.TryParse<UserAccountStatus>(status, true, out var parsed)) query = query.Where(x => x.u.AccountStatus == parsed);
        if (scopeType.HasValue) query = query.Where(x => x.p.TeamScopeType == scopeType.Value);
        if (teamId.HasValue) query = query.Where(x => db.StaffTeamScopes.Any(s => s.UserId == x.p.UserId && s.TeamId == teamId));
        var rows = await query.OrderBy(x => x.p.DisplayName).ThenBy(x => x.u.Email).ThenBy(x => x.u.Id).ToListAsync(ct);
        return await MapAsync(rows.Select(x => (x.p, x.u)), ct);
    }

    public async Task<StaffUserResponse?> GetAsync(Guid userId, CancellationToken ct)
    {
        var profile = await db.StaffAccessProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, ct);
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, ct);
        return profile is null || user is null ? null : (await MapAsync([(profile, user)], ct)).Single();
    }

    public async Task<Result<StaffInvitationCredentialResponse>> CreateInvitationAsync(CreateStaffInvitationRequest request, CancellationToken ct)
    {
        if (!TryAccess(request.DisplayName, request.PrimaryRole, request.CanVerifyReports, request.CanImportData, request.CanViewMedicalDetails, request.TeamScopeType, request.SelectedTeamIds, out var access)) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.Validation);
        var email = request.Email?.Trim(); if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email)) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.Validation);
        if (await users.FindByEmailAsync(email) is not null) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.DuplicateEmail);
        if (!await ValidTeamsAsync(access.Scope, access.TeamIds, ct)) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.Validation);
        var now = clock.UtcNow;
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = email, AccountStatus = UserAccountStatus.INVITED, RequiresPasswordChange = false, CreatedUtc = now, UpdatedUtc = now };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var created = await users.CreateAsync(user);
        if (!created.Succeeded) return Result<StaffInvitationCredentialResponse>.Failure(created.Errors.Any(x => x.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)) ? StaffUserErrors.DuplicateEmail : StaffUserErrors.Conflict);
        db.StaffAccessProfiles.Add(new StaffAccessProfile { UserId = user.Id, DisplayName = access.Name, PrimaryRole = access.Role, CanVerifyReports = access.Verify, CanImportData = access.Import, CanViewMedicalDetails = access.Medical, TeamScopeType = access.Scope, CreatedUtc = now, UpdatedUtc = now });
        AddScopes(user.Id, access.TeamIds, now); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        var token = EncodeToken(await users.GenerateUserTokenAsync(user, InvitationProvider, InvitationPurpose));
        return Result<StaffInvitationCredentialResponse>.Success(new((await GetAsync(user.Id, ct))!, token));
    }

    public async Task<Result<StaffInvitationCredentialResponse>> ReissueInvitationAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.NotFound);
        if (user.AccountStatus != UserAccountStatus.INVITED) return Result<StaffInvitationCredentialResponse>.Failure(StaffUserErrors.Conflict);
        await users.UpdateSecurityStampAsync(user);
        var token = EncodeToken(await users.GenerateUserTokenAsync(user, InvitationProvider, InvitationPurpose));
        return Result<StaffInvitationCredentialResponse>.Success(new((await GetAsync(userId, ct))!, token));
    }

    public async Task<Result<StaffUserResponse>> UpdateProfileAsync(Guid userId, UpdateStaffProfileRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 120) return Result<StaffUserResponse>.Failure(StaffUserErrors.Validation);
        var profile = await db.StaffAccessProfiles.SingleOrDefaultAsync(x => x.UserId == userId, ct); if (profile is null) return Result<StaffUserResponse>.Failure(StaffUserErrors.NotFound);
        profile.DisplayName = request.DisplayName.Trim(); profile.UpdatedUtc = clock.UtcNow; await db.SaveChangesAsync(ct); return Result<StaffUserResponse>.Success((await GetAsync(userId, ct))!);
    }

    public async Task<Result<StaffUserResponse>> ReplaceAccessAsync(Guid userId, ReplaceStaffAccessRequest request, CancellationToken ct)
    {
        if (!TryAccess("valid", request.PrimaryRole, request.CanVerifyReports, request.CanImportData, request.CanViewMedicalDetails, request.TeamScopeType, request.SelectedTeamIds, out var access) || !await ValidTeamsAsync(access.Scope, access.TeamIds, ct)) return Result<StaffUserResponse>.Failure(StaffUserErrors.Validation);
        var profile = await db.StaffAccessProfiles.SingleOrDefaultAsync(x => x.UserId == userId, ct); var user = await db.Users.SingleOrDefaultAsync(x => x.Id == userId, ct); if (profile is null || user is null) return Result<StaffUserResponse>.Failure(StaffUserErrors.NotFound);
        if (profile.PrimaryRole == StaffRole.ADMIN && access.Role != StaffRole.ADMIN && await IsFinalActiveAdminAsync(userId, ct)) return Result<StaffUserResponse>.Failure(StaffUserErrors.Conflict);
        profile.PrimaryRole = access.Role; profile.CanVerifyReports = access.Verify; profile.CanImportData = access.Import; profile.CanViewMedicalDetails = access.Medical; profile.TeamScopeType = access.Scope; profile.UpdatedUtc = clock.UtcNow;
        db.StaffTeamScopes.RemoveRange(db.StaffTeamScopes.Where(x => x.UserId == userId)); AddScopes(userId, access.TeamIds, clock.UtcNow); await db.SaveChangesAsync(ct); return Result<StaffUserResponse>.Success((await GetAsync(userId, ct))!);
    }

    public async Task<Result<StaffUserResponse>> DisableAsync(Guid userId, CancellationToken ct)
    { var user = await db.Users.SingleOrDefaultAsync(x => x.Id == userId, ct); if (user is null) return Result<StaffUserResponse>.Failure(StaffUserErrors.NotFound); if (user.AccountStatus == UserAccountStatus.DISABLED) return Result<StaffUserResponse>.Success((await GetAsync(userId, ct))!); if (await IsFinalActiveAdminAsync(userId, ct)) return Result<StaffUserResponse>.Failure(StaffUserErrors.Conflict); user.AccountStatus = UserAccountStatus.DISABLED; user.UpdatedUtc = clock.UtcNow; await users.UpdateSecurityStampAsync(user); await db.SaveChangesAsync(ct); return Result<StaffUserResponse>.Success((await GetAsync(userId, ct))!); }
    public async Task<Result<StaffUserResponse>> ReactivateAsync(Guid userId, CancellationToken ct)
    { var user = await db.Users.SingleOrDefaultAsync(x => x.Id == userId, ct); if (user is null) return Result<StaffUserResponse>.Failure(StaffUserErrors.NotFound); if (user.AccountStatus != UserAccountStatus.DISABLED) return Result<StaffUserResponse>.Failure(StaffUserErrors.Conflict); user.AccountStatus = string.IsNullOrEmpty(user.PasswordHash) ? UserAccountStatus.INVITED : UserAccountStatus.ACTIVE; user.UpdatedUtc = clock.UtcNow; await db.SaveChangesAsync(ct); return Result<StaffUserResponse>.Success((await GetAsync(userId, ct))!); }

    public async Task<Result> AcceptInvitationAsync(AcceptStaffInvitationRequest request, CancellationToken ct)
    { if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password) || request.Password != request.ConfirmPassword) return Result.Failure(StaffUserErrors.InvalidInvitation); var user = await users.FindByEmailAsync(request.Email.Trim()); string token; try { token = DecodeToken(request.Token); } catch (FormatException) { return Result.Failure(StaffUserErrors.InvalidInvitation); } if (user is null || user.AccountStatus != UserAccountStatus.INVITED || !await users.VerifyUserTokenAsync(user, InvitationProvider, InvitationPurpose, token)) return Result.Failure(StaffUserErrors.InvalidInvitation); var password = await users.AddPasswordAsync(user, request.Password); if (!password.Succeeded) return Result.Failure(StaffUserErrors.InvalidInvitation); user.AccountStatus = UserAccountStatus.ACTIVE; user.RequiresPasswordChange = false; user.UpdatedUtc = clock.UtcNow; await users.UpdateSecurityStampAsync(user); var update = await users.UpdateAsync(user); return update.Succeeded ? Result.Success() : Result.Failure(StaffUserErrors.InvalidInvitation); }

    private async Task<bool> IsFinalActiveAdminAsync(Guid userId, CancellationToken ct) => await db.StaffAccessProfiles.Join(db.Users, p => p.UserId, u => u.Id, (p, u) => new { p, u }).CountAsync(x => x.p.PrimaryRole == StaffRole.ADMIN && x.u.AccountStatus == UserAccountStatus.ACTIVE, ct) == 1 && await db.StaffAccessProfiles.AnyAsync(x => x.UserId == userId && x.PrimaryRole == StaffRole.ADMIN, ct);
    private async Task<bool> ValidTeamsAsync(TeamScopeType scope, IReadOnlyList<Guid> ids, CancellationToken ct) => scope == TeamScopeType.ALL_TEAMS || (ids.Count > 0 && ids.Distinct().Count() == ids.Count && await db.Teams.CountAsync(x => ids.Contains(x.Id) && x.Status != TeamStatus.ARCHIVED, ct) == ids.Count);
    private void AddScopes(Guid userId, IReadOnlyList<Guid> ids, DateTimeOffset now) { foreach (var id in ids) db.StaffTeamScopes.Add(new StaffTeamScope { UserId = userId, TeamId = id, CreatedUtc = now }); }
    private static bool TryAccess(string? name, StaffRole role, bool? verify, bool? import, bool? medical, TeamScopeType scope, IReadOnlyList<Guid>? ids, out (string Name, StaffRole Role, bool Verify, bool Import, bool Medical, TeamScopeType Scope, IReadOnlyList<Guid> TeamIds) access) { access = default; if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120 || !Enum.IsDefined(role) || !Enum.IsDefined(scope) || !verify.HasValue || !import.HasValue || !medical.HasValue || ids is null) return false; var admin = role == StaffRole.ADMIN; if ((admin || scope == TeamScopeType.ALL_TEAMS) && ids.Count != 0) return false; if (!admin && scope == TeamScopeType.SELECTED_TEAMS && ids.Count == 0) return false; access = (name.Trim(), role, admin || verify.Value, admin || import.Value, admin || medical.Value, admin ? TeamScopeType.ALL_TEAMS : scope, admin || scope == TeamScopeType.ALL_TEAMS ? [] : ids); return true; }
    private static string EncodeToken(string token) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    private static string DecodeToken(string token) => Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
    private async Task<IReadOnlyList<StaffUserResponse>> MapAsync(IEnumerable<(StaffAccessProfile p, ApplicationUser u)> rows, CancellationToken ct) { var list = rows.ToList(); var ids = list.Select(x => x.p.UserId).ToList(); var scopes = await db.StaffTeamScopes.AsNoTracking().Where(x => ids.Contains(x.UserId)).OrderBy(x => x.TeamId).ToListAsync(ct); return list.Select(x => new StaffUserResponse(x.u.Id, x.p.DisplayName, x.u.Email ?? string.Empty, x.u.AccountStatus.ToString(), x.p.PrimaryRole, new(x.p.PrimaryRole == StaffRole.ADMIN || x.p.CanVerifyReports, x.p.PrimaryRole == StaffRole.ADMIN || x.p.CanImportData, x.p.PrimaryRole == StaffRole.ADMIN || x.p.CanViewMedicalDetails), new(x.p.PrimaryRole == StaffRole.ADMIN ? TeamScopeType.ALL_TEAMS : x.p.TeamScopeType, x.p.TeamScopeType == TeamScopeType.SELECTED_TEAMS ? scopes.Where(s => s.UserId == x.p.UserId).Select(s => s.TeamId).Distinct().ToList() : []), x.u.CreatedUtc, x.u.UpdatedUtc)).ToList(); }
}
