import { Component, computed, effect, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute } from '@angular/router';
import { forkJoin, of, switchMap, map } from 'rxjs';
import { FormControl, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Avatar, IdentityService } from '@orangepuff/portal-frontend-shared';
import { UserSettingsService } from './user-settings.service';
import { ConfigSettingsService, ConfigValueInput, ConfigValueType, UserConfigItem, UserConfigSection } from './config-settings.service';
import { UserAdminService } from '../admin/users/user-admin.service';
import { User } from '../admin/users/user';
import { ActiveTheme, ThemeApplyService } from '../theme/theme-apply.service';
import { TranslatePipe } from '../translation/translate.pipe';
import { TranslationService } from '../translation/translation.service';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-settings-page',
  imports: [Avatar, MatButtonModule, MatCardModule, MatCheckboxModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatSelectModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss'
})
export class SettingsPage {
  protected readonly ConfigValueType = ConfigValueType;
  protected readonly module = MODULE;

  private readonly route = inject(ActivatedRoute);
  protected readonly identityService = inject(IdentityService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly configSettingsService = inject(ConfigSettingsService);
  private readonly userAdminService = inject(UserAdminService);
  private readonly themeApplyService = inject(ThemeApplyService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly avatarVersion = signal(0);
  protected readonly availableThemes = signal<ActiveTheme[]>([]);
  protected readonly selectedThemeId = computed(() => this.themeApplyService.currentTheme()?.id ?? 0);
  protected readonly savingTheme = signal(false);

  /** Reactive to param changes (not just `.snapshot`) since the guard lets the router reuse this component across /Users/:userId/Settings navigations. */
  protected readonly targetUserId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('userId') ?? '')),
    { initialValue: this.route.snapshot.paramMap.get('userId') ?? '' }
  );

  /** The profile (display name/password) forms only exist for the signed-in user's own page — there's no self-service endpoint for an admin to change someone else's. */
  protected readonly isOwnSettings = computed(() => this.targetUserId() === this.identityService.currentUser()?.userId);

  /** Config values are editable only when the *viewer* is an admin — a normal user always sees their own values read-only, regardless of Configs.btAllowUserEdit. */
  protected readonly isAdmin = computed(() => this.identityService.currentUser()?.isAdmin ?? false);

  /** Avatar upload/remove: a user can always manage their own, and an admin can also manage anyone else's. */
  protected readonly canEditAvatar = computed(() => this.isOwnSettings() || this.isAdmin());

  protected readonly sections = toSignal(
    toObservable(this.targetUserId).pipe(switchMap((userId) => (userId ? this.configSettingsService.getSections(userId) : of([])))),
    { initialValue: [] as UserConfigSection[] }
  );

  private readonly configControls = new Map<string, FormControl>();

  /** The target user's own record and the pool of template users, both admin-only — loaded from the same GET /bff/admin/users the User List uses. */
  protected readonly targetUser = signal<User | null>(null);
  protected readonly templateUsers = signal<User[]>([]);

  protected readonly accountForm = new FormGroup({
    email: new FormControl(''),
    displayName: new FormControl(''),
    isActive: new FormControl(true, { nonNullable: true }),
    isTemplateUser: new FormControl(false, { nonNullable: true }),
    parentId: new FormControl<number | null>(null)
  });

  protected readonly displayNameForm = new FormGroup({
    displayName: new FormControl(this.identityService.currentUser()?.displayName ?? '', { nonNullable: true, validators: [Validators.required] })
  });

  protected readonly passwordForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    newPassword: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  constructor() {
    this.themeApplyService.listActiveThemes().subscribe(t => this.availableThemes.set(t));

    // Rebuild one control per config whenever the loaded sections change (userId navigation, or a reload) — disabled up front for a normal user so they truly cannot submit an edit, not just visually greyed out.
    effect(() => {
      const sections = this.sections();
      const editable = this.isAdmin();

      this.configControls.clear();
      for (const section of sections) {
        for (const item of section.configs) {
          const control = new FormControl<string | number | boolean | null>(this.currentValue(item));
          if (!editable) {
            control.disable();
          }
          this.configControls.set(item.sConfigCode, control);
        }
      }
    });

    // Account section (username/email/display name/active/template) is admin-only, for any target user — reload the same list the User List uses whenever the target user changes.
    effect(() => {
      const userId = this.targetUserId();
      if (!this.isAdmin() || !userId) {
        return;
      }

      this.userAdminService.list().subscribe((users) => {
        this.templateUsers.set(users.filter((u) => u.isTemplateUser));

        const target = users.find((u) => u.id === Number(userId)) ?? null;
        this.targetUser.set(target);

        if (target) {
          this.accountForm.reset({
            email: target.email ?? '',
            displayName: target.displayName ?? '',
            isActive: target.isActive,
            isTemplateUser: target.isTemplateUser,
            parentId: target.parentId
          });
        }
      });
    });
  }

  protected changeTheme(themeId: number): void {
    this.savingTheme.set(true);
    this.themeApplyService.setTheme(themeId).subscribe({
      next: () => {
        this.savingTheme.set(false);
        this.snackBar.open('Theme updated.', this.dismissLabel(), { duration: 3000 });
      },
      error: () => {
        this.savingTheme.set(false);
        this.snackBar.open('Failed to change theme.', this.dismissLabel(), { duration: 3000 });
      }
    });
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected getControl(configCode: string): FormControl {
    return this.configControls.get(configCode)!;
  }

  private currentValue(item: UserConfigItem): string | number | boolean | null {
    switch (item.configType) {
      case ConfigValueType.Int:
        return item.iConfigValue;
      case ConfigValueType.Decimal:
        return item.nConfigValue;
      case ConfigValueType.Boolean:
        return item.btConfigValue ?? false;
      default:
        return item.sConfigValue;
    }
  }

  protected onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    if (!file) {
      return;
    }

    this.updateAvatar(file);
  }

  protected removeAvatar(): void {
    this.updateAvatar(null);
  }

  private updateAvatar(file: File | null): void {
    const request = this.isOwnSettings()
      ? this.userSettingsService.updateAvatar(file)
      : this.userSettingsService.updateUserAvatar(this.targetUserId(), file);

    request.subscribe((result) => {
      if (result.success) {
        this.avatarVersion.update((v) => v + 1);
        this.snackBar.open(result.successMessage!, this.dismissLabel());
      } else {
        this.snackBar.open(`${this.translationService.get('settings.avatar.updateFailed', MODULE)}: ${result.rejectionReason}`, this.dismissLabel());
      }
    });
  }

  protected saveAccount(): void {
    if (this.accountForm.invalid) {
      return;
    }

    const value = this.accountForm.getRawValue();
    const userId = Number(this.targetUserId());

    this.userAdminService
      .update(userId, {
        email: value.email || null,
        displayName: value.displayName || null,
        isActive: value.isActive,
        isTemplateUser: value.isTemplateUser,
        parentId: value.isTemplateUser ? null : value.parentId
      })
      .subscribe((result) => {
        if (result.success) {
          this.snackBar.open(result.successMessage!, this.dismissLabel());
        } else {
          this.snackBar.open(`${this.translationService.get('settings.account.updateFailed', MODULE)}: ${result.rejectionReason}`, this.dismissLabel());
        }
      });
  }

  protected saveDisplayName(): void {
    if (this.displayNameForm.invalid) {
      return;
    }

    const displayName = this.displayNameForm.controls.displayName.value;
    this.userSettingsService.updateDisplayName(displayName).subscribe((result) => {
      if (result.success) {
        this.identityService.checkSession().subscribe();
        this.snackBar.open(result.successMessage!, this.dismissLabel());
      } else {
        this.snackBar.open(`${this.translationService.get('settings.profile.displayNameUpdateFailed', MODULE)}: ${result.rejectionReason}`, this.dismissLabel());
      }
    });
  }

  protected passwordMismatch(): boolean {
    const { newPassword, confirmPassword } = this.passwordForm.controls;
    return confirmPassword.value.length > 0 && newPassword.value !== confirmPassword.value;
  }

  protected changePassword(formDirective: FormGroupDirective): void {
    if (this.passwordForm.invalid || this.passwordMismatch()) {
      return;
    }

    const { currentPassword, newPassword } = this.passwordForm.controls;
    this.userSettingsService.changePassword(currentPassword.value, newPassword.value).subscribe((result) => {
      if (result.success) {
        // FormGroup.reset() alone leaves the directive's `submitted` flag true, so mat-form-field's
        // error state matcher keeps showing the now-empty required fields as invalid — resetForm()
        // clears both the model and that flag together.
        formDirective.resetForm();
        this.snackBar.open(result.successMessage!, this.dismissLabel());
      } else {
        this.snackBar.open(`${this.translationService.get('settings.password.changeFailed', MODULE)}: ${result.rejectionReason}`, this.dismissLabel());
      }
    });
  }

  protected saveSection(section: UserConfigSection): void {
    const userId = this.targetUserId();
    const requests = section.configs.map((item) => {
      const control = this.getControl(item.sConfigCode);
      const value = this.toConfigValueInput(item.configType, control.value);
      return this.configSettingsService.setValue(userId, item.sConfigCode, value);
    });

    if (requests.length === 0) {
      return;
    }

    forkJoin(requests).subscribe({
      next: () => this.snackBar.open(this.translationService.getFormatted('settings.config.saved', MODULE, section.sSectionDesc), this.dismissLabel()),
      error: () => this.snackBar.open(this.translationService.getFormatted('settings.config.saveFailed', MODULE, section.sSectionDesc), this.dismissLabel())
    });
  }

  private toConfigValueInput(configType: ConfigValueType, value: string | number | boolean | null): ConfigValueInput {
    switch (configType) {
      case ConfigValueType.Int:
        return { iConfigValue: value === null || value === '' ? null : Number(value) };
      case ConfigValueType.Decimal:
        return { nConfigValue: value === null || value === '' ? null : Number(value) };
      case ConfigValueType.Boolean:
        return { btConfigValue: Boolean(value) };
      default:
        return { sConfigValue: value === '' ? null : (value as string | null) };
    }
  }
}
