import { LayoutDashboard } from "lucide-react";

import { ContentSection } from "@/components/common/content-section";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";

export function AppEmptyState() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Početni prikaz"
        title="Kontrolna ploča"
        description="Osnovni okvir aplikacije je postavljen. Naredni feature specovi će dodati stvarne module i podatke."
      />

      <ContentSection>
        <EmptyState
          icon={<LayoutDashboard className="mx-auto h-10 w-10" />}
          title="Sistem je spreman za sljedeći korak"
        />
      </ContentSection>
    </div>
  );
}
