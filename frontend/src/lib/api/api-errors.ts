export interface ProblemDetailsError {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
  code?: string;
  extensions?: Record<string, unknown>;
}

export interface ApiValidationErrors {
  [field: string]: string[];
}

export interface NormalizedApiError {
  status: number;
  title: string;
  detail: string | null;
  traceId: string | null;
  validationErrors: ApiValidationErrors | null;
  code: string | null;
  responseBody?: unknown;
}

export class ApiError extends Error implements NormalizedApiError {
  readonly status: number;
  readonly title: string;
  readonly detail: string | null;
  readonly traceId: string | null;
  readonly validationErrors: ApiValidationErrors | null;
  readonly code: string | null;
  readonly responseBody?: unknown;

  constructor(error: NormalizedApiError) {
    super(error.title);
    this.name = "ApiError";
    this.status = error.status;
    this.title = error.title;
    this.detail = error.detail;
    this.traceId = error.traceId;
    this.validationErrors = error.validationErrors;
    this.code = error.code;
    this.responseBody = error.responseBody;
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function readValidationErrors(value: unknown): ApiValidationErrors | null {
  if (!isRecord(value)) {
    return null;
  }

  const entries = Object.entries(value).filter(([, entryValue]) => {
    return (
      Array.isArray(entryValue) &&
      entryValue.every((message) => typeof message === "string")
    );
  });

  if (entries.length === 0) {
    return null;
  }

  return Object.fromEntries(entries) as ApiValidationErrors;
}

function readString(value: unknown): string | null {
  return typeof value === "string" && value.trim().length > 0 ? value : null;
}

export function normalizeApiError(
  status: number,
  responseBody: unknown,
  fallbackTitle = "Zahtjev nije uspio.",
): ApiError {
  const body = isRecord(responseBody) ? responseBody : null;
  const errors =
    readValidationErrors(body?.errors) ??
    readValidationErrors(body?.extensions) ??
    null;

  return new ApiError({
    status,
    title: readString(body?.title) ?? fallbackTitle,
    detail: readString(body?.detail),
    traceId:
      readString(body?.traceId) ??
      readString(body?.requestId) ??
      readString(body?.traceIdentifier),
    validationErrors: errors,
    code:
      readString(body?.code) ??
      readString(isRecord(body?.extensions) ? body.extensions.code : null),
    responseBody,
  });
}
