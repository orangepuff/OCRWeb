using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Theme.Domain.Entity;

namespace OrangepuffPortal.Theme.Infrastructure.Seeding;

/// <summary>
/// Idempotent seeder: creates the "Default" theme with all standard sections, elements, and property
/// definitions if it does not already exist. Runs on every startup when DoMigration = true.
/// </summary>
public class ThemeDbSeeder(ThemeDbContext db)
{
    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var theme = await db.Themes
            .FirstOrDefaultAsync(x => x.ThemeCode == Domain.Entity.Theme.DefaultThemeCode, ct);

        if (theme is null)
        {
            theme = new Domain.Entity.Theme(Domain.Entity.Theme.DefaultThemeCode, "Default light theme for the application", isActive: true, SeedTime);
            await db.Themes.AddAsync(theme, ct);
            await db.SaveChangesAsync(ct);
        }

        await SeedSectionsAsync(theme.Id, ct);
    }

    private async Task SeedSectionsAsync(int themeId, CancellationToken ct)
    {
        var existing = await db.ThemeSections
            .Where(x => x.ThemeId == themeId)
            .Select(x => x.SectionCode)
            .ToHashSetAsync(ct);

        var sections = new[]
        {
            ("GlobalColors",  "Global Material Design color tokens used across all components",   0),
            ("Project",       "Manage colors and styles for project-related components",           1),
            ("Buttons",       "Button styles including primary, secondary and danger",             2),
            ("Typography",    "Font families, sizes, weights and line heights",                    3),
            ("Navigation",    "Sidebar, top bar and breadcrumb visual settings",                   4),
            ("FormControls",  "Input, select, checkbox, radio and toggle styles",                  5),
            ("DataDisplay",   "Badge, chip, tag and status indicator styles",                      6),
            ("Table",         "Data table, pagination and row styles",                             7),
            ("DialogPopup",   "Modal dialog and overlay visual settings",                          8),
            ("AlertFeedback", "Alert, toast and snackbar notification styles",                     9),
            ("Layout",        "Page layout, spacing, container and grid settings",                10),
            ("Others",        "Miscellaneous component and utility styles",                       11),
        };

        foreach (var (code, desc, sort) in sections)
        {
            if (existing.Contains(code))
            {
                continue;
            }
            var section = new ThemeSection(themeId, code, desc, sort, SeedTime);
            await db.ThemeSections.AddAsync(section, ct);
            await db.SaveChangesAsync(ct);

            await SeedElementsAsync(section.Id, code, ct);
        }
    }

    private async Task SeedElementsAsync(int sectionId, string sectionCode, CancellationToken ct)
    {
        var existing = await db.ThemeElements
            .Where(x => x.ThemeSectionId == sectionId)
            .Select(x => x.ElementCode)
            .ToHashSetAsync(ct);

        var elementMap = GetElementMap();
        if (!elementMap.TryGetValue(sectionCode, out var elements))
        {
            return;
        }

        foreach (var (code, desc, sort) in elements)
        {
            if (existing.Contains(code))
            {
                continue;
            }
            var element = new ThemeElement(sectionId, code, desc, sort, SeedTime);
            await db.ThemeElements.AddAsync(element, ct);
            await db.SaveChangesAsync(ct);

            await SeedDetailsAsync(element.Id, sectionCode, code, ct);
        }
    }

    private async Task SeedDetailsAsync(int elementId, string sectionCode, string elementCode, CancellationToken ct)
    {
        var detailMap = GetDetailMap();
        var key = $"{sectionCode}.{elementCode}";
        if (!detailMap.TryGetValue(key, out var details))
        {
            return;
        }

        foreach (var (propKey, label, description, type, allowedValues, value, unit, sort) in details)
        {
            var detail = new ThemeDetail(elementId, propKey, label, description, type, allowedValues, value, unit, sort, SeedTime);
            await db.ThemeDetails.AddAsync(detail, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    // ── Element definitions per section ─────────────────────────────────────────

    private static Dictionary<string, (string Code, string Desc, int Sort)[]> GetElementMap() => new()
    {
        ["GlobalColors"] =
        [
            ("Base", "Material Design 3 base color roles — emitted as --mat-sys-* tokens", 1),
        ],
        ["Project"] =
        [
            ("Card",     "Card component",           1),
            ("Panel",    "Panel container",          2),
            ("Badge",    "Badge / chip indicator",   3),
            ("Tag",      "Tag label",                4),
            ("Progress", "Progress bar",             5),
            ("Stepper",  "Stepper component",        6),
        ],
        ["Buttons"] =
        [
            ("Primary",   "Primary action button",   1),
            ("Secondary", "Secondary action button", 2),
            ("Danger",    "Destructive action button",3),
            ("Ghost",     "Ghost / text button",     4),
        ],
        ["Typography"] =
        [
            ("Heading1", "H1 headline",   1),
            ("Heading2", "H2 headline",   2),
            ("Heading3", "H3 headline",   3),
            ("Body",     "Body text",     4),
            ("Caption",  "Caption text",  5),
            ("Label",    "Form label",    6),
        ],
        ["Navigation"] =
        [
            ("Sidebar",     "Sidebar navigation",    1),
            ("TopBar",      "Top navigation bar",    2),
            ("Breadcrumb",  "Breadcrumb trail",      3),
        ],
        ["FormControls"] =
        [
            ("Input",    "Text input field",   1),
            ("Select",   "Select / dropdown",  2),
            ("Checkbox", "Checkbox control",   3),
            ("Radio",    "Radio button",       4),
            ("Toggle",   "Toggle / switch",    5),
        ],
        ["DataDisplay"] =
        [
            ("Badge",   "Status badge",     1),
            ("Chip",    "Chip / tag",       2),
            ("Tooltip", "Tooltip bubble",   3),
        ],
        ["Table"] =
        [
            ("Header",     "Table header row",  1),
            ("Row",        "Table body row",    2),
            ("Pagination", "Pagination bar",    3),
        ],
        ["DialogPopup"] =
        [
            ("Dialog",   "Modal dialog",     1),
            ("Overlay",  "Overlay backdrop", 2),
        ],
        ["AlertFeedback"] =
        [
            ("Success", "Success alert",   1),
            ("Warning", "Warning alert",   2),
            ("Error",   "Error alert",     3),
            ("Info",    "Info alert",      4),
            ("Snackbar","Snackbar toast",  5),
        ],
        ["Layout"] =
        [
            ("Page",      "Page background",     1),
            ("Container", "Content container",   2),
            ("Divider",   "Divider line",        3),
        ],
    };

    // ── Property definitions per element ────────────────────────────────────────

    private static readonly string ShadowOptions = "None;Small;Medium;Large;Extra Large";
    private static readonly string UnitOptions = "px;rem;%";

    private static Dictionary<string, (string Key, string Label, string Desc, string Type, string? Allowed, string? Value, string? Unit, int Sort)[]> GetDetailMap() => new()
    {
        // GlobalColors.Base properties map 1-to-1 with Material Design 3 color roles.
        // ThemeCssVarBuilder emits each as both --op-global-colors-base-* and --mat-sys-* so that
        // Angular Material components and custom styles using var(--mat-sys-*) respond to theme changes.
        ["GlobalColors.Base"] =
        [
            ("primary",                 "Primary",                  "Primary brand color",                       "color", null, "#0EA5E9", null,  1),
            ("onPrimary",               "On Primary",               "Text/icons on primary color",               "color", null, "#FFFFFF", null,  2),
            ("primaryContainer",        "Primary Container",        "Tonal primary container",                   "color", null, "#BAE6FD", null,  3),
            ("onPrimaryContainer",      "On Primary Container",     "Content on primary container",              "color", null, "#0C3F58", null,  4),
            ("secondary",               "Secondary",                "Secondary accent color",                    "color", null, "#0284C7", null,  5),
            ("onSecondary",             "On Secondary",             "Text/icons on secondary color",             "color", null, "#FFFFFF", null,  6),
            ("secondaryContainer",      "Secondary Container",      "Tonal secondary container",                 "color", null, "#E0F2FE", null,  7),
            ("onSecondaryContainer",    "On Secondary Container",   "Content on secondary container",            "color", null, "#075985", null,  8),
            ("tertiary",                "Tertiary",                 "Tertiary accent color",                     "color", null, "#0369A1", null,  9),
            ("onTertiary",              "On Tertiary",              "Text/icons on tertiary color",              "color", null, "#FFFFFF", null, 10),
            ("tertiaryContainer",       "Tertiary Container",       "Tonal tertiary container",                  "color", null, "#BAE6FD", null, 11),
            ("onTertiaryContainer",     "On Tertiary Container",    "Content on tertiary container",             "color", null, "#023E58", null, 12),
            ("error",                   "Error",                    "Error state color",                         "color", null, "#EF4444", null, 13),
            ("onError",                 "On Error",                 "Text/icons on error color",                 "color", null, "#FFFFFF", null, 14),
            ("errorContainer",          "Error Container",          "Tonal error container",                     "color", null, "#FEE2E2", null, 15),
            ("onErrorContainer",        "On Error Container",       "Content on error container",                "color", null, "#991B1B", null, 16),
            ("surface",                 "Surface",                  "Default surface background",                "color", null, "#FFFFFF", null, 17),
            ("onSurface",               "On Surface",               "Text/icons on surface",                     "color", null, "#0F172A", null, 18),
            ("surfaceVariant",          "Surface Variant",          "Alternative surface color",                 "color", null, "#F1F5F9", null, 19),
            ("onSurfaceVariant",        "On Surface Variant",       "Text/icons on surface variant",             "color", null, "#475569", null, 20),
            ("outline",                 "Outline",                  "Border and divider color",                  "color", null, "#CBD5E1", null, 21),
            ("outlineVariant",          "Outline Variant",          "Subtle border and divider color",           "color", null, "#E2E8F0", null, 22),
            ("surfaceContainerLowest",  "Surface Container Lowest", "Lowest surface container tone",             "color", null, "#FFFFFF", null, 23),
            ("surfaceContainerLow",     "Surface Container Low",    "Low surface container tone",                "color", null, "#F8FAFC", null, 24),
            ("surfaceContainer",        "Surface Container",        "Mid surface container tone",                "color", null, "#F1F5F9", null, 25),
            ("surfaceContainerHigh",    "Surface Container High",   "High surface container tone",               "color", null, "#E8EEF5", null, 26),
            ("surfaceContainerHighest", "Surface Container Highest","Highest surface container tone",            "color", null, "#DDE7F0", null, 27),
            ("background",              "Background",               "Page background color",                     "color", null, "#F8FAFC", null, 28),
            ("onBackground",            "On Background",            "Text/icons on page background",             "color", null, "#374151", null, 29),
            ("inverseSurface",          "Inverse Surface",          "Dark surface for contrasting elements",     "color", null, "#1E293B", null, 30),
            ("inverseOnSurface",        "Inverse On Surface",       "Content on inverse surface",                "color", null, "#F1F5F9", null, 31),
            ("inversePrimary",          "Inverse Primary",          "Primary color on dark surfaces",            "color", null, "#7DD3FC", null, 32),
            ("scrim",                   "Scrim",                    "Modal overlay scrim color",                 "color", null, "#000000", null, 33),
        ],
        ["Project.Card"] =
        [
            ("backgroundColor",  "Background Color",  "Background color of card",      "color",       null,          "#FFFFFF", null, 1),
            ("borderColor",      "Border Color",      "Border color of card",          "color",       null,          "#E2E8F0", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius of card",         "number_unit", UnitOptions,   "12",      "px", 3),
            ("shadow",           "Shadow",            "Shadow elevation of card",      "dropdown",    ShadowOptions, "Medium",  null, 4),
            ("padding",          "Padding",           "Inner spacing of card",         "number_unit", UnitOptions,   "16",      "px", 5),
            ("margin",           "Margin",            "Outer spacing of card",         "number_unit", UnitOptions,   "16",      "px", 6),
            ("titleColor",       "Title Color",       "Color of card title text",      "color",       null,          "#1E293B", null, 7),
            ("bodyTextColor",    "Body Text Color",   "Color of body text",            "color",       null,          "#475569", null, 8),
        ],
        ["Project.Panel"] =
        [
            ("backgroundColor",  "Background Color",  "Background color of panel",    "color",       null,          "#F8FAFC", null, 1),
            ("borderColor",      "Border Color",      "Border color of panel",        "color",       null,          "#E2E8F0", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius of panel",       "number_unit", UnitOptions,   "8",       "px", 3),
            ("padding",          "Padding",           "Inner spacing of panel",       "number_unit", UnitOptions,   "24",      "px", 4),
            ("titleColor",       "Title Color",       "Color of panel title",         "color",       null,          "#1E293B", null, 5),
        ],
        ["Project.Badge"] =
        [
            ("backgroundColor",  "Background Color",  "Badge background",             "color",       null,          "#EFF6FF", null, 1),
            ("textColor",        "Text Color",        "Badge text color",             "color",       null,          "#1D4ED8", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius of badge",       "number_unit", UnitOptions,   "4",       "px", 3),
            ("fontSize",         "Font Size",         "Badge font size",              "number_unit", UnitOptions,   "12",      "px", 4),
        ],
        ["Project.Tag"] =
        [
            ("backgroundColor",  "Background Color",  "Tag background",               "color",       null,          "#F1F5F9", null, 1),
            ("textColor",        "Text Color",        "Tag text color",               "color",       null,          "#475569", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius of tag",         "number_unit", UnitOptions,   "4",       "px", 3),
        ],
        ["Project.Progress"] =
        [
            ("trackColor",       "Track Color",       "Progress track background",    "color",       null,          "#E2E8F0", null, 1),
            ("fillColor",        "Fill Color",        "Progress fill color",          "color",       null,          "#0EA5E9", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "999",     "px", 3),
            ("height",           "Height",            "Progress bar height",          "number_unit", UnitOptions,   "8",       "px", 4),
        ],
        ["Project.Stepper"] =
        [
            ("activeColor",      "Active Color",      "Active step color",            "color",       null,          "#0EA5E9", null, 1),
            ("completedColor",   "Completed Color",   "Completed step color",         "color",       null,          "#22C55E", null, 2),
            ("inactiveColor",    "Inactive Color",    "Inactive step color",          "color",       null,          "#CBD5E1", null, 3),
            ("labelColor",       "Label Color",       "Step label text color",        "color",       null,          "#475569", null, 4),
        ],
        ["Buttons.Primary"] =
        [
            ("backgroundColor",  "Background Color",  "Button background",            "color",       null,          "#0EA5E9", null, 1),
            ("textColor",        "Text Color",        "Button text color",            "color",       null,          "#FFFFFF", null, 2),
            ("hoverColor",       "Hover Color",       "Background on hover",          "color",       null,          "#0284C7", null, 3),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "6",       "px", 4),
            ("fontSize",         "Font Size",         "Button font size",             "number_unit", UnitOptions,   "14",      "px", 5),
            ("padding",          "Padding",           "Inner padding",                "number_unit", UnitOptions,   "10",      "px", 6),
        ],
        ["Buttons.Secondary"] =
        [
            ("backgroundColor",  "Background Color",  "Button background",            "color",       null,          "#FFFFFF", null, 1),
            ("textColor",        "Text Color",        "Button text color",            "color",       null,          "#0EA5E9", null, 2),
            ("borderColor",      "Border Color",      "Button border",                "color",       null,          "#0EA5E9", null, 3),
            ("hoverColor",       "Hover Color",       "Background on hover",          "color",       null,          "#F0F9FF", null, 4),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "6",       "px", 5),
        ],
        ["Buttons.Danger"] =
        [
            ("backgroundColor",  "Background Color",  "Button background",            "color",       null,          "#EF4444", null, 1),
            ("textColor",        "Text Color",        "Button text color",            "color",       null,          "#FFFFFF", null, 2),
            ("hoverColor",       "Hover Color",       "Background on hover",          "color",       null,          "#DC2626", null, 3),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "6",       "px", 4),
        ],
        ["Buttons.Ghost"] =
        [
            ("textColor",        "Text Color",        "Button text color",            "color",       null,          "#475569", null, 1),
            ("hoverColor",       "Hover Color",       "Background on hover",          "color",       null,          "#F1F5F9", null, 2),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "6",       "px", 3),
        ],
        ["Typography.Body"] =
        [
            ("fontFamily",       "Font Family",       "Body font family",             "text",        null,          "Inter, sans-serif", null, 1),
            ("fontSize",         "Font Size",         "Body font size",               "number_unit", UnitOptions,   "14",      "px",  2),
            ("lineHeight",       "Line Height",       "Body line height",             "number_unit", UnitOptions,   "1.5",     "rem", 3),
            ("color",            "Text Color",        "Body text color",              "color",       null,          "#374151", null,  4),
        ],
        ["Typography.Heading1"] =
        [
            ("fontSize",         "Font Size",         "H1 font size",                 "number_unit", UnitOptions,   "32",      "px", 1),
            ("fontWeight",       "Font Weight",       "H1 font weight",               "dropdown",    "300;400;500;600;700;800;900", "700", null, 2),
            ("color",            "Text Color",        "H1 text color",                "color",       null,          "#111827", null, 3),
            ("lineHeight",       "Line Height",       "H1 line height",               "number_unit", UnitOptions,   "1.2",     "rem", 4),
        ],
        ["Typography.Heading2"] =
        [
            ("fontSize",         "Font Size",         "H2 font size",                 "number_unit", UnitOptions,   "24",      "px", 1),
            ("fontWeight",       "Font Weight",       "H2 font weight",               "dropdown",    "300;400;500;600;700;800;900", "600", null, 2),
            ("color",            "Text Color",        "H2 text color",                "color",       null,          "#111827", null, 3),
        ],
        ["Navigation.Sidebar"] =
        [
            ("backgroundColor",  "Background Color",  "Sidebar background",           "color",       null,          "#1E293B", null, 1),
            ("textColor",        "Text Color",        "Sidebar text color",           "color",       null,          "#CBD5E1", null, 2),
            ("activeItemColor",  "Active Item Color", "Active nav item background",   "color",       null,          "#334155", null, 3),
            ("activeTextColor",  "Active Text Color", "Active nav item text",         "color",       null,          "#FFFFFF", null, 4),
            ("width",            "Width",             "Sidebar width",                "number_unit", UnitOptions,   "240",     "px", 5),
        ],
        ["Navigation.TopBar"] =
        [
            ("backgroundColor",  "Background Color",  "Top bar background",           "color",       null,          "#FFFFFF", null, 1),
            ("textColor",        "Text Color",        "Top bar text color",           "color",       null,          "#1E293B", null, 2),
            ("borderColor",      "Border Color",      "Top bar bottom border",        "color",       null,          "#E2E8F0", null, 3),
            ("height",           "Height",            "Top bar height",               "number_unit", UnitOptions,   "64",      "px", 4),
        ],
        ["FormControls.Input"] =
        [
            ("backgroundColor",  "Background Color",  "Input background",             "color",       null,          "#FFFFFF", null, 1),
            ("borderColor",      "Border Color",      "Input border",                 "color",       null,          "#CBD5E1", null, 2),
            ("focusBorderColor", "Focus Border Color","Border color when focused",    "color",       null,          "#0EA5E9", null, 3),
            ("textColor",        "Text Color",        "Input text color",             "color",       null,          "#1E293B", null, 4),
            ("borderRadius",     "Border Radius",     "Corner radius",               "number_unit", UnitOptions,   "6",       "px", 5),
            ("fontSize",         "Font Size",         "Input font size",              "number_unit", UnitOptions,   "14",      "px", 6),
        ],
        ["Table.Header"] =
        [
            ("backgroundColor",  "Background Color",  "Header row background",        "color",       null,          "#F8FAFC", null, 1),
            ("textColor",        "Text Color",        "Header text color",            "color",       null,          "#475569", null, 2),
            ("borderColor",      "Border Color",      "Header bottom border",         "color",       null,          "#E2E8F0", null, 3),
            ("fontSize",         "Font Size",         "Header font size",             "number_unit", UnitOptions,   "12",      "px", 4),
        ],
        ["Table.Row"] =
        [
            ("backgroundColor",  "Background Color",  "Row background",               "color",       null,          "#FFFFFF", null, 1),
            ("hoverColor",       "Hover Color",       "Row hover background",         "color",       null,          "#F8FAFC", null, 2),
            ("textColor",        "Text Color",        "Row text color",               "color",       null,          "#374151", null, 3),
            ("borderColor",      "Border Color",      "Row divider color",            "color",       null,          "#F1F5F9", null, 4),
        ],
        ["AlertFeedback.Success"] =
        [
            ("backgroundColor",  "Background Color",  "Success alert background",     "color",       null,          "#F0FDF4", null, 1),
            ("textColor",        "Text Color",        "Success alert text",           "color",       null,          "#15803D", null, 2),
            ("borderColor",      "Border Color",      "Success alert border",         "color",       null,          "#86EFAC", null, 3),
        ],
        ["AlertFeedback.Error"] =
        [
            ("backgroundColor",  "Background Color",  "Error alert background",       "color",       null,          "#FEF2F2", null, 1),
            ("textColor",        "Text Color",        "Error alert text",             "color",       null,          "#DC2626", null, 2),
            ("borderColor",      "Border Color",      "Error alert border",           "color",       null,          "#FCA5A5", null, 3),
        ],
        ["AlertFeedback.Warning"] =
        [
            ("backgroundColor",  "Background Color",  "Warning alert background",     "color",       null,          "#FFFBEB", null, 1),
            ("textColor",        "Text Color",        "Warning alert text",           "color",       null,          "#D97706", null, 2),
            ("borderColor",      "Border Color",      "Warning alert border",         "color",       null,          "#FCD34D", null, 3),
        ],
        ["Layout.Page"] =
        [
            ("backgroundColor",  "Background Color",  "Page background",              "color",       null,          "#F1F5F9", null, 1),
            ("textColor",        "Text Color",        "Page default text color",      "color",       null,          "#374151", null, 2),
        ],
    };
}
