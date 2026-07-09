import { getRequiredApiBaseUrl } from "@/lib/env";

import { ApiError, normalizeApiError } from "./api-errors";

export interface ApiRequestOptions extends Omit<
  RequestInit,
  "body" | "credentials"
> {
  body?: BodyInit | null;
  credentials?: RequestCredentials;
  json?: unknown;
}

function buildApiUrl(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${getRequiredApiBaseUrl()}${normalizedPath}`;
}

function hasJsonContentType(headers: Headers): boolean {
  const contentType = headers.get("content-type");
  return contentType !== null && contentType.includes("application/json");
}

async function parseResponseBody(response: Response): Promise<unknown> {
  if (response.status === 204 || response.status === 205) {
    return undefined;
  }

  const responseText = await response.text();

  if (responseText.length === 0) {
    return undefined;
  }

  const responseHeaders = new Headers(response.headers);

  if (!hasJsonContentType(responseHeaders)) {
    return responseText;
  }

  try {
    return JSON.parse(responseText) as unknown;
  } catch {
    return responseText;
  }
}

function createHeaders(headers?: HeadersInit): Headers {
  const requestHeaders = new Headers(headers);
  requestHeaders.set("Accept", "application/json");
  return requestHeaders;
}

function resolveBody(
  options: ApiRequestOptions,
  headers: Headers,
): BodyInit | null | undefined {
  if (options.json !== undefined) {
    if (!headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    return JSON.stringify(options.json);
  }

  return options.body;
}

export async function apiRequest<TResponse>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<TResponse> {
  const headers = createHeaders(options.headers);
  const body = resolveBody(options, headers);

  const response = await fetch(buildApiUrl(path), {
    ...options,
    body,
    credentials: options.credentials ?? "include",
    headers,
  });

  const responseBody = await parseResponseBody(response);

  if (!response.ok) {
    throw normalizeApiError(response.status, responseBody);
  }

  return responseBody as TResponse;
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
