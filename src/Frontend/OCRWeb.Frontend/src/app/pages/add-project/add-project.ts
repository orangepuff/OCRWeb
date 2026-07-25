import { HttpEventType } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Button, TextInput } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../i18n/i18n.service';
import { PdfService } from '../../services/pdf.service';
import { ProjectService } from '../../services/project.service';

@Component({
  selector: 'app-add-project',
  imports: [ReactiveFormsModule, MatProgressSpinnerModule, MatProgressBarModule, Button, TextInput],
  templateUrl: './add-project.html',
  styleUrl: './add-project.scss'
})
export class AddProject {
  private readonly projectService = inject(ProjectService);
  private readonly pdfService = inject(PdfService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly fileTouched = signal(false);
  protected readonly submitting = signal(false);
  protected readonly uploadProgress = signal<number | null>(null);
  protected readonly errorText = signal<string | null>(null);

  protected back(): void {
    this.router.navigate(['/home']);
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
    this.fileTouched.set(true);
  }

  protected submit(): void {
    this.fileTouched.set(true);
    this.form.markAllAsTouched();

    const file = this.selectedFile();
    if (this.form.invalid || !file) {
      return;
    }

    this.submitting.set(true);
    this.uploadProgress.set(null);
    this.errorText.set(null);
    this.form.disable();

    this.projectService.create(this.form.controls.name.value).subscribe({
      next: ({ id }) => {
        this.pdfService.upload(id, file).subscribe({
          next: (event) => {
            if (event.type === HttpEventType.UploadProgress && event.total) {
              this.uploadProgress.set(Math.round((100 * event.loaded) / event.total));
            } else if (event.type === HttpEventType.Response) {
              this.snackBar.open(this.i18n.messages().project.createSuccess, undefined, { duration: 3000 });
              this.router.navigate(['/home']);
            }
          },
          error: () => {
            this.submitting.set(false);
            this.uploadProgress.set(null);
            this.form.enable();
            this.errorText.set(this.i18n.messages().project.uploadError);
          }
        });
      },
      error: () => {
        this.submitting.set(false);
        this.form.enable();
        this.errorText.set(this.i18n.messages().project.createError);
      }
    });
  }
}
