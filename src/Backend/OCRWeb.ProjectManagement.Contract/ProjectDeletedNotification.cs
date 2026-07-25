using MediatR;

namespace OCRWeb.ProjectManagement.Contract;

/// <summary>
/// Published after a project is permanently deleted, carrying only its id - other bounded contexts that own data keyed by project id (e.g. Document's files) react to this to tear their own data down, without ProjectManagement knowing anything about them.
/// </summary>
public record ProjectDeletedNotification(Guid ProjectId) : INotification;
