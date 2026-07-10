export type StaffRole =
  "ADMIN" | "DATA_OPERATOR" | "ANALYST" | "COACH" | "MEDICAL_STAFF" | "VIEWER";
export type StaffAccountStatus = "INVITED" | "ACTIVE" | "DISABLED" | "LOCKED";
export type TeamScopeType = "ALL_TEAMS" | "SELECTED_TEAMS";

export interface StaffPermissionSet {
  canVerifyReports: boolean;
  canImportData: boolean;
  canViewMedicalDetails: boolean;
}
export interface StaffTeamScope {
  type: TeamScopeType;
  selectedTeamIds: string[];
}
export interface StaffUserResponse {
  id: string;
  displayName: string;
  email: string;
  status: StaffAccountStatus;
  primaryRole: StaffRole;
  permissions: StaffPermissionSet;
  teamScope: StaffTeamScope;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
export interface StaffInvitationCredentialResponse {
  user: StaffUserResponse;
  setupToken: string;
}
export interface StaffListFilters {
  q?: string;
  role?: StaffRole;
  status?: StaffAccountStatus;
  scope?: TeamScopeType;
  team?: string;
}
export interface StaffAccessRequest extends StaffPermissionSet {
  primaryRole: StaffRole;
  teamScopeType: TeamScopeType;
  selectedTeamIds: string[];
}
export interface CreateStaffInvitationRequest extends StaffAccessRequest {
  displayName: string;
  email: string;
}
export interface TeamResponse {
  id: string;
  name: string;
  status: "ACTIVE" | "INACTIVE" | "ARCHIVED";
}

export const roleLabels: Record<StaffRole, string> = {
  ADMIN: "Administrator",
  DATA_OPERATOR: "Operater podataka",
  ANALYST: "Analitičar",
  COACH: "Trener",
  MEDICAL_STAFF: "Medicinsko osoblje",
  VIEWER: "Pregled",
};
export const statusLabels: Record<StaffAccountStatus, string> = {
  INVITED: "Pozvan",
  ACTIVE: "Aktivan",
  DISABLED: "Onemogućen",
  LOCKED: "Zaključan",
};
export const scopeLabels: Record<TeamScopeType, string> = {
  ALL_TEAMS: "Svi timovi",
  SELECTED_TEAMS: "Odabrane selekcije",
};
export const permissionLabels: Record<keyof StaffPermissionSet, string> = {
  canVerifyReports: "Može verifikovati izvještaje",
  canImportData: "Može importovati podatke",
  canViewMedicalDetails: "Može pregledati medicinske detalje",
};
