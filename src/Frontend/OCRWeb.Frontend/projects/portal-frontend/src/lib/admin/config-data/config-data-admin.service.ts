import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigDataMutationResult, ConfigDataRow, ConfigDataUpsertRequest } from './config-data';

@Injectable({ providedIn: 'root' })
export class ConfigDataAdminService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<ConfigDataRow[]>('/bff/admin/config-data');
  }

  create(request: ConfigDataUpsertRequest) {
    return this.http.post<ConfigDataMutationResult>('/bff/admin/config-data', request);
  }

  update(id: number, request: ConfigDataUpsertRequest) {
    return this.http.put<ConfigDataMutationResult>(`/bff/admin/config-data/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<ConfigDataMutationResult>(`/bff/admin/config-data/${id}`);
  }
}
