import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CropPdfRequest } from '../models/crop-pdf-request';
import { PdfFileListItem } from '../models/pdf-file-list-item';

@Injectable({ providedIn: 'root' })
export class PdfService {
  private readonly http = inject(HttpClient);

  upload(projectId: number, file: File): Observable<HttpEvent<{ id: number }>> {
    const formData = new FormData();
    formData.append('ProjectId', String(projectId));
    formData.append('File', file);
    return this.http.post<{ id: number }>('/api/pdf-files', formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  list(projectId: number): Observable<PdfFileListItem[]> {
    return this.http.get<PdfFileListItem[]>('/api/pdf-files', { params: { projectId } });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/pdf-files/${id}`);
  }

  crop(id: number, request: CropPdfRequest): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(`/api/pdf-files/${id}/crop`, request);
  }

  contentUrl(id: number): string {
    return `/api/pdf-files/${id}/content`;
  }
}
