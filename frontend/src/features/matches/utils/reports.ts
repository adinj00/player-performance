import type {
  MatchReportAction,
  MatchReportStatus,
} from "@/features/matches/types/match";

export const reportStatusLabels: Record<MatchReportStatus, string> = {
  DRAFT: "Nacrt",
  READY_FOR_REVIEW: "Spremno za pregled",
  VERIFIED: "Verificirano",
  NEEDS_CORRECTION: "Potrebna korekcija",
  ARCHIVED: "Arhivirano",
};

export function hasReportAction(
  actions: MatchReportAction[] | undefined,
  action: MatchReportAction,
) {
  return actions?.includes(action) ?? false;
}
