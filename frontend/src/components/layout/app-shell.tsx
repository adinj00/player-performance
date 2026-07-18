import { type ReactNode, useState } from "react";
import { X } from "lucide-react";

import { AppSidebar } from "@/components/layout/app-sidebar";
import { AppTopbar } from "@/components/layout/app-topbar";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";

interface AppShellProps {
  children: ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const closeSidebar = () => {
    setIsSidebarOpen(false);
  };

  return (
    <div className="bg-background text-foreground min-h-screen">
      <a
        className="bg-primary text-primary-foreground focus-visible:ring-ring fixed top-3 left-3 z-60 -translate-y-24 rounded-md px-4 py-3 text-sm font-semibold shadow-lg transition-transform focus:translate-y-0 focus:outline-none focus-visible:ring-3"
        href="#glavni-sadrzaj"
      >
        Preskoči na glavni sadržaj
      </a>
      <div className="flex min-h-screen">
        <div className="hidden md:block md:w-80 md:shrink-0">
          <AppSidebar className="sticky top-0 h-screen" />
        </div>

        <Sheet open={isSidebarOpen} onOpenChange={setIsSidebarOpen}>
          <SheetContent
            className="w-[min(20rem,85vw)] gap-0 p-0 data-[side=left]:max-w-none"
            side="left"
            showCloseButton={false}
          >
            <SheetHeader className="sr-only">
              <SheetTitle>Navigacija aplikacije</SheetTitle>
              <SheetDescription>
                Odaberite stranicu ili zatvorite navigaciju.
              </SheetDescription>
            </SheetHeader>
            <SheetClose
              render={
                <Button
                  aria-label="Zatvori navigaciju"
                  className="absolute top-3 right-3"
                  size="icon-sm"
                  variant="outline"
                />
              }
            >
              <X />
            </SheetClose>
            <AppSidebar className="shadow-lg" onNavigate={closeSidebar} />
          </SheetContent>
        </Sheet>

        <div className="flex min-w-0 flex-1 flex-col">
          <AppTopbar onOpenSidebar={() => setIsSidebarOpen(true)} />
          <main
            className="bg-surface flex-1 overflow-x-hidden"
            id="glavni-sadrzaj"
            tabIndex={-1}
          >
            <div className="mx-auto w-full max-w-7xl px-4 py-6 md:px-6 lg:px-8">
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
