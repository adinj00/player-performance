import type { ReactNode } from "react";

import { ContentSection } from "@/components/common/content-section";
import { PageHeader } from "@/components/common/page-header";

interface AuthPageShellProps {
  title: string;
  description: string;
  children: ReactNode;
}

export function AuthPageShell({
  title,
  description,
  children,
}: AuthPageShellProps) {
  return (
    <div className="bg-surface box-border min-h-screen px-4 py-8 md:px-6 md:py-10">
      <div className="mx-auto flex w-full max-w-xl items-center justify-center">
        <div className="flex w-full flex-col gap-6">
          <div className="flex items-center gap-3">
            <img
              aria-hidden="true"
              alt=""
              className="size-14 shrink-0"
              src="/fk-velez-mostar-logo.svg"
            />
            <div>
              <p className="text-club-red font-mono text-xs tracking-[0.18em] uppercase">
                FK Velež Mostar
              </p>
              <p className="text-muted-foreground mt-1 text-sm">
                Performance Data System
              </p>
            </div>
          </div>

          <PageHeader title={title} description={description} />

          <ContentSection>{children}</ContentSection>
        </div>
      </div>
    </div>
  );
}
