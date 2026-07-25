using MediatR;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Application.Queries.ListProjects;

/// <summary>List the current user's own projects.</summary>
public record ListProjectsQuery : IRequest<IReadOnlyList<ProjectListItemDto>>;
