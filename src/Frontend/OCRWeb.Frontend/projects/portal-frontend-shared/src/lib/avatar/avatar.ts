import { Component, computed, effect, input, signal } from '@angular/core';

/**
 * Shows a user's uploaded avatar (GET /bff/users/{id}/avatar, same-origin) with an
 * initials fallback when there isn't one. Reusable anywhere a user is referenced —
 * the header, an admin user list, a "created by" column, etc.
 */
@Component({
  selector: 'lib-avatar',
  imports: [],
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss'
})
export class Avatar {
  readonly userId = input.required<string>();
  readonly displayName = input<string | null>(null);
  readonly size = input(32);
  /** Bump this after a successful upload so the browser doesn't serve a stale cached image. */
  readonly version = input(0);

  protected readonly imageFailed = signal(false);

  protected readonly avatarUrl = computed(() => `/bff/users/${this.userId()}/avatar?v=${this.version()}`);

  constructor() {
    effect(() => {
      this.version();
      this.imageFailed.set(false);
    });
  }

  protected readonly initials = computed(() => {
    const name = this.displayName()?.trim();
    if (!name) {
      return '?';
    }

    const parts = name.split(/\s+/);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  });

  protected onImageError(): void {
    this.imageFailed.set(true);
  }
}
