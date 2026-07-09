import { AlertCircle } from "lucide-react";

import {
  DEFAULT_FORM_ERROR_MESSAGE,
  normalizeFormErrors,
} from "@/lib/form-errors";

interface FormErrorSummaryProps {
  errors?: string | string[] | null;
}

export function FormErrorSummary({ errors }: FormErrorSummaryProps) {
  const normalizedErrors = normalizeFormErrors(errors);
  const summaryErrors =
    normalizedErrors.length > 0
      ? normalizedErrors
      : errors
        ? [DEFAULT_FORM_ERROR_MESSAGE]
        : [];

  if (summaryErrors.length === 0) {
    return null;
  }

  return (
    <section
      aria-live="polite"
      className="border-destructive/20 bg-destructive/10 rounded-xl border px-4 py-3"
      role="alert"
    >
      <div className="flex gap-3">
        <div className="bg-destructive/15 text-destructive flex h-8 w-8 shrink-0 items-center justify-center rounded-lg">
          <AlertCircle className="h-4 w-4" aria-hidden="true" />
        </div>

        <div className="min-w-0 space-y-2">
          <p className="font-medium">Provjerite unesene podatke.</p>

          {summaryErrors.length === 1 ? (
            <p className="text-sm leading-6">{summaryErrors[0]}</p>
          ) : (
            <ul className="list-disc space-y-1 pl-5 text-sm leading-6">
              {summaryErrors.map((error) => (
                <li key={error}>{error}</li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
}
