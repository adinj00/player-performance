export type MatchStatus = "SCHEDULED" | "PLAYED" | "POSTPONED" | "CANCELLED";
export type MatchLocationType = "HOME" | "AWAY" | "NEUTRAL";

export interface MatchReference {
  id: string;
  name: string;
}

export interface MatchResponse {
  id: string;
  season: MatchReference;
  competition: MatchReference;
  team: MatchReference;
  opponent: MatchReference;
  venue: MatchReference | null;
  kickoffAtUtc: string;
  round: string | null;
  locationType: MatchLocationType;
  status: MatchStatus;
  teamScore: number | null;
  opponentScore: number | null;
  isArchived: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedMatches {
  items: MatchResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface MatchListFilters {
  seasonId?: string | null;
  teamId?: string | null;
  competitionId?: string | null;
  opponentId?: string | null;
  status?: MatchStatus | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  includeArchived?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateMatchRequest {
  seasonId: string;
  competitionId: string;
  teamId: string;
  opponentId: string;
  venueId: string | null;
  kickoffAtUtc: string;
  round: string | null;
  locationType: MatchLocationType;
}

export interface UpdateMatchRequest extends Omit<CreateMatchRequest, "teamId"> {
  status: MatchStatus;
  teamScore: number | null;
  opponentScore: number | null;
}

export type MatchLineupRole = "STARTER" | "SUBSTITUTE";

export interface MatchLineupPlayer {
  id: string;
  firstName: string;
  lastName: string;
  preferredName: string | null;
}

export interface MatchLineupEntry {
  id: string;
  player: MatchLineupPlayer;
  role: MatchLineupRole;
}

export interface MatchAppearance {
  id: string;
  playerId: string;
  minutesPlayed: number;
}

export interface MatchSubstitution {
  id: string;
  playerOutId: string;
  playerInId: string;
  minute: number;
  stoppageTimeMinute: number | null;
  sequence: number;
}

export interface MatchLineupResponse {
  matchId: string;
  teamId: string;
  matchStatus: MatchStatus;
  isArchived: boolean;
  formation: string | null;
  captain: MatchLineupPlayer | null;
  entries: MatchLineupEntry[];
  appearances: MatchAppearance[];
  substitutions: MatchSubstitution[];
}

export interface SaveMatchLineupRequest {
  formation: string | null;
  captainPlayerId: string | null;
  entries: Array<{ playerId: string; role: MatchLineupRole }>;
  appearances: Array<{ playerId: string; minutesPlayed: number }>;
  substitutions: Array<{
    playerOutId: string;
    playerInId: string;
    minute: number;
    stoppageTimeMinute: number | null;
    sequence: number;
  }>;
}

export interface EligibleLineupPlayer extends MatchLineupPlayer {
  status: "ACTIVE" | "ARCHIVED";
  assignmentStartDate: string;
  assignmentEndDate: string | null;
}

export interface MatchReportResponse {
  id: string;
  matchId: string;
  status:
    "DRAFT" | "READY_FOR_REVIEW" | "VERIFIED" | "NEEDS_CORRECTION" | "ARCHIVED";
}

export type MatchReportStatus = MatchReportResponse["status"];

export type PlayerStatisticFieldCode =
  | "goals"
  | "assists"
  | "yellowCards"
  | "redCards"
  | "shots"
  | "shotsOnTarget"
  | "passesAttempted"
  | "passesCompleted"
  | "keyPasses"
  | "duelsAttempted"
  | "duelsWon"
  | "foulsCommitted"
  | "foulsWon"
  | "offsides"
  | "ballRecoveries"
  | "possessionLosses";

export type GoalkeeperStatisticFieldCode =
  "saves" | "goalsConceded" | "cleanSheet" | "penaltySaves";

export interface StatisticsAppearance {
  playerMatchAppearanceId: string;
  playerId: string;
  firstName: string;
  lastName: string;
  preferredName: string | null;
  minutesPlayed: number;
}

export type PlayerStatisticsRow = {
  playerMatchAppearanceId: string;
  isComplete: boolean;
} & {
  [K in PlayerStatisticFieldCode]: number | null;
};

export type GoalkeeperStatisticsRow = {
  playerMatchAppearanceId: string;
  isComplete: boolean;
  saves: number | null;
  goalsConceded: number | null;
  cleanSheet: boolean | null;
  penaltySaves: number | null;
};

export interface MatchReportStatisticsResponse {
  reportId: string;
  matchId: string;
  reportStatus: MatchReportStatus;
  appliedTrackingLevel: "BASIC" | "STANDARD" | "FULL" | null;
  enabledPlayerFields: string[];
  enabledGoalkeeperFields: string[];
  appearances: StatisticsAppearance[];
  playerStatistics: PlayerStatisticsRow[];
  goalkeeperStatistics: GoalkeeperStatisticsRow[];
  isComplete: boolean;
  canEditStatistics: boolean;
}

export interface SaveMatchReportStatisticsRequest {
  playerStatistics: Array<
    { playerMatchAppearanceId: string } & Record<
      PlayerStatisticFieldCode,
      number | null
    >
  >;
  goalkeeperStatistics: Array<
    { playerMatchAppearanceId: string } & Record<
      GoalkeeperStatisticFieldCode,
      number | boolean | null
    >
  >;
}
