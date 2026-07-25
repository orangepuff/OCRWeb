import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PdfFileListItem } from '../models/pdf-file-list-item';

@Injectable({ providedIn: 'root' })
export class PdfService {
  private readonly http = inject(HttpClient);

  upload(projectId: string, file: File): Observable<HttpEvent<{ id: string }>> {
    const formData = new FormData();
    formData.append('ProjectId', projectId);
    formData.append('File', file);
    return this.http.post<{ id: string }>('/api/pdf-files', formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  list(projectId: string): Observable<PdfFileListItem[]> {
    return this.http.get<PdfFileListItem[]>('/api/pdf-files', { params: { projectId } });
  }

  contentUrl(id: string): string {
    return `/api/pdf-files/${id}/content`;
  }
}
