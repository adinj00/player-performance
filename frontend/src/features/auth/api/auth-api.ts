import { apiRequest } from "@/lib/api/api-client";

import type {
  CsrfResponse,
  CsrfToken,
  SessionResponse,
} from "../types/session";

const csrfCookieName = "XSRF-TOKEN";

function readCookieValue(cookieName: string): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  const cookieEntries = document.cookie.split(";").map((entry) => entry.trim());
  const matchingEntry = cookieEntries.find((entry) =>
    entry.startsWith(`${cookieName}=`),
  );

  if (!matchingEntry) {
    return null;
  }

  return decodeURIComponent(matchingEntry.slice(cookieName.length + 1));
}

export function getSession() {
  return apiRequest<SessionResponse>("/api/auth/session", {
    method: "GET",
  });
}

export async function getCsrf(): Promise<CsrfToken> {
  const response = await apiRequest<CsrfResponse>("/api/auth/csrf", {
    method: "GET",
  });

  const token = readCookieValue(csrfCookieName);

  if (!token) {
    throw new Error(
      "CSRF token nije dostupan nakon pripreme zahtjeva za odjavu.",
    );
  }

  return {
    headerName: response.csrfTokenHeaderName,
    token,
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
