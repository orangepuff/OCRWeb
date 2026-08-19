import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  ConfigCatalogMutationResult,
  ConfigItemFilter,
  ConfigItemRow,
  ConfigItemUpsertRequest,
  ConfigSection,
  ConfigSectionUpsertRequest,
  PagedResult
} from './config-item';

@Injectable({ providedIn: 'root' })
export class ConfigAdminService {
  private readonly http = inject(HttpClient);

  listSections() {
    return this.http.get<ConfigSection[]>('/bff/admin/config/sections');
  }

  addSection(request: ConfigSectionUpsertRequest) {
    return this.http.post<ConfigCatalogMutationResult>('/bff/admin/config/sections', request);
  }

  updateSection(id: number, request: ConfigSectionUpsertRequest) {
    return this.http.put<ConfigCatalogMutationResult>(`/bff/admin/config/sections/${id}`, request);
  }

  deleteSection(id: number) {
    return this.http.delete<ConfigCatalogMutationResult>(`/bff/admin/config/sections/${id}`);
  }

  listConfigs(filter: ConfigItemFilter, sortBy: string | null, sortDescending: boolean, page: number, pageSize: number) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize).set('sortDescending', sortDescending);
    if (filter.sectionId !== undefined) {
      params = params.set('sectionId', filter.sectionId);
    }
    if (filter.configCode) {
      params = params.set('configCode', filter.configCode);
    }
    if (filter.configName) {
      params = params.set('configName', filter.configName);
    }
    if (filter.configType !== undefined) {
      params = params.set('configType', filter.configType);
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    return this.http.get<PagedResult<ConfigItemRow>>('/bff/admin/config/items', { params });
  }

  addConfig(request: ConfigItemUpsertRequest) {
    return this.http.post<ConfigCatalogMutationResult>('/bff/admin/config/items', request);
  }

  updateConfig(id: number, request: ConfigItemUpsertRequest) {
    return this.http.put<ConfigCatalogMutationResult>(`/bff/admin/config/items/${id}`, request);
  }

  deleteConfig(id: number) {
    return this.http.delete<ConfigCatalogMutationResult>(`/bff/admin/config/items/${id}`);
  }
}
