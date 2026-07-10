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
}

export interface CsrfToken {
  headerName: string;
  token: string;
}
