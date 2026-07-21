import type { CurrentAssignment } from "./types";

export type PlayerLifecycleAction =
  "activate" | "deactivate" | "archive" | "restore";

export function getLifecycleBlockingAssignments(
  action: PlayerLifecycleAction,
  currentAssignments: CurrentAssignment[],
): CurrentAssignment[] {
  return action === "deactivate" || action === "archive"
    ? currentAssignments
    : [];
}
