import type { ReactNode } from "react";

interface TableRegionProps {
  label: string;
  children: ReactNode;
}

/** Provides a named, contained overflow region for dense data tables. */
export function TableRegion({ label, children }: TableRegionProps) {
  return (
    <div
      aria-label={label}
      className="overflow-x-auto rounded-xl border"
      role="region"
      tabIndex={0}
    >
      {children}
    </div>
  );
}
