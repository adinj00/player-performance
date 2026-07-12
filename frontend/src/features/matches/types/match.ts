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
