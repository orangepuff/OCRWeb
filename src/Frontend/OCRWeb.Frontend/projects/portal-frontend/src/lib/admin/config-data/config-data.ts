export interface ConfigDataRow {
  id: number;
  key: string;
  value: string | null;
  allowEditByScreen: boolean | null;
  description: string | null;
}

export interface ConfigDataUpsertRequest {
  sKey: string;
  sValue: string | null;
  bAllowEditByScreen: boolean | null;
  sDescription: string | null;
}

export interface ConfigDataMutationResult {
  success: boolean;
  id: number | null;
  rejectionReason: string | null;
  successMessage: string | null;
}
