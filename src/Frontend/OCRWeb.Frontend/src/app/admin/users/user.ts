export interface User {
  id: number;
  username: string;
  email: string | null;
  displayName: string | null;
  isActive: boolean;
  isTemplateUser: boolean;
  parentId: number | null;
}

export interface AddUserRequest {
  username: string;
  email: string | null;
  displayName: string | null;
  templateUserId: number | null;
}

export interface UpdateUserRequest {
  email: string | null;
  displayName: string | null;
  isTemplateUser: boolean;
  parentId: number | null;
}

export interface UserMutationResult {
  success: boolean;
  rejectionReason: string | null;
}
