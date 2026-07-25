using MediatR;

namespace OCRWeb.ProjectManagement.Application.Commands.DeleteProject;

/// <summary>Delete a project owned by the current user.</summary>
public record DeleteProjectCommand(Guid Id) : IRequest;
