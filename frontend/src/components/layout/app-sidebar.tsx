import { NavLink } from "react-router-dom";

import { navigationGroups } from "@/app/route-paths";
import { UserMenu } from "@/features/auth";
import { useSession } from "@/features/auth/hooks/use-session";
import { cn } from "@/lib/utils";

interface AppSidebarProps {
  className?: string;
  onNavigate?: () => void;
  reserveCloseButtonSpace?: boolean;
}

export function AppSidebar({
  className,
  onNavigate,
  reserveCloseButtonSpace = false,
}: AppSidebarProps) {
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
        "border-sidebar-border bg-sidebar text-sidebar-foreground flex h-full w-full max-w-72 flex-col border-r",
        className,
      )}
    >
      <div
        className={cn(
          "border-sidebar-border border-b px-4 py-4",
          reserveCloseButtonSpace && "pr-12",
        )}
      >
        <div className="flex items-center gap-3">
          <img
            aria-hidden="true"
            alt=""
            className="size-12 shrink-0"
            src="/fk-velez-mostar-logo.svg"
          />
          <div className="min-w-0">
            <p className="text-club-red truncate font-mono text-xs tracking-[0.18em] uppercase">
              FK Velež Mostar
            </p>
            <p className="font-heading text-foreground mt-1 truncate text-sm font-semibold">
              Performance Data System
            </p>
            <p className="text-muted-foreground mt-0.5 truncate text-xs">
              Operativni pregled kluba
            </p>
          </div>
        </div>
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
            <div className="mt-2 flex flex-col gap-1">
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
                    <Icon className="size-5 shrink-0" />
                    <span>{item.label}</span>
                  </NavLink>
                );
              })}
            </div>
          </div>
        ))}
      </nav>

      <div className="border-sidebar-border border-t p-3">
        <UserMenu />
      </div>
    </aside>
  );
}
