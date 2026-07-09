import { Menu } from "lucide-react";
import { useLocation } from "react-router-dom";

import { getRouteDefinition } from "@/app/route-paths";
import { Button } from "@/components/ui/button";

interface AppTopbarProps {
  onOpenSidebar: () => void;
}

export function AppTopbar({ onOpenSidebar }: AppTopbarProps) {
  const location = useLocation();
  const routeDefinition = getRouteDefinition(location.pathname);
  const pageTitle = routeDefinition?.title ?? "Nepoznata stranica";

  return (
    <header className="sticky top-0 z-20 border-b border-border bg-background/95 backdrop-blur-sm">
      <div className="flex min-h-18 items-center justify-between gap-4 px-4 py-3 md:px-6">
        <div className="flex min-w-0 items-center gap-3">
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="md:hidden"
            onClick={onOpenSidebar}
            aria-label="Otvori navigaciju"
          >
            <Menu className="h-5 w-5" />
          </Button>

          <div className="min-w-0">
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-muted-foreground">
              Početni prikaz
            </p>
            <h2 className="truncate font-heading text-2xl text-foreground">
              {pageTitle}
            </h2>
          </div>
        </div>

        <div className="hidden items-center gap-2 lg:flex">
          <div className="rounded-lg border border-border bg-surface px-3 py-2 text-right">
            <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">
              Sezona
            </p>
            <p className="text-sm font-medium text-foreground">
              Sezona nije odabrana
            </p>
          </div>

          <div className="rounded-lg border border-border bg-surface px-3 py-2 text-right">
            <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">
              Selekcija
            </p>
            <p className="text-sm font-medium text-foreground">
              Selekcija nije odabrana
            </p>
          </div>
        </div>

        <div className="rounded-lg border border-border bg-card px-3 py-2 text-sm text-muted-foreground">
          Korisničke akcije uskoro
        </div>
      </div>
    </header>
  );
}
