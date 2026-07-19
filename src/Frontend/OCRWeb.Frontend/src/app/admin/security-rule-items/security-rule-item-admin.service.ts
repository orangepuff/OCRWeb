import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  AddSecurityRuleItemRequest,
  SecurityRuleItem,
  SecurityRuleItemMutationResult,
  UpdateSecurityRuleItemRequest
} from './security-rule-item';

@Injectable({ providedIn: 'root' })
export class SecurityRuleItemAdminService {
  private readonly http = inject(HttpClient);

  list(categoryId?: number) {
    const params = categoryId !== undefined ? new HttpParams().set('categoryId', categoryId) : undefined;
    return this.http.get<SecurityRuleItem[]>('/bff/admin/security-rule-items', { params });
  }

  add(request: AddSecurityRuleItemRequest) {
    return this.http.post<SecurityRuleItemMutationResult & { ruleItemId: number | null }>('/bff/admin/security-rule-items', request);
  }

  update(id: number, request: UpdateSecurityRuleItemRequest) {
    return this.http.put<SecurityRuleItemMutationResult>(`/bff/admin/security-rule-items/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<SecurityRuleItemMutationResult>(`/bff/admin/security-rule-items/${id}`);
  }
}
