import { CircleAlert } from "lucide-react";
import { Link } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { ContentSection } from "@/components/common/content-section";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";
import { Button } from "@/components/ui/button";

export function NotFoundPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Nepoznata stranica"
        title="Tražena ruta nije pronađena"
        description="Stranica koju pokušavate otvoriti trenutno nije dostupna u aplikaciji."
      />

      <ContentSection>
        <EmptyState
          icon={<CircleAlert className="mx-auto h-10 w-10" />}
          title="Nije moguće otvoriti traženu stranicu"
          description="Vratite se na kontrolnu ploču i nastavite iz dostupne navigacije."
          action={
            <Button render={<Link to={routePaths.dashboard} />}>
              Nazad na kontrolnu ploču
            </Button>
          }
        />
      </ContentSection>
    </div>
  );
}
