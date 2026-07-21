import type { ComponentType } from "react";
import {
  Activity,
  ClipboardCheck,
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
  changePassword: "/change-password",
  acceptInvitation: "/accept-invitation",
  matches: "/matches",
  matchReports: "/match-reports",
  matchDetail: (matchId: string) => `/matches/${matchId}`,
  players: "/players",
  trainingGps: "/training-sessions",
  imports: "/imports",
  teams: "/teams",
  medical: "/availability",
  media: "/media",
  users: "/users",
  settings: "/settings",
  settingsSeasons: "/settings/seasons",
  settingsCompetitions: "/settings/competitions",
  settingsTeams: "/settings/teams",
  settingsVenues: "/settings/venues",
  settingsOpponents: "/settings/opponents",
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
    path: routePaths.signIn,
    title: "Prijava",
    description: "Prijava za ovlašteno osoblje kluba.",
  },
  {
    path: routePaths.forgotPassword,
    title: "Povrat lozinke",
    description: "Povrat pristupa korisničkom računu.",
  },
  {
    path: routePaths.resetPassword,
    title: "Postavljanje nove lozinke",
    description: "Postavljanje nove korisničke lozinke.",
  },
  {
    path: routePaths.changePassword,
    title: "Promjena lozinke",
    description: "Promjena privremene ili postojeće lozinke.",
  },
  {
    path: routePaths.acceptInvitation,
    title: "Prihvatanje poziva",
    description: "Aktivacija pozvanog korisničkog računa.",
  },
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
    path: routePaths.matchReports,
    title: "Izvještaji utakmica",
    description: "Pregled i verifikacija izvještaja utakmica.",
  },
  {
    path: routePaths.players,
    title: "Igrači",
    description: "Modul za igrače će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.trainingGps,
    title: "Treninzi i GPS",
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
    title: "Dostupnost igrača",
    description:
      "Modul za medicinski status i dostupnost igrača će biti dodan u kasnijem feature specu.",
  },
  {
    path: routePaths.media,
    title: "Medijateka",
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
  {
    path: routePaths.settingsSeasons,
    title: "Postavke",
    description: "Upravljanje osnovnim podacima sistema.",
  },
  {
    path: routePaths.settingsCompetitions,
    title: "Postavke",
    description: "Upravljanje osnovnim podacima sistema.",
  },
  {
    path: routePaths.settingsTeams,
    title: "Postavke",
    description: "Upravljanje osnovnim podacima sistema.",
  },
  {
    path: routePaths.settingsVenues,
    title: "Postavke",
    description: "Upravljanje osnovnim podacima sistema.",
  },
  {
    path: routePaths.settingsOpponents,
    title: "Postavke",
    description: "Upravljanje osnovnim podacima sistema.",
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
      {
        label: "Izvještaji utakmica",
        path: routePaths.matchReports,
        icon: ClipboardCheck,
      },
      { label: "Igrači", path: routePaths.players, icon: Users },
      { label: "Treninzi i GPS", path: routePaths.trainingGps, icon: Activity },
      {
        label: "Dostupnost igrača",
        path: routePaths.medical,
        icon: HeartPulse,
      },
      { label: "Importi", path: routePaths.imports, icon: Upload },
    ],
  },
  {
    label: "Klub",
    items: [
      { label: "Timovi / Selekcije", path: routePaths.teams, icon: Users },
      { label: "Medijateka", path: routePaths.media, icon: Film },
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
  const normalizedPathname =
    pathname.length > 1 ? pathname.replace(/\/+$/, "") : pathname;
  const exactMatch = appRouteDefinitions.find(
    (route) => route.path === normalizedPathname,
  );

  if (exactMatch) return exactMatch;

  const detailParentPaths = [
    routePaths.matches,
    routePaths.players,
    routePaths.trainingGps,
  ];
  const detailParent = detailParentPaths.find((parentPath) => {
    if (!normalizedPathname.startsWith(`${parentPath}/`)) return false;

    const detailId = normalizedPathname.slice(parentPath.length + 1);
    return detailId.length > 0 && !detailId.includes("/");
  });

  return detailParent
    ? (appRouteDefinitions.find((route) => route.path === detailParent) ?? null)
    : null;
}
