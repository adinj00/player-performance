import type { FieldError } from "react-hook-form";

import { getFieldErrorMessage } from "@/lib/form-errors";
import { cn } from "@/lib/utils";

interface FormFieldMessageProps {
  error?: FieldError;
  className?: string;
}

export function FormFieldMessage({ error, className }: FormFieldMessageProps) {
  const message = getFieldErrorMessage(error);

  if (!message) {
    return null;
  }

  return (
    <p className={cn("text-destructive text-sm leading-5", className)}>
      {message}
    </p>
  );
}
