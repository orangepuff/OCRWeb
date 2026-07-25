import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProjectListItem } from '../models/project-list-item';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly http = inject(HttpClient);

  list(): Observable<ProjectListItem[]> {
    return this.http.get<ProjectListItem[]>('/api/projects');
  }

  getById(id: string): Observable<ProjectListItem> {
    return this.http.get<ProjectListItem>(`/api/projects/${id}`);
  }

  create(name: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>('/api/projects', { name });
  }

  update(id: string, name: string): Observable<ProjectListItem> {
    return this.http.put<ProjectListItem>(`/api/projects/${id}`, { name });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}`);
  }
}
