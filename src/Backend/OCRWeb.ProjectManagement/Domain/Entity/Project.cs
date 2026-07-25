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
        var project = new Project { Id = Guid.NewGuid(), Name = NormalizeName(name) };
        project.MarkInserted(userId, utcNow);
        return project;
    }

    /// <summary>Rename this project.</summary>
    public void Rename(string name, int userId, DateTime utcNow)
    {
        Name = NormalizeName(name);
        MarkUpdated(userId, utcNow);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        var trimmed = name.Trim();
        return trimmed.Length > 200 ? trimmed[..200] : trimmed;
    }
}
