import { HttpEventType } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Button, ConfirmDialog, FileInput, TextInput } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../i18n/i18n.service';
import { PdfFileListItem } from '../../models/pdf-file-list-item';
import { PdfService } from '../../services/pdf.service';
import { ProjectService } from '../../services/project.service';

/**
 * Reused for both "Add Project" (/projects/add) and "Edit Project" (/projects/:id/edit) —
 * the only difference is whether projectId() is set, which drives labels, whether a PDF
 * file is required, and whether submit calls create or update.
 */
@Component({
  selector: 'app-project-form',
  imports: [
    ReactiveFormsModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    Button,
    TextInput,
    FileInput
  ],
  templateUrl: './project-form.html',
  styleUrl: './project-form.scss'
})
export class ProjectForm implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly pdfService = inject(PdfService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly projectId = signal<number | null>(null);
  protected readonly isEditMode = computed(() => this.projectId() !== null);
  protected readonly loaded = signal(false);

  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });
  protected readonly existingFile = signal<PdfFileListItem | null>(null);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly fileTouched = signal(false);
  protected readonly fileBusy = signal(false);

  protected readonly submitting = signal(false);
  protected readonly uploadProgress = signal<number | null>(null);
  protected readonly errorText = signal<string | null>(null);

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) {
      this.loaded.set(true);
      return;
    }

    const id = Number(idParam);
    this.projectId.set(id);
    this.projectService.getById(id).subscribe({
      next: (project) => {
        this.form.controls.name.setValue(project.name);
        this.loaded.set(true);
      },
      error: () => {
        this.errorText.set(this.i18n.messages().project.loadError);
        this.loaded.set(true);
      }
    });
    this.pdfService.list(id).subscribe((files) => {
      this.existingFile.set(files[0] ?? null);
    });
  }

  protected back(): void {
    this.router.navigate(['/home']);
  }

  protected pdfContentUrl(id: number): string {
    return this.pdfService.contentUrl(id);
  }

  protected onFileSelected(file: File | null): void {
    this.selectedFile.set(file);
    this.fileTouched.set(true);
  }

  protected canSubmit(): boolean {
    return this.form.valid && !this.submitting() && (this.isEditMode() || !!this.selectedFile());
  }

  protected submitLabel(): string {
    if (this.submitting()) {
      return this.isEditMode() ? this.i18n.labels().project.savingButton : this.i18n.labels().project.creatingButton;
    }
    return this.isEditMode() ? this.i18n.labels().project.saveButton : this.i18n.labels().project.createButton;
  }

  protected confirmDeleteExistingFile(): void {
    const file = this.existingFile();
    if (!file) {
      return;
    }

    const ref = this.dialog.open(ConfirmDialog, {
      data: {
        title: this.i18n.labels().project.deleteFileConfirmTitle,
        message: this.i18n.labels().project.deleteFileConfirmMessage,
        confirmLabel: this.i18n.labels().common.delete,
        cancelLabel: this.i18n.labels().common.cancel
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.deleteExistingFile(file);
      }
    });
  }

  private deleteExistingFile(file: PdfFileListItem): void {
    this.fileBusy.set(true);
    this.pdfService.delete(file.id).subscribe({
      next: () => {
        this.fileBusy.set(false);
        this.existingFile.set(null);
        this.snackBar.open(this.i18n.messages().project.fileDeleteSuccess, undefined, { duration: 3000 });
      },
      error: () => {
        this.fileBusy.set(false);
        this.errorText.set(this.i18n.messages().project.fileDeleteError);
      }
    });
  }

  protected submit(): void {
    this.fileTouched.set(true);
    this.form.markAllAsTouched();

    if (!this.canSubmit()) {
      return;
    }

    this.submitting.set(true);
    this.uploadProgress.set(null);
    this.errorText.set(null);
    this.form.disable();

    const id = this.projectId();
    if (id !== null) {
      this.saveEdit(id);
    } else {
      this.saveCreate();
    }
  }

  private saveCreate(): void {
    const file = this.selectedFile()!;
    this.projectService.create(this.form.controls.name.value).subscribe({
      next: ({ id }) => this.uploadFile(id, file, this.i18n.messages().project.createSuccess),
      error: () => this.finishError(this.i18n.messages().project.createError)
    });
  }

  private saveEdit(id: number): void {
    this.projectService.update(id, this.form.controls.name.value).subscribe({
      next: () => {
        const file = this.selectedFile();
        if (file) {
          this.uploadFile(id, file, this.i18n.messages().project.updateSuccess);
        } else {
          this.finishSuccess(this.i18n.messages().project.updateSuccess);
        }
      },
      error: () => this.finishError(this.i18n.messages().project.updateError)
    });
  }

  private uploadFile(projectId: number, file: File, successMessage: string): void {
    this.pdfService.upload(projectId, file).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress.set(Math.round((100 * event.loaded) / event.total));
        } else if (event.type === HttpEventType.Response) {
          this.finishSuccess(successMessage);
        }
      },
      error: () => this.finishError(this.i18n.messages().project.uploadError)
    });
  }

  private finishSuccess(message: string): void {
    this.snackBar.open(message, undefined, { duration: 3000 });
    this.router.navigate(['/home']);
  }

  private finishError(message: string): void {
    this.submitting.set(false);
    this.uploadProgress.set(null);
    this.form.enable();
    this.errorText.set(message);
  }
}
