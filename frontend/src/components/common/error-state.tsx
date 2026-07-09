import type { ReactNode } from "react";
import { AlertCircle } from "lucide-react";

interface ErrorStateProps {
  title?: string;
  description?: string;
  action?: ReactNode;
}

export function ErrorState({
  title = "Došlo je do greške",
  description,
  action,
}: ErrorStateProps) {
  return (
    <section className="border-border bg-card rounded-xl border p-6 shadow-sm">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
        <div className="bg-destructive/10 text-destructive flex h-10 w-10 shrink-0 items-center justify-center rounded-lg">
          <AlertCircle className="h-5 w-5" aria-hidden="true" />
        </div>

        <div className="min-w-0 flex-1 space-y-2">
          <h2 className="font-heading text-foreground text-xl">{title}</h2>

          {description ? (
            <p className="text-muted-foreground text-sm leading-6">
              {description}
            </p>
          ) : null}
        </div>

        {action ? (
          <div className="flex shrink-0 items-center">{action}</div>
        ) : null}
      </div>
    </section>
  );
}
