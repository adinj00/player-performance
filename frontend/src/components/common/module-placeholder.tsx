import { LayoutTemplate } from "lucide-react";

import { ContentSection } from "@/components/common/content-section";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";

interface ModulePlaceholderProps {
  title: string;
  description: string;
}

export function ModulePlaceholder({
  title,
  description,
}: ModulePlaceholderProps) {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Pregled modula"
        title={title}
        description={description}
      />

      <ContentSection>
        <EmptyState
          icon={<LayoutTemplate className="mx-auto h-10 w-10" />}
          title="Modul je spreman za narednu fazu"
          description="Ovaj dio aplikacije trenutno koristi zajedničke prikaze dok ne bude implementiran stvarni sadržaj."
        />
      </ContentSection>
    </div>
  );
}
