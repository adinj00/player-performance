import type { FieldError } from "react-hook-form";

export const DEFAULT_FORM_ERROR_MESSAGE =
  "Došlo je do greške. Pokušajte ponovo.";

// Shared helpers stay generic. Feature-owned schemas and field rules belong
// near the forms that use them, and backend validation remains authoritative.
export function getFieldErrorMessage(
  error: FieldError | undefined,
): string | undefined {
  if (!error) {
    return undefined;
  }

  return getReadableErrorText(error.message);
}

export function normalizeFormError(error: unknown): string {
  if (typeof error === "string") {
    return getReadableErrorText(error) ?? DEFAULT_FORM_ERROR_MESSAGE;
  }

  if (error instanceof Error) {
    return getReadableErrorText(error.message) ?? DEFAULT_FORM_ERROR_MESSAGE;
  }

  if (hasMessage(error)) {
    return getReadableErrorText(error.message) ?? DEFAULT_FORM_ERROR_MESSAGE;
  }

  return DEFAULT_FORM_ERROR_MESSAGE;
}

export function normalizeFormErrors(
  errors: string | string[] | null | undefined,
): string[] {
  if (!errors) {
    return [];
  }

  const values = Array.isArray(errors) ? errors : [errors];

  return values
    .map((value) => getReadableErrorText(value))
    .filter((value): value is string => value !== undefined);
}

function getReadableErrorText(value: unknown): string | undefined {
  if (typeof value !== "string") {
    return undefined;
  }

  const trimmedValue = value.trim();

  return trimmedValue.length > 0 ? trimmedValue : undefined;
}

function hasMessage(value: unknown): value is { message: unknown } {
  return typeof value === "object" && value !== null && "message" in value;
}
