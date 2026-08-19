export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface ConfigTextRow {
  id: number;
  sModule: string;
  sTextCode: string;
  sCultureCode: string;
  sTextType: string;
  sText: string;
  sNote: string | null;
  dtInsertedTime: string | null;
  dtUpdatedTime: string | null;
}

export interface ConfigTextUpsertRequest {
  sModule: string;
  sTextCode: string;
  sCultureCode: string;
  sTextType: string;
  sText: string;
  sNote: string | null;
}

export interface ConfigTextFilter {
  module?: string;
  textCode?: string;
  cultureCode?: string;
  textType?: string;
  text?: string;
}

export interface ConfigTextMutationResult {
  success: boolean;
  id: number | null;
  rejectionReason: string | null;
  successMessage: string | null;
}
