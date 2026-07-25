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

  getById(id: number): Observable<ProjectListItem> {
    return this.http.get<ProjectListItem>(`/api/projects/${id}`);
  }

  create(name: string): Observable<{ id: number }> {
    return this.http.post<{ id: number }>('/api/projects', { name });
  }

  update(id: number, name: string): Observable<ProjectListItem> {
    return this.http.put<ProjectListItem>(`/api/projects/${id}`, { name });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}`);
  }
}
