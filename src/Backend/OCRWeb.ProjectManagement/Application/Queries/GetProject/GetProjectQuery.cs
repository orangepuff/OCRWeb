using MediatR;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.ProjectManagement.Application.Queries.GetProject;

/// <summary>Get a project owned by the current user.</summary>
public record GetProjectQuery(Guid Id) : IRequest<ProjectListItemDto>;
