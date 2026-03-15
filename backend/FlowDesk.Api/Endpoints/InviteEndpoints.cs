using System.Security.Claims;
using FlowDesk.Application.Invites;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Api.Endpoints;

public static class InviteEndpoints
{
    public static void MapInviteEndpoints(this WebApplication app)
    {
        app.MapPost("/api/workspaces/{slug}/invites", SendInvite)
           .RequireAuthorization();
           
        app.MapPost("/api/invites/{token}/accept", AcceptInvite)
           .RequireAuthorization();
    }

    record SendInviteRequest(string Email, MemberRole Role);

    static async Task<IResult> SendInvite(
        string slug, SendInviteRequest req,
        ClaimsPrincipal user, InviteService svc)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var result = await svc.SendAsync(slug, userId, req.Email, req.Role);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    static async Task<IResult> AcceptInvite(
        string token, ClaimsPrincipal user, InviteService svc)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var slug = await svc.AcceptAsync(token, userId);
            return Results.Ok(new { workspaceSlug = slug });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
