import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import type {
  PagedTrainingSessions,
  TrainingFilters,
  TrainingParticipant,
  TrainingSession,
} from "./types";
import type { PhysicalWorkload } from "./workloads";

function query(filters: TrainingFilters) {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) params.set(key, String(value));
  });
  params.set("pageSize", "25");
  return params.toString();
}
async function mutate<T>(
  path: string,
  method: "POST" | "PATCH" | "DELETE",
  json?: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    headers: { [csrf.headerName]: csrf.token },
    ...(json === undefined ? {} : { json }),
  });
}
export const trainingKeys = {
  list: (filters: TrainingFilters) => ["trainingSessions", filters] as const,
  detail: (id: string) => ["trainingSession", id] as const,
  participants: (id: string) =>
    ["trainingSession", id, "participants"] as const,
  candidates: (id: string, search: string) =>
    ["trainingSession", id, "candidates", search] as const,
  workloads: (id: string) => ["trainingSession", id, "workloads"] as const,
};
export const trainingApi = {
  list: (filters: TrainingFilters) =>
    apiRequest<PagedTrainingSessions>(
      `/api/training-sessions?${query(filters)}`,
    ),
  get: (id: string) =>
    apiRequest<TrainingSession>(`/api/training-sessions/${id}`),
  create: (request: Omit<TrainingSession, "id" | "status">) =>
    mutate<TrainingSession>("/api/training-sessions", "POST", request),
  update: (
    id: string,
    request: Omit<TrainingSession, "id" | "teamId" | "status">,
  ) =>
    mutate<TrainingSession>(`/api/training-sessions/${id}`, "PATCH", request),
  lifecycle: (id: string, action: "complete" | "cancel") =>
    mutate<TrainingSession>(`/api/training-sessions/${id}/${action}`, "POST"),
  participants: (id: string) =>
    apiRequest<TrainingParticipant[]>(
      `/api/training-sessions/${id}/participants`,
    ),
  candidates: (id: string, search: string) =>
    apiRequest<{ items: import("./types").ParticipantCandidate[] }>(
      `/api/training-sessions/${id}/participant-candidates?search=${encodeURIComponent(search)}&pageSize=25`,
    ),
  addParticipant: (id: string, playerId: string) =>
    mutate<TrainingParticipant>(
      `/api/training-sessions/${id}/participants`,
      "POST",
      { playerId },
    ),
  removeParticipant: (id: string, participantId: string) =>
    mutate<void>(
      `/api/training-sessions/${id}/participants/${participantId}`,
      "DELETE",
    ),
  workloads: (id: string) =>
    apiRequest<PhysicalWorkload[]>(`/api/training-sessions/${id}/workloads`),
};
