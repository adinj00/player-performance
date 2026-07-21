import { describe, expect, it } from "vitest";

import { getLifecycleBlockingAssignments } from "./lifecycle";
import type { CurrentAssignment } from "./types";

const currentAssignments: CurrentAssignment[] = [
  {
    teamId: "team-1",
    teamName: "Prvi tim",
    startDate: "2026-07-01",
    endDate: null,
  },
];

describe("getLifecycleBlockingAssignments", () => {
  it.each(["deactivate", "archive"] as const)(
    "blocks %s while the player has current assignments",
    (action) => {
      expect(
        getLifecycleBlockingAssignments(action, currentAssignments),
      ).toEqual(currentAssignments);
    },
  );

  it.each(["activate", "restore"] as const)(
    "does not block %s because of current assignments",
    (action) => {
      expect(
        getLifecycleBlockingAssignments(action, currentAssignments),
      ).toEqual([]);
    },
  );
});
