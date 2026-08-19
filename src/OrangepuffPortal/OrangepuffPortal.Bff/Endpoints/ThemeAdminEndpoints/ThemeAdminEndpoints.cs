using OrangepuffPortal.Theme.Contract;
using OrangepuffPortal.Theme.Domain.Repositories;
using System.Security.Claims;
using ThemeEntity = OrangepuffPortal.Theme.Domain.Entity.Theme;

namespace OrangepuffPortal.Bff.Endpoints.ThemeAdminEndpoints;

/// <summary>
/// Maps /bff/admin/themes/* — full CRUD for themes, sections, elements and details.
/// Mapped under the /bff/admin group which already requires the AdminOnly policy.
/// </summary>
public static class ThemeAdminEndpoints
{
    public static void MapThemeAdminEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Themes ────────────────────────────────────────────────────────────

        app.MapGet("/themes", async (IThemeRepository repo, CancellationToken ct) =>
        {
            var themes = await repo.ListThemesAsync(ct);
            return Results.Ok(themes.Select(t => new
            {
                id = t.Id,
                themeCode = t.ThemeCode,
                description = t.Description,
                isActive = t.IsActive
            }));
        });

        app.MapGet("/themes/{id:int}", async (int id, IThemeRepository repo, CancellationToken ct) =>
        {
            var theme = await repo.GetFullThemeAsync(id, ct);
            if (theme is null)
            {
                return Results.NotFound();
            }

            var sections = await repo.ListSectionsAsync(theme.Id, ct);
            var sectionDtos = new List<ThemeSectionDto>();

            foreach (var section in sections)
            {
                var elements = await repo.ListElementsAsync(section.Id, ct);
                var elementDtos = new List<ThemeElementDto>();

                foreach (var element in elements)
                {
                    var details = await repo.ListDetailsAsync(element.Id, ct);
                    elementDtos.Add(new ThemeElementDto
                    {
                        Id = element.Id,
                        ElementCode = element.ElementCode,
                        Description = element.Description,
                        SortOrder = element.SortOrder,
                        Details = details.Select(d => new ThemeDetailDto
                        {
                            Id = d.Id,
                            PropertyKey = d.PropertyKey,
                            PropertyLabel = d.PropertyLabel,
                            PropertyDescription = d.PropertyDescription,
                            PropertyType = d.PropertyType,
                            PropertyValue = d.PropertyValue,
                            Unit = d.Unit,
                            SortOrder = d.SortOrder
                        }).ToList()
                    });
                }

                sectionDtos.Add(new ThemeSectionDto
                {
                    Id = section.Id,
                    SectionCode = section.SectionCode,
                    Description = section.Description,
                    SortOrder = section.SortOrder,
                    Elements = elementDtos
                });
            }

            return Results.Ok(new ThemeDto
            {
                Id = theme.Id,
                ThemeCode = theme.ThemeCode,
                Description = theme.Description,
                IsActive = theme.IsActive,
                Sections = sectionDtos
            });
        });

        app.MapPost("/themes", async (AddThemeRequest req, ClaimsPrincipal user, IThemeRepository repo, CancellationToken ct) =>
        {
            if (await repo.ExistsByCodeAsync(req.ThemeCode, null, ct))
            {
                return Results.Conflict($"Theme code '{req.ThemeCode}' already exists.");
            }

            var actorId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var theme = new ThemeEntity(req.ThemeCode, req.Description, req.IsActive, DateTime.UtcNow, actorId);
            await repo.AddThemeAsync(theme, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Ok(new { id = theme.Id });
        });

        app.MapPut("/themes/{id:int}", async (int id, UpdateThemeRequest req, ClaimsPrincipal user, IThemeRepository repo, CancellationToken ct) =>
        {
            var theme = await repo.FindByIdAsync(id, ct);
            if (theme is null)
            {
                return Results.NotFound();
            }

            if (await repo.ExistsByCodeAsync(req.ThemeCode, id, ct))
            {
                return Results.Conflict($"Theme code '{req.ThemeCode}' already exists.");
            }

            var actorId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            theme.Update(req.ThemeCode, req.Description, req.IsActive, DateTime.UtcNow, actorId);
            await repo.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        app.MapDelete("/themes/{id:int}", async (int id, IThemeRepository repo, CancellationToken ct) =>
        {
            var theme = await repo.FindByIdAsync(id, ct);
            if (theme is null)
            {
                return Results.NotFound();
            }

            if (theme.ThemeCode == ThemeEntity.DefaultThemeCode)
            {
                return Results.BadRequest("The Default theme cannot be deleted.");
            }

            await repo.DeleteThemeAsync(theme, ct);
            await repo.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        // ── Details (bulk save per element) ───────────────────────────────────

        app.MapPut("/themes/{themeId:int}/elements/{elementId:int}/details", async (
            int themeId, int elementId,
            IReadOnlyList<SaveDetailRequest> requests,
            ClaimsPrincipal user,
            IThemeRepository repo,
            CancellationToken ct) =>
        {
            var actorId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existing = await repo.ListDetailsAsync(elementId, ct);
            var now = DateTime.UtcNow;

            foreach (var req in requests)
            {
                var detail = existing.FirstOrDefault(d => d.Id == req.Id);
                if (detail is not null)
                {
                    detail.UpdateValue(req.PropertyValue, req.Unit, now, actorId);
                }
            }

            await repo.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}

public record AddThemeRequest(string ThemeCode, string Description, bool IsActive);
public record UpdateThemeRequest(string ThemeCode, string Description, bool IsActive);
public record SaveDetailRequest(int Id, string? PropertyValue, string? Unit);
