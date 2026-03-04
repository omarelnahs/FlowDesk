using System.Security.Claims;
using FlowDesk.Application.Workspaces;

namespace FlowDesk.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapWorkspaceEndpoints(this WebApplication app)
    {
        // Both routes require a valid JWT
        var group = app.MapGroup("/api/workspaces").RequireAuthorization();

        group.MapGet("/",    GetMyWorkspaces);
        group.MapPost("/",   CreateWorkspace);
    }

    record CreateRequest(string Name);

    static async Task<IResult> GetMyWorkspaces(
        ClaimsPrincipal user, WorkspacesService svc)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Results.Ok(await svc.GetForUserAsync(userId));
    }

    static async Task<IResult> CreateWorkspace(
        CreateRequest req, ClaimsPrincipal user, WorkspacesService svc)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await svc.CreateAsync(req.Name, userId);
        return Results.Created($"/api/workspaces/{result.Slug}", result);
    }
}
