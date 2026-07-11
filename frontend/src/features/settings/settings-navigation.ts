import { routePaths } from "@/app/route-paths";

export const settingsNavigation = [
  ["Sezone", routePaths.settingsSeasons],
  ["Takmičenja", routePaths.settingsCompetitions],
  ["Timovi / Selekcije", routePaths.settingsTeams],
  ["Stadioni / Lokacije", routePaths.settingsVenues],
  ["Protivnici", routePaths.settingsOpponents],
] as const;
