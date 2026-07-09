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
    <div className="bg-background text-foreground min-h-screen">
      <div className="flex min-h-screen">
        <div className="hidden md:block md:w-80 md:shrink-0">
          <AppSidebar className="sticky top-0 h-screen" />
        </div>

        {isSidebarOpen && (
          <div className="md:hidden">
            <button
              type="button"
              className="bg-foreground/15 fixed inset-0 z-30 backdrop-blur-[1px]"
              onClick={closeSidebar}
              aria-label="Zatvori navigaciju"
            />
            <div className="fixed inset-y-0 left-0 z-40 w-74 max-w-[85vw]">
              <div className="absolute top-3 right-3 z-10">
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
          <main className="bg-surface flex-1 overflow-x-hidden">
            <div className="mx-auto w-full max-w-7xl px-4 py-6 md:px-6 lg:px-8">
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
