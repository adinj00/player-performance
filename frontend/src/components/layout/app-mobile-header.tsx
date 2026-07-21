import { Menu } from "lucide-react";

import { Button } from "@/components/ui/button";

interface AppMobileHeaderProps {
  onOpenSidebar: () => void;
}

export function AppMobileHeader({ onOpenSidebar }: AppMobileHeaderProps) {
  return (
    <header className="border-border bg-background/95 sticky top-0 z-20 border-b backdrop-blur-sm lg:hidden">
      <div className="flex h-16 items-center gap-3 px-4 md:px-6">
        <Button
          type="button"
          variant="outline"
          size="icon"
          onClick={onOpenSidebar}
          aria-label="Otvori navigaciju"
        >
          <Menu />
        </Button>

        <img
          aria-hidden="true"
          alt=""
          className="size-9 shrink-0"
          src="/fk-velez-mostar-logo.svg"
        />

        <div className="min-w-0">
          <p className="text-club-red truncate font-mono text-xs tracking-[0.16em] uppercase">
            FK Velež Mostar
          </p>
          <p className="text-muted-foreground truncate text-xs">
            Performance Data System
          </p>
        </div>
      </div>
    </header>
  );
}
