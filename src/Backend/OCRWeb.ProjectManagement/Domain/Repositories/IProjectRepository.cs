using OCRWeb.ProjectManagement.Domain.Entity;

namespace OCRWeb.ProjectManagement.Domain.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Projects owned (created) by the given user, newest first.</summary>
    Task<IReadOnlyList<Project>> ListByOwnerAsync(int ownerUserId, CancellationToken ct = default);

    Task AddAsync(Project project, CancellationToken ct = default);
    void Remove(Project project);
    Task SaveChangesAsync(CancellationToken ct = default);
}
