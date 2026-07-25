using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.ProjectManagement.Application.Commands.UpdateProject;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Api.Endpoints.UpdateProject;

/// <summary>PUT /projects/{Id} — rename a project owned by the current user.</summary>
public class UpdateProjectEndpoint(IMediator mediator) : Endpoint<UpdateProjectRequest, ProjectListItemDto>
{
    public override void Configure()
    {
        Put("/projects/{Id}");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(new UpdateProjectCommand(req.Id, req.Name), ct);
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
