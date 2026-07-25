import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProjectListItem } from '../models/project-list-item';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly http = inject(HttpClient);

  list(): Observable<ProjectListItem[]> {
    return this.http.get<ProjectListItem[]>('/projects');
  }

  create(name: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>('/projects', { name });
  }
}
