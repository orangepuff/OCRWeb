import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddUserRequest, UpdateUserRequest, User, UserMutationResult } from './user';

@Injectable({ providedIn: 'root' })
export class UserAdminService {
  private readonly http = inject(HttpClient);

  list() {
    return this.http.get<User[]>('/bff/admin/users');
  }

  add(request: AddUserRequest) {
    return this.http.post<UserMutationResult & { userId: number | null }>('/bff/admin/users', request);
  }

  update(id: number, request: UpdateUserRequest) {
    return this.http.put<UserMutationResult>(`/bff/admin/users/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<UserMutationResult>(`/bff/admin/users/${id}`);
  }
}
