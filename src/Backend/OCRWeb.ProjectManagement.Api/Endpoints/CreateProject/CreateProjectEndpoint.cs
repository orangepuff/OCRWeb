using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.ProjectManagement.Application.Commands.CreateProject;

namespace OCRWeb.ProjectManagement.Api.Endpoints.CreateProject;

/// <summary>POST /projects — create a new project owned by the current user.</summary>
public class CreateProjectEndpoint(IMediator mediator) : Endpoint<CreateProjectRequest, CreateProjectResponse>
{
    public override void Configure()
    {
        Post("/projects");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateProjectRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateProjectCommand(req.Name), ct);
        await Send.OkAsync(new CreateProjectResponse(id), ct);
    }
}
