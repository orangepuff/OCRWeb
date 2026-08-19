import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SaveDetailRequest, ThemeFull, ThemeItem } from './theme-models';

@Injectable({ providedIn: 'root' })
export class ThemeAdminService {
  private readonly http = inject(HttpClient);

  listThemes() {
    return this.http.get<ThemeItem[]>('/bff/admin/themes');
  }

  getTheme(id: number) {
    return this.http.get<ThemeFull>(`/bff/admin/themes/${id}`);
  }

  addTheme(themeCode: string, description: string, isActive: boolean) {
    return this.http.post<{ id: number }>('/bff/admin/themes', { themeCode, description, isActive });
  }

  updateTheme(id: number, themeCode: string, description: string, isActive: boolean) {
    return this.http.put(`/bff/admin/themes/${id}`, { themeCode, description, isActive });
  }

  deleteTheme(id: number) {
    return this.http.delete(`/bff/admin/themes/${id}`);
  }

  saveDetails(themeId: number, elementId: number, details: SaveDetailRequest[]) {
    return this.http.put(`/bff/admin/themes/${themeId}/elements/${elementId}/details`, details);
  }
}
