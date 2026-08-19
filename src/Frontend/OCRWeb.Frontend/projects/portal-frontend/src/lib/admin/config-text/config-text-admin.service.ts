import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ConfigTextFilter, ConfigTextMutationResult, ConfigTextRow, ConfigTextUpsertRequest, PagedResult } from './config-text';

@Injectable({ providedIn: 'root' })
export class ConfigTextAdminService {
  private readonly http = inject(HttpClient);

  list(filter: ConfigTextFilter, page: number, pageSize: number) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filter.module) {
      params = params.set('module', filter.module);
    }
    if (filter.textCode) {
      params = params.set('textCode', filter.textCode);
    }
    if (filter.cultureCode) {
      params = params.set('cultureCode', filter.cultureCode);
    }
    if (filter.textType) {
      params = params.set('textType', filter.textType);
    }
    if (filter.text) {
      params = params.set('text', filter.text);
    }

    return this.http.get<PagedResult<ConfigTextRow>>('/bff/admin/config-text', { params });
  }

  add(request: ConfigTextUpsertRequest) {
    return this.http.post<ConfigTextMutationResult>('/bff/admin/config-text', request);
  }

  update(id: number, request: ConfigTextUpsertRequest) {
    return this.http.put<ConfigTextMutationResult>(`/bff/admin/config-text/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<ConfigTextMutationResult>(`/bff/admin/config-text/${id}`);
  }
}
