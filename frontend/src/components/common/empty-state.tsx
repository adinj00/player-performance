import type { ReactNode } from "react";

interface EmptyStateProps {
  title?: string;
  description?: string;
  icon?: ReactNode;
  action?: ReactNode;
}

export function EmptyState({
  title = "Nema podataka",
  description = "Sadržaj će biti prikazan kada bude dostupan.",
  icon,
  action,
}: EmptyStateProps) {
  return (
    <section className="border-border bg-card flex min-h-72 flex-col items-center justify-center rounded-xl border px-6 py-10 text-center shadow-sm sm:px-8">
      {icon ? <div className="text-muted-foreground mb-4">{icon}</div> : null}

      <div className="max-w-2xl space-y-3">
        <h2 className="font-heading text-foreground text-2xl leading-tight">
          {title}
        </h2>
        <p className="text-muted-foreground text-sm leading-6 md:text-base">
          {description}
        </p>
      </div>

      {action ? <div className="mt-6 flex justify-center">{action}</div> : null}
    </section>
  );
}
