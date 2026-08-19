export interface ThemeDetail {
  id: number;
  propertyKey: string;
  propertyLabel: string;
  propertyDescription: string;
  propertyType: 'color' | 'number_unit' | 'dropdown' | 'text';
  propertyValue: string | null;
  unit: string | null;
  sortOrder: number;
  // local edit state
  editValue?: string | null;
  editUnit?: string | null;
}

export interface ThemeElement {
  id: number;
  elementCode: string;
  description: string;
  sortOrder: number;
  details: ThemeDetail[];
}

export interface ThemeSection {
  id: number;
  sectionCode: string;
  description: string;
  sortOrder: number;
  elements: ThemeElement[];
}

export interface ThemeItem {
  id: number;
  themeCode: string;
  description: string;
  isActive: boolean;
}

export interface ThemeFull extends ThemeItem {
  sections: ThemeSection[];
}

export interface SaveDetailRequest {
  id: number;
  propertyValue: string | null;
  unit: string | null;
}
