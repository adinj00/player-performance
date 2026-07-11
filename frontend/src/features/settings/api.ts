import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";

import type { NamedSetting, Season, Team, TeamTrackingLevel } from "./types";

function listPath(resource: string, includeArchived: boolean) {
  return `/api/settings/${resource}${includeArchived ? "?includeArchived=true" : ""}`;
}

async function unsafe<T>(
  path: string,
  method: "POST" | "PATCH" | "PUT",
  json?: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    headers: { [csrf.headerName]: csrf.token },
    ...(json === undefined ? {} : { json }),
  });
}

export const settingsApi = {
  listSeasons: (includeArchived: boolean) =>
    apiRequest<Season[]>(listPath("seasons", includeArchived)),
  createSeason: (request: Pick<Season, "name" | "startDate" | "endDate">) =>
    unsafe<Season>("/api/settings/seasons", "POST", request),
  updateSeason: (
    id: string,
    request: Pick<Season, "name" | "startDate" | "endDate">,
  ) => unsafe<Season>(`/api/settings/seasons/${id}`, "PATCH", request),
  listNamed: (
    resource: "competitions" | "venues" | "opponents",
    includeArchived: boolean,
  ) => apiRequest<NamedSetting[]>(listPath(resource, includeArchived)),
  createNamed: (
    resource: "competitions" | "venues" | "opponents",
    name: string,
  ) => unsafe<NamedSetting>(`/api/settings/${resource}`, "POST", { name }),
  updateNamed: (
    resource: "competitions" | "venues" | "opponents",
    id: string,
    name: string,
  ) =>
    unsafe<NamedSetting>(`/api/settings/${resource}/${id}`, "PATCH", { name }),
  listTeams: (includeArchived: boolean) =>
    apiRequest<Team[]>(listPath("teams", includeArchived)),
  createTeam: (request: { name: string; trackingLevel: TeamTrackingLevel }) =>
    unsafe<Team>("/api/settings/teams", "POST", request),
  updateTeam: (
    id: string,
    request: { name: string; trackingLevel: TeamTrackingLevel },
  ) => unsafe<Team>(`/api/settings/teams/${id}`, "PATCH", request),
  reorderTeams: (orderedTeamIds: string[]) =>
    unsafe<Team[]>("/api/settings/teams/order", "PUT", { orderedTeamIds }),
  lifecycle: (
    resource: "seasons" | "competitions" | "venues" | "opponents" | "teams",
    id: string,
    action: "archive" | "restore" | "activate" | "deactivate",
  ) => unsafe(`/api/settings/${resource}/${id}/${action}`, "POST"),
};
