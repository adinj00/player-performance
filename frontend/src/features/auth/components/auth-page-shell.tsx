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
        <div className="w-full space-y-6">
          <PageHeader
            eyebrow="FK Velež Mostar"
            title={title}
            description={description}
          />

          <ContentSection>{children}</ContentSection>
        </div>
      </div>
    </div>
  );
}
