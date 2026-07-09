import type { ComponentType } from "react";
import {
  Activity,
  Film,
  HeartPulse,
  LayoutDashboard,
  Settings,
  ShieldCheck,
  Trophy,
  Upload,
  Users,
} from "lucide-react";

import { cn } from "@/lib/utils";

interface NavigationItem {
  label: string;
  icon: ComponentType<{ className?: string }>;
  isActive?: boolean;
}

interface NavigationGroup {
  label: string;
  items: NavigationItem[];
}

interface AppSidebarProps {
  className?: string;
  onNavigate?: () => void;
}

const navigationGroups: NavigationGroup[] = [
  {
    label: "Pregled",
    items: [
      { label: "Kontrolna ploča", icon: LayoutDashboard, isActive: true },
    ],
  },
  {
    label: "Performanse",
    items: [
      { label: "Utakmice", icon: Trophy },
      { label: "Igrači", icon: Users },
      { label: "Trening GPS", icon: Activity },
      { label: "Importi", icon: Upload },
    ],
  },
  {
    label: "Klub",
    items: [
      { label: "Timovi / Selekcije", icon: Users },
      { label: "Medicinski status", icon: HeartPulse },
      { label: "Medijska biblioteka", icon: Film },
    ],
  },
  {
    label: "Administracija",
    items: [
      { label: "Korisnici i uloge", icon: ShieldCheck },
      { label: "Postavke", icon: Settings },
    ],
  },
];

export function AppSidebar({ className, onNavigate }: AppSidebarProps) {
  return (
    <aside
      className={cn(
        "flex h-full w-full max-w-80 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground",
        className,
      )}
    >
      <div className="border-b border-sidebar-border px-5 py-5">
        <p className="font-mono text-xs uppercase tracking-[0.22em] text-club-red">
          FK Velež Mostar
        </p>
        <h1 className="mt-3 font-heading text-lg text-foreground">
          Performance Data System
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Operativni pregled kluba
        </p>
      </div>

      <nav
        aria-label="Glavna navigacija"
        className="flex-1 overflow-y-auto px-3 py-4"
      >
        {navigationGroups.map((group) => (
          <div key={group.label} className="mb-6 last:mb-0">
            <p className="px-2 text-xs font-semibold uppercase tracking-[0.18em] text-muted-foreground">
              {group.label}
            </p>
            <div className="mt-2 space-y-1">
              {group.items.map((item) => {
                const Icon = item.icon;

                return (
                  <button
                    key={item.label}
                    type="button"
                    onClick={onNavigate}
                    className={cn(
                      "flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm transition-colors outline-none focus-visible:ring-3 focus-visible:ring-sidebar-ring/50",
                      item.isActive
                        ? "bg-sidebar-primary text-sidebar-primary-foreground shadow-xs"
                        : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                    )}
                    aria-current={item.isActive ? "page" : undefined}
                  >
                    <Icon className="h-5 w-5 shrink-0" />
                    <span>{item.label}</span>
                  </button>
                );
              })}
            </div>
          </div>
        ))}
      </nav>

      <div className="border-t border-sidebar-border px-5 py-4">
        <p className="text-sm font-medium text-foreground">Korisnički meni</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Prostor za buduće korisničke akcije.
        </p>
      </div>
    </aside>
  );
}
