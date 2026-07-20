import { describe, expect, it } from "vitest";

import { routePaths } from "@/app/route-paths";
import { resolveSafeReturnPath } from "./route-decisions";

describe("resolveSafeReturnPath", () => {
  it("keeps an internal destination including query and hash", () => {
    expect(resolveSafeReturnPath("/players?teamId=team-1#availability")).toBe(
      "/players?teamId=team-1#availability",
    );
  });

  it.each([
    [undefined],
    ["https://example.com"],
    ["//example.com"],
    ["\\\\example.com"],
    [routePaths.signIn],
    [routePaths.changePassword],
  ])("uses the dashboard for an unsafe destination: %s", (candidate) => {
    expect(resolveSafeReturnPath(candidate)).toBe(routePaths.dashboard);
  });
});
