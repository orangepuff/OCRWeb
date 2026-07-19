import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Avatar, IdentityService } from 'ocrweb.frontend.shared';
import { UserSettingsService } from './user-settings.service';

@Component({
  selector: 'app-settings-page',
  imports: [Avatar, MatButtonModule],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss'
})
export class SettingsPage {
  protected readonly identityService = inject(IdentityService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly avatarVersion = signal(0);

  protected onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    if (!file) {
      return;
    }

    this.userSettingsService.updateAvatar(file).subscribe((result) => {
      if (result.success) {
        this.avatarVersion.update((v) => v + 1);
        this.snackBar.open('Avatar updated', 'Dismiss');
      } else {
        this.snackBar.open(`Could not update avatar: ${result.rejectionReason}`, 'Dismiss');
      }
    });
  }

  protected removeAvatar(): void {
    this.userSettingsService.updateAvatar(null).subscribe((result) => {
      if (result.success) {
        this.avatarVersion.update((v) => v + 1);
        this.snackBar.open('Avatar removed', 'Dismiss');
      } else {
        this.snackBar.open(`Could not remove avatar: ${result.rejectionReason}`, 'Dismiss');
      }
    });
  }
}
