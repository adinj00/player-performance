export interface SessionUser {
  id: string;
  email: string;
  accountStatus: string;
  mustChangePassword: boolean;
}

export interface SessionResponse {
  isAuthenticated: boolean;
  user: SessionUser | null;
}

export interface CsrfResponse {
  csrfTokenHeaderName: string;
  requestToken: string;
}

export interface CsrfToken {
  headerName: string;
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
