import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import type {
  PagedPlayers,
  Player,
  PlayerAssignment,
  PlayerFilters,
  PlayerRequest,
} from "./types";

async function unsafe<T>(
  path: string,
  method: "POST" | "PATCH",
  json?: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    json,
    headers: { [csrf.headerName]: csrf.token },
  });
}
export const playersApi = {
  list(filters: PlayerFilters) {
    const p = new URLSearchParams({
      page: String(filters.page),
      pageSize: String(filters.pageSize),
    });
    if (filters.search) p.set("search", filters.search);
    if (filters.status) p.set("status", filters.status);
    if (filters.teamId) p.set("teamId", filters.teamId);
    return apiRequest<PagedPlayers>(`/api/players?${p}`);
  },
  get: (id: string) => apiRequest<Player>(`/api/players/${id}`),
  create: (v: PlayerRequest) => unsafe<Player>("/api/players", "POST", v),
  update: (id: string, v: PlayerRequest) =>
    unsafe<Player>(`/api/players/${id}`, "PATCH", v),
  lifecycle: (
    id: string,
    action: "activate" | "deactivate" | "archive" | "restore",
  ) => unsafe<Player>(`/api/players/${id}/${action}`, "POST"),
  assignments: (id: string) =>
    apiRequest<PlayerAssignment[]>(`/api/players/${id}/assignments`),
  createAssignment: (
    id: string,
    v: { teamId: string; startDate: string; endDate?: string | null },
  ) => unsafe<PlayerAssignment>(`/api/players/${id}/assignments`, "POST", v),
  endAssignment: (p: string, a: string, endDate: string) =>
    unsafe<PlayerAssignment>(`/api/players/${p}/assignments/${a}/end`, "POST", {
      endDate,
    }),
};
