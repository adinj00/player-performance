import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import {
  parsePagedAuditHistory,
  type AuditFilters,
} from "@/features/audit/types";

export type AvailabilityStatus =
  "AVAILABLE" | "LIMITED" | "UNAVAILABLE" | "REHAB" | "UNKNOWN";
export type InjuryStatus = "OPEN" | "RESOLVED";

export interface AvailabilityItem {
  playerId: string;
  displayName: string;
  teamId: string;
  status: AvailabilityStatus;
  effectiveOn: string | null;
  expectedReturnOn: string | null;
  coachVisibleNote: string | null;
  currentRevisionId: string | null;
  revisionNumber: number | null;
  recordedAtUtc: string | null;
  allowedActions: string[];
}
export interface AvailabilityPage {
  items: AvailabilityItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface AvailabilitySummary {
  totalPlayers: number;
  availableCount: number;
  limitedCount: number;
  unavailableCount: number;
  rehabCount: number;
  unknownCount: number;
}
export interface InjuryItem {
  id: string;
  playerId: string;
  teamId: string;
  occurredOn: string;
  status: InjuryStatus;
  resolvedOn: string | null;
}
export interface InjuryPage {
  items: InjuryItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface InjuryCandidate {
  id: string;
  displayName: string;
  preferredName: string | null;
  dateOfBirth: string | null;
  eligibleAssignment: {
    id: string;
    teamId: string;
    startDate: string;
    endDate: string | null;
  };
}
export interface AvailabilityHistoryItem extends AvailabilityItem {
  availabilityId: string;
}
export interface InjuryRevision {
  id: string;
  revisionNumber: number;
  bodyArea: string | null;
  diagnosis: string | null;
  restrictedNotes: string | null;
  recordedAtUtc: string;
}
export interface InjuryRevisionPage {
  items: InjuryRevision[];
  page: number;
  pageSize: number;
  totalCount: number;
}
export interface InjuryDetail extends InjuryItem {
  currentRevision: InjuryRevision;
  revisionCount: number;
  allowedActions: string[];
}

function params(values: Record<string, string | number | null | undefined>) {
  const query = new URLSearchParams();
  Object.entries(values).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== "")
      query.set(key, String(value));
  });
  return query.toString();
}
function auditParams(filters: AuditFilters) {
  return params({
    action: filters.action,
    dateFrom: filters.dateFrom,
    dateTo: filters.dateTo,
    page: filters.page ?? 1,
    pageSize: filters.pageSize ?? 25,
  });
}
async function mutate<T>(
  path: string,
  method: "POST" | "PATCH",
  json: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    headers: { [csrf.headerName]: csrf.token },
    json,
  });
}
export const medicalApi = {
  summary: (teamId: string) =>
    apiRequest<AvailabilitySummary>(
      `/api/player-availability/summary?teamId=${teamId}`,
    ),
  availability: (filters: {
    teamId: string;
    status?: string | null;
    search?: string | null;
    page: number;
  }) =>
    apiRequest<AvailabilityPage>(
      `/api/player-availability?${params({ teamId: filters.teamId, status: filters.status, search: filters.search, page: filters.page, pageSize: 25 })}`,
    ),
  recordAvailability: (
    playerId: string,
    body: {
      teamId: string;
      status: AvailabilityStatus;
      effectiveOn: string;
      expectedReturnOn: string | null;
      coachVisibleNote: string | null;
      expectedCurrentRevisionId: string | null;
    },
  ) => mutate(`/api/players/${playerId}/availability`, "POST", body),
  playerAvailability: (playerId: string, teamId: string) =>
    apiRequest<{ items: AvailabilityHistoryItem[] }>(
      `/api/players/${playerId}/availability?${params({ teamId, page: 1, pageSize: 25 })}`,
    ),
  availabilityAudit: (availabilityId: string, filters: AuditFilters = {}) =>
    apiRequest<unknown>(
      `/api/player-availability/${availabilityId}/audit?${auditParams(filters)}`,
    ).then(parsePagedAuditHistory),
  injuries: (filters: {
    teamId: string;
    status?: string | null;
    playerId?: string | null;
    occurredFrom?: string | null;
    occurredTo?: string | null;
    page: number;
  }) =>
    apiRequest<InjuryPage>(
      `/api/medical/injuries?${params({ teamId: filters.teamId, status: filters.status, playerId: filters.playerId, occurredFrom: filters.occurredFrom, occurredTo: filters.occurredTo, page: filters.page, pageSize: 25 })}`,
    ),
  candidates: (filters: {
    teamId: string;
    occurredOn: string;
    search?: string;
    playerId?: string;
  }) =>
    apiRequest<{ items: InjuryCandidate[] }>(
      `/api/medical/injury-player-candidates?${params({ teamId: filters.teamId, occurredOn: filters.occurredOn, search: filters.search, playerId: filters.playerId, page: 1, pageSize: 25 })}`,
    ),
  createInjury: (body: {
    playerId: string;
    teamId: string;
    occurredOn: string;
    bodyArea: string | null;
    diagnosis: string | null;
    restrictedNotes: string | null;
  }) => mutate("/api/medical/injuries", "POST", body),
  injury: (injuryId: string) =>
    apiRequest<InjuryDetail>(`/api/medical/injuries/${injuryId}`),
  injuryRevisions: (injuryId: string, page = 1) =>
    apiRequest<InjuryRevisionPage>(
      `/api/medical/injuries/${injuryId}/revisions?page=${page}&pageSize=25`,
    ),
  injuryAudit: (injuryId: string, filters: AuditFilters = {}) =>
    apiRequest<unknown>(
      `/api/medical/injuries/${injuryId}/audit?${auditParams(filters)}`,
    ).then(parsePagedAuditHistory),
  updateInjury: (
    injuryId: string,
    body: {
      bodyArea: string | null;
      diagnosis: string | null;
      restrictedNotes: string | null;
      expectedCurrentRevisionId: string;
    },
  ) => mutate(`/api/medical/injuries/${injuryId}`, "PATCH", body),
  resolveInjury: (
    injuryId: string,
    body: { resolvedOn: string; expectedCurrentRevisionId: string },
  ) => mutate(`/api/medical/injuries/${injuryId}/resolve`, "POST", body),
};
