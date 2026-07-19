import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

interface AvatarMutationResult {
  success: boolean;
  rejectionReason: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private readonly http = inject(HttpClient);

  updateAvatar(file: File | null) {
    const form = new FormData();
    if (file) {
      form.append('file', file);
    }
    return this.http.put<AvatarMutationResult>('/bff/me/avatar', form);
  }
}
