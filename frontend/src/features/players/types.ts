export type PlayerStatus = "ACTIVE" | "INACTIVE" | "ARCHIVED";
export type AssignmentTimingState = "UPCOMING" | "CURRENT" | "PAST";
export interface CurrentAssignment {
  teamId: string;
  teamName: string;
  startDate: string;
  endDate: string | null;
}
export interface Player {
  id: string;
  firstName: string;
  lastName: string;
  preferredName: string | null;
  displayName: string;
  dateOfBirth: string | null;
  status: PlayerStatus;
  currentAssignments: CurrentAssignment[];
  createdAtUtc: string;
  updatedAtUtc: string;
}
export interface PlayerAssignment {
  id: string;
  playerId: string;
  teamId: string;
  teamName: string;
  startDate: string;
  endDate: string | null;
  timingState: AssignmentTimingState;
  createdAtUtc: string;
  updatedAtUtc: string;
}
export interface PagedPlayers {
  items: Player[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface PlayerFilters {
  search?: string;
  status?: PlayerStatus;
  teamId?: string;
  page: number;
  pageSize: number;
}
export interface PlayerRequest {
  firstName: string;
  lastName: string;
  preferredName?: string | null;
  dateOfBirth?: string | null;
}
export const playerStatusLabels: Record<PlayerStatus, string> = {
  ACTIVE: "Aktivan",
  INACTIVE: "Neaktivan",
  ARCHIVED: "Arhiviran",
};
export const assignmentTimingLabels: Record<AssignmentTimingState, string> = {
  UPCOMING: "Predstojeći",
  CURRENT: "Trenutna",
  PAST: "Završena",
};
