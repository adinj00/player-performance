import type { ReactNode } from "react";

interface ContentSectionProps {
  title?: string;
  description?: string;
  actions?: ReactNode;
  children: ReactNode;
}

export function ContentSection({
  title,
  description,
  actions,
  children,
}: ContentSectionProps) {
  const hasHeader = title || description || actions;

  return (
    <section className="border-border bg-card rounded-xl border shadow-sm">
      {hasHeader ? (
        <div className="border-border flex flex-col gap-4 border-b px-6 py-5 md:flex-row md:items-start md:justify-between">
          <div className="min-w-0 space-y-1.5">
            {title ? (
              <h2 className="font-heading text-foreground text-xl">{title}</h2>
            ) : null}

            {description ? (
              <p className="text-muted-foreground max-w-3xl text-sm leading-6">
                {description}
              </p>
            ) : null}
          </div>

          {actions ? (
            <div className="flex shrink-0 items-center gap-2">{actions}</div>
          ) : null}
        </div>
      ) : null}

      <div className="p-6">{children}</div>
    </section>
  );
}
