import { type ReactNode, useState } from "react";
import { X } from "lucide-react";

import { AppSidebar } from "@/components/layout/app-sidebar";
import { AppTopbar } from "@/components/layout/app-topbar";
import { Button } from "@/components/ui/button";

interface AppShellProps {
  children: ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  const closeSidebar = () => {
    setIsSidebarOpen(false);
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="flex min-h-screen">
        <div className="hidden md:block md:w-80 md:shrink-0">
          <AppSidebar className="sticky top-0 h-screen" />
        </div>

        {isSidebarOpen && (
          <div className="md:hidden">
            <button
              type="button"
              className="fixed inset-0 z-30 bg-foreground/15 backdrop-blur-[1px]"
              onClick={closeSidebar}
              aria-label="Zatvori navigaciju"
            />
            <div className="fixed inset-y-0 left-0 z-40 w-74 max-w-[85vw]">
              <div className="absolute right-3 top-3 z-10">
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={closeSidebar}
                  aria-label="Zatvori navigaciju"
                >
                  <X className="h-5 w-5" />
                </Button>
              </div>
              <AppSidebar className="shadow-lg" onNavigate={closeSidebar} />
            </div>
          </div>
        )}

        <div className="flex min-w-0 flex-1 flex-col">
          <AppTopbar onOpenSidebar={() => setIsSidebarOpen(true)} />
          <main className="flex-1 overflow-x-hidden bg-surface">
            <div className="mx-auto w-full max-w-7xl px-4 py-6 md:px-6 lg:px-8">
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
