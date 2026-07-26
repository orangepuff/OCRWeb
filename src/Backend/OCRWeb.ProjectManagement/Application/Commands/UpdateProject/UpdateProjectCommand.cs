using MediatR;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Application.Commands.UpdateProject;

/// <summary>Rename a project owned by the current user.</summary>
public record UpdateProjectCommand(int Id, string Name) : IRequest<ProjectListItemDto>;
