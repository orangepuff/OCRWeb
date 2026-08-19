import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { ThemeAdminService } from './theme-admin.service';
import { ThemeDetail, ThemeElement, ThemeFull, ThemeItem, ThemeSection } from './theme-models';
import { ThemeApplyService } from '../../theme/theme-apply.service';
import { TranslatePipe } from '../../translation/translate.pipe';
import { TranslationService } from '../../translation/translation.service';

const MODULE = 'OrangepuffPortal.Frontend';
const SNACK_MS = 3000;

@Component({
  selector: 'lib-portal-theme-page',
  imports: [
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSlideToggleModule,
    TranslatePipe
  ],
  templateUrl: './theme-page.html',
  styleUrl: './theme-page.scss'
})
export class ThemePage implements OnInit {
  private readonly service = inject(ThemeAdminService);
  private readonly themeApplyService = inject(ThemeApplyService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;

  // ── State ──────────────────────────────────────────────────────────────────
  protected readonly themes = signal<ThemeItem[]>([]);
  protected readonly selectedTheme = signal<ThemeFull | null>(null);
  protected readonly selectedSection = signal<ThemeSection | null>(null);
  protected readonly selectedElement = signal<ThemeElement | null>(null);
  protected readonly previewMode = signal<'desktop' | 'tablet' | 'mobile'>('desktop');
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly sectionSearch = signal('');

  protected readonly filteredSections = computed(() => {
    const q = this.sectionSearch().toLowerCase();
    return (this.selectedTheme()?.sections ?? []).filter(s =>
      !q || s.description.toLowerCase().includes(q) || s.sectionCode.toLowerCase().includes(q)
    );
  });

  protected readonly isDefault = computed(() =>
    this.selectedTheme()?.themeCode === 'Default'
  );

  // ── Unit options for number_unit properties ─────────────────────────────
  protected readonly unitOptions = ['px', 'rem', '%'];

  ngOnInit(): void {
    this.loadThemes();
  }

  // ── Theme list ─────────────────────────────────────────────────────────────

  private loadThemes(): void {
    this.service.listThemes().subscribe(list => {
      this.themes.set(list);
      if (list.length > 0 && !this.selectedTheme()) {
        this.selectTheme(list[0].id);
      }
    });
  }

  protected selectTheme(id: number): void {
    this.loading.set(true);
    this.selectedSection.set(null);
    this.selectedElement.set(null);

    this.service.getTheme(id).subscribe({
      next: theme => {
        this.initEditState(theme);
        this.selectedTheme.set(theme);
        if (theme.sections.length > 0) {
          this.selectSection(theme.sections[0]);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  protected onThemeSelect(id: number): void {
    this.selectTheme(id);
  }

  // ── Sections / Elements ────────────────────────────────────────────────────

  protected selectSection(section: ThemeSection): void {
    this.selectedSection.set(section);
    this.selectedElement.set(section.elements[0] ?? null);
  }

  protected selectElement(element: ThemeElement): void {
    this.selectedElement.set(element);
  }

  // ── Property editing ───────────────────────────────────────────────────────

  private initEditState(theme: ThemeFull): void {
    for (const section of theme.sections) {
      for (const element of section.elements) {
        for (const detail of element.details) {
          detail.editValue = detail.propertyValue;
          detail.editUnit = detail.unit;
        }
      }
    }
  }

  protected getAllowedValues(detail: ThemeDetail): string[] {
    if (!detail.propertyType) { return []; }
    // AllowedValues are stored on the DTO but ThemeDetailDto doesn't expose it currently.
    // For dropdown types we fall back to a sensible hard-coded map.
    const map: Record<string, string[]> = {
      shadow: ['None', 'Small', 'Medium', 'Large', 'Extra Large'],
      fontWeight: ['300', '400', '500', '600', '700', '800', '900'],
    };
    return map[detail.propertyKey] ?? [];
  }

  // ── Toolbar actions ────────────────────────────────────────────────────────

  protected saveChanges(): void {
    const theme = this.selectedTheme();
    const element = this.selectedElement();
    if (!theme || !element) { return; }

    this.saving.set(true);
    const payload = element.details.map(d => ({
      id: d.id,
      propertyValue: d.editValue ?? null,
      unit: d.editUnit ?? null
    }));

    this.service.saveDetails(theme.id, element.id, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.snackBar.open('Changes saved.', undefined, { duration: SNACK_MS });
        // Re-apply CSS vars so the change is visible immediately if this is the user's active theme.
        this.themeApplyService.load().subscribe();
      },
      error: () => {
        this.saving.set(false);
        this.snackBar.open('Save failed. Please try again.', undefined, { duration: SNACK_MS });
      }
    });
  }

  protected addTheme(): void {
    const code = `Theme${Date.now()}`;
    this.service.addTheme(code, 'New theme', false).subscribe(res => {
      this.loadThemes();
      this.selectTheme(res.id);
      this.snackBar.open('Theme created.', undefined, { duration: SNACK_MS });
    });
  }

  protected duplicateTheme(): void {
    const src = this.selectedTheme();
    if (!src) { return; }
    const code = `${src.themeCode}_copy`;
    this.service.addTheme(code, `Copy of ${src.description}`, false).subscribe(res => {
      this.loadThemes();
      this.selectTheme(res.id);
      this.snackBar.open('Theme duplicated.', undefined, { duration: SNACK_MS });
    });
  }

  protected deleteTheme(): void {
    const theme = this.selectedTheme();
    if (!theme) { return; }

    const data: ConfirmDialogData = {
      title: 'Delete Theme',
      message: `Delete "${theme.description}"? This cannot be undone.`,
      confirmLabel: 'Delete'
    };

    this.dialog.open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed().subscribe(confirmed => {
        if (!confirmed) { return; }
        this.service.deleteTheme(theme.id).subscribe(() => {
          this.selectedTheme.set(null);
          this.loadThemes();
          this.snackBar.open('Theme deleted.', undefined, { duration: SNACK_MS });
        });
      });
  }

  protected resetToDefault(): void {
    const element = this.selectedElement();
    if (!element) { return; }
    for (const d of element.details) {
      d.editValue = d.propertyValue;
      d.editUnit = d.unit;
    }
  }

  protected setPreviewMode(mode: 'desktop' | 'tablet' | 'mobile'): void {
    this.previewMode.set(mode);
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  protected previewStyle(detail: ThemeDetail): string {
    if (detail.propertyType === 'color') {
      return detail.editValue ?? '#cccccc';
    }
    return '';
  }

  protected trackById(_: number, item: { id: number }): number {
    return item.id;
  }
}
