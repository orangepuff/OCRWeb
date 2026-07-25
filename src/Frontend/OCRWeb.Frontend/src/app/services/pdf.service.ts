import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PdfService {
  private readonly http = inject(HttpClient);

  upload(projectId: string, file: File): Observable<{ id: string }> {
    const formData = new FormData();
    formData.append('ProjectId', projectId);
    formData.append('File', file);
    return this.http.post<{ id: string }>('/api/pdf-files', formData);
  }
}
