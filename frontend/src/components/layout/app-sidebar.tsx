import { NavLink } from "react-router-dom";

import { navigationGroups } from "@/app/route-paths";
import { useSession } from "@/features/auth/hooks/use-session";
import { cn } from "@/lib/utils";

interface AppSidebarProps {
  className?: string;
  onNavigate?: () => void;
}

export function AppSidebar({ className, onNavigate }: AppSidebarProps) {
  const { user, isLoading } = useSession();
  const groups = navigationGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) =>
        item.path === "/imports"
          ? !isLoading &&
            (user?.primaryRole === "ADMIN" ||
              (user?.primaryRole === "DATA_OPERATOR" &&
                user.permissions.canImportData))
          : (item.path !== "/users" && item.path !== "/settings") ||
            (!isLoading && user?.primaryRole === "ADMIN"),
      ),
    }))
    .filter((group) => group.items.length > 0);
  return (
    <aside
      className={cn(
        "border-sidebar-border bg-sidebar text-sidebar-foreground flex h-full w-full max-w-80 flex-col border-r",
        className,
      )}
    >
      <div className="border-sidebar-border border-b px-5 py-5">
        <p className="text-club-red font-mono text-xs tracking-[0.22em] uppercase">
          FK Velež Mostar
        </p>
        <h1 className="font-heading text-foreground mt-3 text-lg">
          Performance Data System
        </h1>
        <p className="text-muted-foreground mt-1 text-sm">
          Operativni pregled kluba
        </p>
      </div>

      <nav
        aria-label="Glavna navigacija"
        className="flex-1 overflow-y-auto px-3 py-4"
      >
        {groups.map((group) => (
          <div key={group.label} className="mb-6 last:mb-0">
            <p className="text-muted-foreground px-2 text-xs font-semibold tracking-[0.18em] uppercase">
              {group.label}
            </p>
            <div className="mt-2 space-y-1">
              {group.items.map((item) => {
                const Icon = item.icon;

                return (
                  <NavLink
                    key={item.label}
                    to={item.path}
                    onClick={onNavigate}
                    className={({ isActive }) =>
                      cn(
                        "focus-visible:ring-sidebar-ring/50 flex w-full items-center gap-3 rounded-lg border border-transparent px-3 py-2.5 text-left text-sm transition-colors outline-none focus-visible:ring-3",
                        isActive
                          ? "bg-sidebar-primary text-sidebar-primary-foreground font-semibold shadow-xs"
                          : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                      )
                    }
                  >
                    <Icon className="h-5 w-5 shrink-0" />
                    <span>{item.label}</span>
                  </NavLink>
                );
              })}
            </div>
          </div>
        ))}
      </nav>

      <div className="border-sidebar-border border-t px-5 py-4">
        <p className="text-foreground text-sm font-medium">Korisnički meni</p>
        <p className="text-muted-foreground mt-1 text-sm">
          Prostor za buduće korisničke akcije.
        </p>
      </div>
    </aside>
  );
}
