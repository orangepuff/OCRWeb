import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

/** Mirrors OrangepuffPortal.Config.Contract.ConfigValueType. */
export enum ConfigValueType {
  String = 0,
  Int = 1,
  Decimal = 2,
  Boolean = 3
}

/** Mirrors OrangepuffPortal.Config.Contract.ConfigValueInput — exactly one field set, matching the target config's type. */
export interface ConfigValueInput {
  sConfigValue?: string | null;
  iConfigValue?: number | null;
  nConfigValue?: number | null;
  btConfigValue?: boolean | null;
}

/** Mirrors OrangepuffPortal.Config.Contract.UserConfigItemDto. */
export interface UserConfigItem {
  sConfigCode: string;
  sConfigName: string;
  sTextCode: string;
  configType: ConfigValueType;
  btAllowUserEdit: boolean;
  iSortOrder: number | null;
  sConfigValue: string | null;
  iConfigValue: number | null;
  nConfigValue: number | null;
  btConfigValue: boolean | null;
}

/** Mirrors OrangepuffPortal.Config.Contract.UserConfigSectionDto. */
export interface UserConfigSection {
  sSectionDesc: string;
  sTextCode: string;
  iSortOrder: number | null;
  configs: UserConfigItem[];
}

@Injectable({ providedIn: 'root' })
export class ConfigSettingsService {
  private readonly http = inject(HttpClient);

  getSections(userId: string) {
    return this.http.get<UserConfigSection[]>(`/bff/users/${userId}/config`);
  }

  setValue(userId: string, configCode: string, value: ConfigValueInput) {
    return this.http.put<void>(`/bff/admin/users/${userId}/config/${configCode}`, { value });
  }
}
