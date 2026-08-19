import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { ConfigAdminService } from '../config-admin.service';
import { CONFIG_VALUE_TYPES, ConfigItemFilter, ConfigItemRow, ConfigSection } from '../config-item';
import { ConfigSectionFormDialog, ConfigSectionFormDialogData, ConfigSectionFormDialogResult } from '../config-section-form-dialog/config-section-form-dialog';
import { ConfigItemFormDialog, ConfigItemFormDialogData, ConfigItemFormDialogResult } from '../config-item-form-dialog/config-item-form-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-config-admin-page',
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSortModule,
    MatTabsModule,
    TranslatePipe
  ],
  templateUrl: './config-admin-page.html',
  styleUrl: './config-admin-page.scss'
})
export class ConfigAdminPage implements OnInit, AfterViewInit {
  private readonly configAdminService = inject(ConfigAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  @ViewChild('sectionSort') private sectionSort!: MatSort;

  protected readonly module = MODULE;

  protected readonly sections = signal<ConfigSection[]>([]);
  // Sections are unpaged (small, hand-curated list) — sorted entirely client-side via
  // MatTableDataSource, unlike the Configs table below which sorts server-side.
  protected readonly sectionsDataSource = new MatTableDataSource<ConfigSection>([]);
  protected readonly sectionColumns = ['sSectionDesc', 'sTextCode', 'btShow', 'actions'];

  protected readonly items = signal<ConfigItemRow[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly itemsLoading = signal(false);
  protected readonly itemColumns = [
    'sSectionDesc',
    'sConfigCode',
    'sConfigName',
    'iConfigType',
    'btShow',
    'btAllowUserEdit',
    'iSortOrder',
    'defaultValue',
    'actions'
  ];
  protected readonly pageSizeOptions = [50, 100, 200, 500];
  protected readonly configValueTypes = CONFIG_VALUE_TYPES;

  private page = 1;
  private pageSize = 50;
  private appliedFilter: ConfigItemFilter = {};
  private sortBy: string | null = null;
  private sortDescending = false;
  // Bumped on every reloadItems() call so a late-arriving response from a superseded request
  // (e.g. the user clicks a sort header twice in quick succession) can be told apart from the
  // response to the most recent request and dropped instead of overwriting it out of order.
  private itemsRequestId = 0;

  protected readonly filterForm = new FormGroup({
    sectionId: new FormControl<number | null>(null),
    configCode: new FormControl(''),
    configName: new FormControl('')
  });

  ngOnInit(): void {
    this.reloadSections();
    this.reloadItems();
  }

  ngAfterViewInit(): void {
    this.sectionsDataSource.sort = this.sectionSort;
  }

  protected configTypeName(item: ConfigItemRow): string {
    const type = this.configValueTypes.find((t) => t.value === item.iConfigType);
    return type ? this.translationService.get(type.textCode, MODULE) : `#${item.iConfigType}`;
  }

  protected defaultValueDisplay(item: ConfigItemRow): string {
    switch (item.iConfigType) {
      case 0:
        return item.sDefaultValue ?? '—';
      case 1:
        return item.iDefaultValue !== null ? String(item.iDefaultValue) : '—';
      case 2:
        return item.nDefaultValue !== null ? String(item.nDefaultValue) : '—';
      case 3:
        return item.btDefaultValue !== null
          ? this.translationService.get(item.btDefaultValue ? 'yes' : 'no', 'OrangepuffPortal.Common')
          : '—';
      default:
        return '—';
    }
  }

  protected onSortChange(sort: Sort): void {
    this.sortBy = sort.direction ? sort.active : null;
    this.sortDescending = sort.direction === 'desc';
    this.page = 1;
    this.reloadItems();
  }

  private reloadSections(): void {
    this.configAdminService.listSections().subscribe((sections) => {
      this.sections.set(sections);
      this.sectionsDataSource.data = sections;
    });
  }

  private reloadItems(): void {
    const requestId = ++this.itemsRequestId;
    this.itemsLoading.set(true);

    this.configAdminService.listConfigs(this.appliedFilter, this.sortBy, this.sortDescending, this.page, this.pageSize).subscribe({
      next: (result) => {
        if (requestId !== this.itemsRequestId) {
          return; // a newer request has since been issued — this response is stale, drop it
        }

        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.itemsLoading.set(false);
      },
      error: () => {
        if (requestId === this.itemsRequestId) {
          this.itemsLoading.set(false);
        }
      }
    });
  }

  protected applyFilter(): void {
    const value = this.filterForm.getRawValue();
    this.appliedFilter = {
      sectionId: value.sectionId ?? undefined,
      configCode: value.configCode || undefined,
      configName: value.configName || undefined
    };
    this.page = 1;
    this.reloadItems();
  }

  protected clearFilter(): void {
    this.filterForm.reset();
    this.appliedFilter = {};
    this.page = 1;
    this.reloadItems();
  }

  protected onPage(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.reloadItems();
  }

  protected openAddSectionDialog(): void {
    const data: ConfigSectionFormDialogData = { section: null };

    this.dialog
      .open<ConfigSectionFormDialog, ConfigSectionFormDialogData, ConfigSectionFormDialogResult>(ConfigSectionFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configAdminService.addSection(result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadSections();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.section.msg.addFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }

  protected openEditSectionDialog(section: ConfigSection): void {
    const data: ConfigSectionFormDialogData = { section };

    this.dialog
      .open<ConfigSectionFormDialog, ConfigSectionFormDialogData, ConfigSectionFormDialogResult>(ConfigSectionFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configAdminService.updateSection(section.id, result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadSections();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.section.msg.updateFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }

  protected deleteSection(section: ConfigSection): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.config.section.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.config.section.confirmDelete.message', MODULE, section.sSectionDesc),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.configAdminService.deleteSection(section.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadSections();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.section.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }

  protected openAddConfigDialog(): void {
    const data: ConfigItemFormDialogData = { item: null, sections: this.sections() };

    this.dialog
      .open<ConfigItemFormDialog, ConfigItemFormDialogData, ConfigItemFormDialogResult>(ConfigItemFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configAdminService.addConfig(result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadItems();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.item.msg.addFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }

  protected openEditConfigDialog(item: ConfigItemRow): void {
    const data: ConfigItemFormDialogData = { item, sections: this.sections() };

    this.dialog
      .open<ConfigItemFormDialog, ConfigItemFormDialogData, ConfigItemFormDialogResult>(ConfigItemFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configAdminService.updateConfig(item.id, result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadItems();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.item.msg.updateFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }

  protected deleteConfig(item: ConfigItemRow): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.config.item.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.config.item.confirmDelete.message', MODULE, item.sConfigCode),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.configAdminService.deleteConfig(item.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.translationService.get('dismiss', 'OrangepuffPortal.Common'));
            this.reloadItems();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.config.item.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`,
              this.translationService.get('dismiss', 'OrangepuffPortal.Common')
            );
          }
        });
      });
  }
}
