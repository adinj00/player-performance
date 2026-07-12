import type { MatchLineupRole } from "@/features/matches/types/match";

interface LineupEntrySnapshot {
  playerId: string;
  role: MatchLineupRole;
}

interface SubstitutionSnapshot {
  playerOutId: string;
  playerInId: string;
}

export function validateOrderedSubstitutions(
  entries: LineupEntrySnapshot[],
  substitutions: SubstitutionSnapshot[],
): string | null {
  const lineupIds = new Set(entries.map((entry) => entry.playerId));
  const onField = new Set(
    entries
      .filter((entry) => entry.role === "STARTER")
      .map((entry) => entry.playerId),
  );

  for (const [index, substitution] of substitutions.entries()) {
    if (
      !lineupIds.has(substitution.playerOutId) ||
      !lineupIds.has(substitution.playerInId)
    ) {
      return `Izmjena ${index + 1} mora koristiti igrače iz sastava.`;
    }
    if (substitution.playerOutId === substitution.playerInId) {
      return `Igrač koji izlazi i ulazi u izmjeni ${index + 1} ne može biti isti.`;
    }
    if (!onField.has(substitution.playerOutId)) {
      return `Igrač koji izlazi u izmjeni ${index + 1} nije na terenu.`;
    }
    if (onField.has(substitution.playerInId)) {
      return `Igrač koji ulazi u izmjeni ${index + 1} već je na terenu.`;
    }

    onField.delete(substitution.playerOutId);
    onField.add(substitution.playerInId);
  }

  return null;
}
