export type TrainingSessionStatus = "PLANNED" | "COMPLETED" | "CANCELLED";

export interface TrainingSession {
  id: string;
  teamId: string;
  sessionDate: string;
  startsAtUtc: string | null;
  endsAtUtc: string | null;
  title: string;
  location: string | null;
  description: string | null;
  status: TrainingSessionStatus;
}

export interface PagedTrainingSessions {
  items: TrainingSession[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface TrainingParticipant {
  id: string;
  trainingSessionId: string;
  playerId: string;
  playerName: string;
  preferredName: string | null;
  removedAtUtc: string | null;
}

export interface ParticipantCandidate {
  id: string;
  firstName: string;
  lastName: string;
  preferredName: string | null;
  assignmentStartDate: string;
  assignmentEndDate: string | null;
}

export interface TrainingFilters {
  teamId?: string | null;
  status?: TrainingSessionStatus | null;
  search?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  page?: number;
}
