using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.ProjectManagement.Application.Commands.DeleteProject;

namespace OCRWeb.ProjectManagement.Api.Endpoints.DeleteProject;

/// <summary>DELETE /projects/{Id} — delete a project owned by the current user.</summary>
public class DeleteProjectEndpoint(IMediator mediator) : Endpoint<DeleteProjectRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/projects/{Id}");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteProjectRequest req, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteProjectCommand(req.Id), ct);
            await Send.NoContentAsync(ct);
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
