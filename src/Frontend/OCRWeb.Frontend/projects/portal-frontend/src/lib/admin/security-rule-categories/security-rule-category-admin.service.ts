import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  AddSecurityRuleCategoryRequest,
  SecurityRuleCategory,
  SecurityRuleCategoryMutationResult,
  UpdateSecurityRuleCategoryRequest
} from './security-rule-category';

@Injectable({ providedIn: 'root' })
export class SecurityRuleCategoryAdminService {
  private readonly http = inject(HttpClient);

  list() {
    return this.http.get<SecurityRuleCategory[]>('/bff/admin/security-rule-categories');
  }

  add(request: AddSecurityRuleCategoryRequest) {
    return this.http.post<SecurityRuleCategoryMutationResult & { categoryId: number | null }>('/bff/admin/security-rule-categories', request);
  }

  update(id: number, request: UpdateSecurityRuleCategoryRequest) {
    return this.http.put<SecurityRuleCategoryMutationResult>(`/bff/admin/security-rule-categories/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<SecurityRuleCategoryMutationResult>(`/bff/admin/security-rule-categories/${id}`);
  }
}
