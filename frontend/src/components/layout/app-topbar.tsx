import { Menu } from "lucide-react";
import { useLocation } from "react-router-dom";

import { getRouteDefinition } from "@/app/route-paths";
import { UserMenu } from "@/features/auth";
import { Button } from "@/components/ui/button";

interface AppTopbarProps {
  onOpenSidebar: () => void;
}

export function AppTopbar({ onOpenSidebar }: AppTopbarProps) {
  const location = useLocation();
  const routeDefinition = getRouteDefinition(location.pathname);
  const pageTitle = routeDefinition?.title ?? "Nepoznata stranica";

  return (
    <header className="border-border bg-background/95 sticky top-0 z-20 border-b backdrop-blur-sm">
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
            <p className="text-muted-foreground text-xs font-semibold tracking-[0.18em] uppercase">
              Početni prikaz
            </p>
            <h2 className="font-heading text-foreground truncate text-2xl">
              {pageTitle}
            </h2>
          </div>
        </div>

        <div className="hidden items-center gap-2 lg:flex">
          <div className="border-border bg-surface rounded-lg border px-3 py-2 text-right">
            <p className="text-muted-foreground text-xs tracking-[0.14em] uppercase">
              Sezona
            </p>
            <p className="text-foreground text-sm font-medium">
              Sezona nije odabrana
            </p>
          </div>

          <div className="border-border bg-surface rounded-lg border px-3 py-2 text-right">
            <p className="text-muted-foreground text-xs tracking-[0.14em] uppercase">
              Selekcija
            </p>
            <p className="text-foreground text-sm font-medium">
              Selekcija nije odabrana
            </p>
          </div>
        </div>

        <UserMenu />
      </div>
    </header>
  );
}
