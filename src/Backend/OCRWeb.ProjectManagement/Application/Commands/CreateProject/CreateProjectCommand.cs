using MediatR;

namespace OCRWeb.ProjectManagement.Application.Commands.CreateProject;

/// <summary>Create a new project owned by the current user; returns the new project id.</summary>
public record CreateProjectCommand(string Name) : IRequest<int>;
