using Microsoft.EntityFrameworkCore;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;

namespace OCRWeb.ProjectManagement.Infrastructure.Repositories;

public class ProjectRepository(ProjectDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Projects.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Project>> ListByOwnerAsync(int ownerUserId, CancellationToken ct = default) =>
        await db.Projects
            .Where(x => x.InsertedUserId == ownerUserId)
            .OrderByDescending(x => x.InsertedTime)
            .ToListAsync(ct);

    public async Task AddAsync(Project project, CancellationToken ct = default) =>
        await db.Projects.AddAsync(project, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
