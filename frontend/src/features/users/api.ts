import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import type {
  CreateStaffInvitationRequest,
  StaffAccessRequest,
  StaffInvitationCredentialResponse,
  StaffListFilters,
  StaffUserResponse,
  TeamResponse,
} from "./types";

async function unsafe<T>(
  path: string,
  method: "POST" | "PATCH" | "PUT",
  json?: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    json,
    headers: { [csrf.headerName]: csrf.token },
  });
}
export function listStaff(filters: StaffListFilters) {
  const p = new URLSearchParams();
  if (filters.q) p.set("search", filters.q);
  if (filters.role) p.set("role", filters.role);
  if (filters.status) p.set("status", filters.status);
  if (filters.scope) p.set("scopeType", filters.scope);
  if (filters.team) p.set("teamId", filters.team);
  return apiRequest<StaffUserResponse[]>(`/api/users${p.size ? `?${p}` : ""}`, {
    method: "GET",
  });
}
export function listTeams() {
  return apiRequest<TeamResponse[]>(
    "/api/settings/teams?includeArchived=true",
    { method: "GET" },
  );
}
export function createInvitation(request: CreateStaffInvitationRequest) {
  return unsafe<StaffInvitationCredentialResponse>(
    "/api/users/invitations",
    "POST",
    request,
  );
}
export function reissueInvitation(id: string) {
  return unsafe<StaffInvitationCredentialResponse>(
    `/api/users/${id}/invitations/reissue`,
    "POST",
  );
}
export function updateProfile(id: string, displayName: string) {
  return unsafe<StaffUserResponse>(`/api/users/${id}`, "PATCH", {
    displayName,
  });
}
export function replaceAccess(id: string, request: StaffAccessRequest) {
  return unsafe<StaffUserResponse>(`/api/users/${id}/access`, "PUT", request);
}
export function disableStaff(id: string) {
  return unsafe<StaffUserResponse>(`/api/users/${id}/disable`, "POST");
}
export function reactivateStaff(id: string) {
  return unsafe<StaffUserResponse>(`/api/users/${id}/reactivate`, "POST");
}
export function acceptInvitation(request: {
  email: string;
  token: string;
  password: string;
  confirmPassword: string;
}) {
  return apiRequest<void>("/api/auth/invitations/accept", {
    method: "POST",
    json: request,
  });
}
