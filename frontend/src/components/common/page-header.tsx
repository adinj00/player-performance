import type { ReactNode } from "react";

interface PageHeaderProps {
  title: string;
  description?: string;
  eyebrow?: string;
  actions?: ReactNode;
}

export function PageHeader({
  title,
  description,
  eyebrow,
  actions,
}: PageHeaderProps) {
  return (
    <header className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
      <div className="min-w-0 space-y-2">
        {eyebrow ? (
          <p className="text-muted-foreground font-mono text-xs tracking-[0.18em] uppercase">
            {eyebrow}
          </p>
        ) : null}

        <div className="space-y-2">
          <h1
            className="font-heading text-foreground scroll-mt-24 text-3xl leading-tight outline-none md:text-4xl"
            data-route-heading
            tabIndex={-1}
          >
            {title}
          </h1>

          {description ? (
            <p className="text-muted-foreground max-w-3xl text-sm leading-6 md:text-base">
              {description}
            </p>
          ) : null}
        </div>
      </div>

      {actions ? (
        <div className="flex shrink-0 flex-wrap items-center gap-2">
          {actions}
        </div>
      ) : null}
    </header>
  );
}
