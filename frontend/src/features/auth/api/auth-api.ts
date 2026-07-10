import { apiRequest } from "@/lib/api/api-client";

import type {
  ChangePasswordRequest,
  CsrfResponse,
  CsrfToken,
  LoginRequest,
  SessionResponse,
} from "../types/session";

export function getSession() {
  return apiRequest<SessionResponse>("/api/auth/session", {
    method: "GET",
  });
}

export async function getCsrf(): Promise<CsrfToken> {
  const response = await apiRequest<CsrfResponse>("/api/auth/csrf", {
    method: "GET",
  });

  if (!response.requestToken) {
    throw new Error("CSRF token nije dostupan nakon pripreme zahtjeva.");
  }

  return {
    headerName: response.csrfTokenHeaderName,
    token: response.requestToken,
  };
}

export async function logout(): Promise<void> {
  const csrfToken = await getCsrf();

  await apiRequest("/api/auth/logout", {
    method: "POST",
    headers: {
      [csrfToken.headerName]: csrfToken.token,
    },
  });
}

export async function login(request: LoginRequest): Promise<SessionResponse> {
  const csrfToken = await getCsrf();

  return apiRequest<SessionResponse>("/api/auth/login", {
    method: "POST",
    headers: {
      [csrfToken.headerName]: csrfToken.token,
    },
    json: request,
  });
}

export async function changePassword(
  request: ChangePasswordRequest,
): Promise<SessionResponse> {
  const csrfToken = await getCsrf();

  return apiRequest<SessionResponse>("/api/auth/change-password", {
    method: "POST",
    headers: {
      [csrfToken.headerName]: csrfToken.token,
    },
    json: request,
  });
}
