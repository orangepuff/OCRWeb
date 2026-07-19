export interface SecurityRuleCategory {
  id: number;
  categoryDesc: string;
  textCode: string | null;
  hidden: boolean;
}

export interface AddSecurityRuleCategoryRequest {
  categoryDesc: string;
  textCode: string | null;
}

export interface UpdateSecurityRuleCategoryRequest {
  categoryDesc: string;
  textCode: string | null;
  hidden: boolean;
}

export interface SecurityRuleCategoryMutationResult {
  success: boolean;
  rejectionReason: string | null;
}
