import { DatePipe } from '@angular/common';
import { HttpEventType } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Button, ConfirmDialog, FileInput, Menu, MenuItem, TextInput } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../i18n/i18n.service';
import { PdfFileListItem } from '../../models/pdf-file-list-item';
import { ProjectListItem } from '../../models/project-list-item';
import { PdfService } from '../../services/pdf.service';
import { ProjectService } from '../../services/project.service';

@Component({
  selector: 'app-project-list',
  imports: [ReactiveFormsModule, DatePipe, Button, TextInput, FileInput, Menu],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly pdfService = inject(PdfService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly projects = signal<ProjectListItem[]>([]);
  protected readonly loaded = signal(false);
  protected readonly errorText = signal<string | null>(null);
  protected readonly pdfFilesByProject = signal<Record<string, PdfFileListItem[]>>({});

  protected readonly editingId = signal<string | null>(null);
  protected readonly editControl = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  protected readonly editSelectedFile = signal<File | null>(null);
  protected readonly fileBusy = signal(false);

  ngOnInit(): void {
    this.loadProjects();
  }

  protected addProject(): void {
    this.router.navigate(['/projects/add']);
  }

  protected pdfFilesFor(projectId: string): PdfFileListItem[] {
    return this.pdfFilesByProject()[projectId] ?? [];
  }

  protected pdfContentUrl(id: string): string {
    return this.pdfService.contentUrl(id);
  }

  protected rowMenuItems(): MenuItem[] {
    return [
      { id: 'edit', label: this.i18n.labels().common.edit },
      { id: 'delete', label: this.i18n.labels().common.delete }
    ];
  }

  protected onRowMenuAction(itemId: string, project: ProjectListItem): void {
    if (itemId === 'edit') {
      this.startEdit(project);
    } else if (itemId === 'delete') {
      this.confirmDelete(project);
    }
  }

  protected startEdit(project: ProjectListItem): void {
    this.editingId.set(project.id);
    this.editControl.setValue(project.name);
    this.editSelectedFile.set(null);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
    this.editSelectedFile.set(null);
  }

  protected onEditFileSelected(file: File | null): void {
    this.editSelectedFile.set(file);
  }

  protected uploadEditFile(project: ProjectListItem): void {
    const file = this.editSelectedFile();
    if (!file) {
      return;
    }

    this.fileBusy.set(true);
    this.pdfService.upload(project.id, file).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.Response) {
          this.fileBusy.set(false);
          this.editSelectedFile.set(null);
          this.refreshPdfFiles(project.id);
          this.snackBar.open(this.i18n.messages().project.fileUploadSuccess, undefined, { duration: 3000 });
        }
      },
      error: () => {
        this.fileBusy.set(false);
        this.errorText.set(this.i18n.messages().project.fileUploadError);
      }
    });
  }

  protected confirmDeleteFile(file: PdfFileListItem): void {
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
        this.deleteFile(file);
      }
    });
  }

  private deleteFile(file: PdfFileListItem): void {
    this.fileBusy.set(true);
    this.pdfService.delete(file.id).subscribe({
      next: () => {
        this.fileBusy.set(false);
        this.refreshPdfFiles(file.projectId);
        this.snackBar.open(this.i18n.messages().project.fileDeleteSuccess, undefined, { duration: 3000 });
      },
      error: () => {
        this.fileBusy.set(false);
        this.errorText.set(this.i18n.messages().project.fileDeleteError);
      }
    });
  }

  private refreshPdfFiles(projectId: string): void {
    this.pdfService.list(projectId).subscribe((files) => {
      this.pdfFilesByProject.update((byProject) => ({ ...byProject, [projectId]: files }));
    });
  }

  protected saveEdit(project: ProjectListItem): void {
    if (this.editControl.invalid) {
      return;
    }

    this.projectService.update(project.id, this.editControl.value).subscribe({
      next: (updated) => {
        this.projects.update((list) => list.map((p) => (p.id === updated.id ? updated : p)));
        this.editingId.set(null);
        this.snackBar.open(this.i18n.messages().project.updateSuccess, undefined, { duration: 3000 });
      },
      error: () => this.errorText.set(this.i18n.messages().project.updateError)
    });
  }

  protected confirmDelete(project: ProjectListItem): void {
    const ref = this.dialog.open(ConfirmDialog, {
      data: {
        title: this.i18n.labels().project.deleteConfirmTitle,
        message: this.i18n.labels().project.deleteConfirmMessage,
        confirmLabel: this.i18n.labels().common.delete,
        cancelLabel: this.i18n.labels().common.cancel
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.deleteProject(project);
      }
    });
  }

  private deleteProject(project: ProjectListItem): void {
    this.projectService.delete(project.id).subscribe({
      next: () => {
        this.projects.update((list) => list.filter((p) => p.id !== project.id));
        this.snackBar.open(this.i18n.messages().project.deleteSuccess, undefined, { duration: 3000 });
      },
      error: () => this.errorText.set(this.i18n.messages().project.deleteError)
    });
  }

  private loadProjects(): void {
    this.projectService.list().subscribe((projects) => {
      this.projects.set(projects);
      this.loaded.set(true);

      for (const project of projects) {
        this.pdfService.list(project.id).subscribe((files) => {
          this.pdfFilesByProject.update((byProject) => ({ ...byProject, [project.id]: files }));
        });
      }
    });
  }
}
