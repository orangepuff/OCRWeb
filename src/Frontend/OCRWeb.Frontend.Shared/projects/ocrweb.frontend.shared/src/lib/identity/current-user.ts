export interface CurrentUser {
  userId: string;
  email: string | null;
  displayName: string | null;
  isAdmin: boolean;
}
