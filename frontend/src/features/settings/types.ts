export interface Season {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isArchived: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface NamedSetting {
  id: string;
  name: string;
  isArchived: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type TeamTrackingLevel = "BASIC" | "STANDARD" | "FULL";
export type TeamStatus = "ACTIVE" | "INACTIVE" | "ARCHIVED";

export interface Team {
  id: string;
  name: string;
  trackingLevel: TeamTrackingLevel;
  status: TeamStatus;
  displayOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export const trackingLevelLabels: Record<TeamTrackingLevel, string> = {
  BASIC: "Osnovni",
  STANDARD: "Standardni",
  FULL: "Potpuni",
};

export const trackingLevelHelp: Record<TeamTrackingLevel, string> = {
  BASIC: "Osnovni obim praćenja i unosa podataka.",
  STANDARD: "Prošireni obim praćenja za selekciju.",
  FULL: "Puni raspoloživi obim praćenja i izvještavanja.",
};

export const teamStatusLabels: Record<TeamStatus, string> = {
  ACTIVE: "Aktivna",
  INACTIVE: "Neaktivna",
  ARCHIVED: "Arhivirana",
};
