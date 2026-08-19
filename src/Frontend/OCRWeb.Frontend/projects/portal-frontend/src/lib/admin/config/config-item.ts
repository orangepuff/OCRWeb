export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export const CONFIG_VALUE_TYPES: { value: number; textCode: string }[] = [
  { value: 0, textCode: 'admin.config.item.valueType.string' },
  { value: 1, textCode: 'admin.config.item.valueType.integer' },
  { value: 2, textCode: 'admin.config.item.valueType.decimal' },
  { value: 3, textCode: 'admin.config.item.valueType.boolean' }
];

export interface ConfigSection {
  id: number;
  sSectionDesc: string;
  sTextCode: string;
  btShow: boolean;
  iSortOrder: number | null;
}

export interface ConfigSectionUpsertRequest {
  sSectionDesc: string;
  sTextCode: string;
  btShow: boolean;
  iSortOrder: number | null;
}

export interface ConfigItemRow {
  id: number;
  iSectionId: number;
  sSectionDesc: string;
  sConfigCode: string;
  sConfigName: string;
  sTextCode: string;
  iConfigType: number;
  btShow: boolean;
  btAllowUserEdit: boolean;
  iSortOrder: number | null;
  sDefaultValue: string | null;
  iDefaultValue: number | null;
  nDefaultValue: number | null;
  btDefaultValue: boolean | null;
}

export interface ConfigItemUpsertRequest {
  iSectionId: number;
  sConfigCode: string;
  sConfigName: string;
  sTextCode: string;
  iConfigType: number;
  btShow: boolean;
  btAllowUserEdit: boolean;
  iSortOrder: number | null;
  sDefaultValue: string | null;
  iDefaultValue: number | null;
  nDefaultValue: number | null;
  btDefaultValue: boolean | null;
}

export interface ConfigCatalogMutationResult {
  success: boolean;
  id: number | null;
  rejectionReason: string | null;
  successMessage: string | null;
}

export interface ConfigItemFilter {
  sectionId?: number;
  configCode?: string;
  configName?: string;
  configType?: number;
}
