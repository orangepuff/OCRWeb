export type RuleTypeName = 'Boolean' | 'Integer' | 'Decimal';

export const RULE_TYPES: { value: number; name: RuleTypeName }[] = [
  { value: 0, name: 'Boolean' },
  { value: 1, name: 'Integer' },
  { value: 2, name: 'Decimal' }
];

export interface SecurityRuleItem {
  id: number;
  categoryId: number;
  code: string;
  description: string;
  ruleType: RuleTypeName;
  sortOrder: number | null;
  textCode: string | null;
  hidden: boolean;
}

export interface AddSecurityRuleItemRequest {
  categoryId: number;
  code: string;
  description: string;
  ruleType: number;
  textCode: string | null;
  sortOrder: number | null;
}

export interface UpdateSecurityRuleItemRequest {
  categoryId: number;
  description: string;
  ruleType: number;
  textCode: string | null;
  sortOrder: number | null;
  hidden: boolean;
}

export interface SecurityRuleItemMutationResult {
  success: boolean;
  rejectionReason: string | null;
}
