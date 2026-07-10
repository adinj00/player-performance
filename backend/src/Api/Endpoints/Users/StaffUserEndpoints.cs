using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Application.Users;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Api.Endpoints.Users;

internal static class StaffUserEndpoints
{
    public static IEndpointRouteBuilder MapStaffUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users").RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        group.MapGet("", (string? search, StaffRole? role, string? status, TeamScopeType? scopeType, Guid? teamId, IStaffUsersService service, CancellationToken ct) => service.ListAsync(search, role, status, scopeType, teamId, ct));
        group.MapGet("/{userId:guid}", GetAsync);
        group.MapPost("/invitations", CreateAsync);
        group.MapPost("/{userId:guid}/invitations/reissue", ReissueAsync);
        group.MapPatch("/{userId:guid}", UpdateAsync);
        group.MapPut("/{userId:guid}/access", ReplaceAccessAsync);
        group.MapPost("/{userId:guid}/disable", DisableAsync);
        group.MapPost("/{userId:guid}/reactivate", ReactivateAsync);
        endpoints.MapPost("/api/auth/invitations/accept", AcceptAsync).AllowAnonymous().WithTags("Auth");
        return endpoints;
    }
    private static async Task<IResult> GetAsync(Guid userId, IStaffUsersService service, CancellationToken ct) => (await service.GetAsync(userId, ct)) is { } user ? TypedResults.Ok(user) : TypedResults.NotFound();
    private static async Task<IResult> CreateAsync(HttpContext c, IAntiforgery a, CreateStaffInvitationRequest r, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToCreated(await s.CreateInvitationAsync(r, ct), c); }
    private static async Task<IResult> ReissueAsync(Guid userId, HttpContext c, IAntiforgery a, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToResult(await s.ReissueInvitationAsync(userId, ct), c); }
    private static async Task<IResult> UpdateAsync(Guid userId, HttpContext c, IAntiforgery a, UpdateStaffProfileRequest r, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToResult(await s.UpdateProfileAsync(userId, r, ct), c); }
    private static async Task<IResult> ReplaceAccessAsync(Guid userId, HttpContext c, IAntiforgery a, ReplaceStaffAccessRequest r, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToResult(await s.ReplaceAccessAsync(userId, r, ct), c); }
    private static async Task<IResult> DisableAsync(Guid userId, HttpContext c, IAntiforgery a, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToResult(await s.DisableAsync(userId, ct), c); }
    private static async Task<IResult> ReactivateAsync(Guid userId, HttpContext c, IAntiforgery a, IStaffUsersService s, CancellationToken ct) { var f = await AntiforgeryValidation.ValidateRequestAsync(c, a); return f ?? ToResult(await s.ReactivateAsync(userId, ct), c); }
    private static async Task<IResult> AcceptAsync(HttpContext c, AcceptStaffInvitationRequest r, IStaffUsersService s, CancellationToken ct) { var result = await s.AcceptInvitationAsync(r, ct); return result.IsSuccess ? TypedResults.NoContent() : ToProblem(result.Error, c); }
    private static IResult ToCreated(Result<StaffInvitationCredentialResponse> r, HttpContext c) => r.IsSuccess ? TypedResults.Created($"/api/users/{r.Value.User.Id}", r.Value) : ToProblem(r.Error, c);
    private static IResult ToResult<T>(Result<T> r, HttpContext c) => r.IsSuccess ? TypedResults.Ok(r.Value) : ToProblem(r.Error, c);
    private static IResult ToProblem(Error e, HttpContext c) { var status = e.Code switch { "not_found" => 404, "duplicate_email" or "staff_conflict" => 409, "invalid_invitation" => 400, "validation_failed" => 422, _ => 400 }; return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = e.Message, Extensions = { ["code"] = e.Code, ["traceId"] = c.TraceIdentifier } }); }
}
