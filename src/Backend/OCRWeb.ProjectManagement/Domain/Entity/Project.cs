using OrangepuffPortal.Shared.Domain;

namespace OCRWeb.ProjectManagement.Domain.Entity;

/// <summary>
/// Aggregate root for a project. Maps to [project].[Projects]. Ownership is derived from
/// the audit InsertedUserId (no separate owner field), matching the codebase's convention.
/// </summary>
public class Project : AuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Project() { } // EF

    /// <summary>Create a new project owned by the given user.</summary>
    public static Project Create(string name, int userId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        var trimmed = name.Trim();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = trimmed.Length > 200 ? trimmed[..200] : trimmed
        };
        project.MarkInserted(userId, utcNow);
        return project;
    }
}
