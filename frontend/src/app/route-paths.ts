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

export const routePaths = {
  dashboard: "/",
  signIn: "/sign-in",
  forgotPassword: "/forgot-password",
  resetPassword: "/reset-password",
  matches: "/matches",
  players: "/players",
  trainingGps: "/training-gps",
  imports: "/imports",
  teams: "/teams",
  medical: "/medical",
  media: "/media",
  users: "/users",
  settings: "/settings",
} as const;

export interface AppRouteDefinition {
  path: string;
  title: string;
  description: string;
}

export interface NavigationItemDefinition {
  label: string;
  path: string;
  icon: ComponentType<{ className?: string }>;
}

export interface NavigationGroupDefinition {
  label: string;
  items: NavigationItemDefinition[];
}

export const appRouteDefinitions: AppRouteDefinition[] = [
  {
    path: routePaths.dashboard,
    title: "Kontrolna ploča",
    description: "Pregled sistema će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.matches,
    title: "Utakmice",
    description: "Modul za utakmice će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.players,
    title: "Igrači",
    description: "Modul za igrače će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.trainingGps,
    title: "Trening GPS",
    description:
      "Modul za GPS i fizičko opterećenje će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.imports,
    title: "Importi",
    description:
      "Modul za import podataka će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.teams,
    title: "Timovi / Selekcije",
    description:
      "Modul za timove i selekcije će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.medical,
    title: "Medicinski status",
    description:
      "Modul za medicinski status i dostupnost igrača će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.media,
    title: "Medijska biblioteka",
    description:
      "Modul za medije i vanjske reference će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.users,
    title: "Korisnici i uloge",
    description:
      "Modul za korisnike, uloge i pristup će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.settings,
    title: "Postavke",
    description:
      "Modul za sistemske postavke će biti dodan u kasnijem feature specu.",
  },
];

export const navigationGroups: NavigationGroupDefinition[] = [
  {
    label: "Pregled",
    items: [
      {
        label: "Kontrolna ploča",
        path: routePaths.dashboard,
        icon: LayoutDashboard,
      },
    ],
  },
  {
    label: "Performanse",
    items: [
      { label: "Utakmice", path: routePaths.matches, icon: Trophy },
      { label: "Igrači", path: routePaths.players, icon: Users },
      { label: "Trening GPS", path: routePaths.trainingGps, icon: Activity },
      { label: "Importi", path: routePaths.imports, icon: Upload },
    ],
  },
  {
    label: "Klub",
    items: [
      { label: "Timovi / Selekcije", path: routePaths.teams, icon: Users },
      {
        label: "Medicinski status",
        path: routePaths.medical,
        icon: HeartPulse,
      },
      { label: "Medijska biblioteka", path: routePaths.media, icon: Film },
    ],
  },
  {
    label: "Administracija",
    items: [
      { label: "Korisnici i uloge", path: routePaths.users, icon: ShieldCheck },
      { label: "Postavke", path: routePaths.settings, icon: Settings },
    ],
  },
];

export function getRouteDefinition(pathname: string) {
  return appRouteDefinitions.find((route) => route.path === pathname) ?? null;
}
