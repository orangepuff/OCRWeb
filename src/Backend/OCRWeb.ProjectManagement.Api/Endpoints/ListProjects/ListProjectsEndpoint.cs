using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.ProjectManagement.Application.Queries.ListProjects;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Api.Endpoints.ListProjects;

/// <summary>GET /api/projects — list the current user's own projects.</summary>
public class ListProjectsEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<ProjectListItemDto>>
{
    public override void Configure()
    {
        Get("/api/projects");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListProjectsQuery(), ct);
        await Send.OkAsync(result, ct);
    }
}
