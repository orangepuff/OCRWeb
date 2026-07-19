export interface EffectivePermission {
  code: string;
  description: string;
  ruleType: 'Boolean' | 'Integer' | 'Decimal';
  allowed: number | null;
  allowedDecimal: number | null;
}
