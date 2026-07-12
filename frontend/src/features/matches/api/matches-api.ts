import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";

import type {
  CreateMatchRequest,
  MatchListFilters,
  MatchLineupResponse,
  MatchReportResponse,
  MatchResponse,
  PagedMatches,
  EligibleLineupPlayer,
  SaveMatchLineupRequest,
  UpdateMatchRequest,
} from "@/features/matches/types/match";

function query(filters: MatchListFilters) {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (
      value !== null &&
      value !== undefined &&
      value !== false &&
      value !== ""
    ) {
      if (key === "dateFrom") {
        params.set(key, `${value}T00:00:00.000Z`);
      } else if (key === "dateTo") {
        params.set(key, `${value}T23:59:59.999Z`);
      } else {
        params.set(key, String(value));
      }
    }
  });
  const suffix = params.toString();
  return `/api/matches${suffix ? `?${suffix}` : ""}`;
}

async function mutate<T>(
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

export const matchesApi = {
  list: (filters: MatchListFilters) => apiRequest<PagedMatches>(query(filters)),
  get: (id: string) => apiRequest<MatchResponse>(`/api/matches/${id}`),
  getLineup: (id: string) =>
    apiRequest<MatchLineupResponse>(`/api/matches/${id}/lineup`),
  getEligibleLineupPlayers: (id: string) =>
    apiRequest<EligibleLineupPlayer[]>(
      `/api/matches/${id}/lineup/eligible-players`,
    ),
  getReport: (id: string) =>
    apiRequest<MatchReportResponse>(`/api/matches/${id}/report`),
  saveLineup: (id: string, request: SaveMatchLineupRequest) =>
    mutate<MatchLineupResponse>(`/api/matches/${id}/lineup`, "PUT", request),
  create: (request: CreateMatchRequest) =>
    mutate<MatchResponse>("/api/matches", "POST", request),
  update: (id: string, request: UpdateMatchRequest) =>
    mutate<MatchResponse>(`/api/matches/${id}`, "PATCH", request),
  archive: (id: string) =>
    mutate<MatchResponse>(`/api/matches/${id}/archive`, "POST"),
  restore: (id: string) =>
    mutate<MatchResponse>(`/api/matches/${id}/restore`, "POST"),
};
