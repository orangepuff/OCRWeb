/** Mirrors the backend's ConfigTextEntryDto — one resolved (module, code, type) text row. */
export interface ConfigTextEntry {
  sModule: string;
  sTextCode: string;
  sTextType: string;
  sText: string;
}
