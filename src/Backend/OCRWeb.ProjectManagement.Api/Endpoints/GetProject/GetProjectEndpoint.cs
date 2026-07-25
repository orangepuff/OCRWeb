using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.ProjectManagement.Application.Queries.GetProject;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Api.Endpoints.GetProject;

/// <summary>GET /api/projects/{Id} — get a project owned by the current user.</summary>
public class GetProjectEndpoint(IMediator mediator) : Endpoint<GetProjectRequest, ProjectListItemDto>
{
    public override void Configure()
    {
        Get("/api/projects/{Id}");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetProjectRequest req, CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(new GetProjectQuery(req.Id), ct);
            await Send.OkAsync(dto, ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
        catch (UnauthorizedAccessException)
        {
            await Send.ForbiddenAsync(ct);
        }
    }
}
