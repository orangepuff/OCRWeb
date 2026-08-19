export type RuleTypeName = 'Boolean' | 'Integer' | 'Decimal';

export const RULE_TYPES: { value: number; name: RuleTypeName; textCode: string }[] = [
  { value: 0, name: 'Boolean', textCode: 'admin.securityRuleItems.ruleType.boolean' },
  { value: 1, name: 'Integer', textCode: 'admin.securityRuleItems.ruleType.integer' },
  { value: 2, name: 'Decimal', textCode: 'admin.securityRuleItems.ruleType.decimal' }
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
  successMessage: string | null;
}
