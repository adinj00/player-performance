import { describe, expect, it } from "vitest";

import { getRouteDefinition, routePaths } from "@/app/route-paths";

describe("getRouteDefinition", () => {
  it.each([
    [routePaths.signIn, "Prijava"],
    [routePaths.acceptInvitation, "Prihvatanje poziva"],
    [routePaths.dashboard, "Kontrolna ploča"],
    [routePaths.settingsOpponents, "Postavke"],
  ])("resolves the legitimate static route %s", (pathname, title) => {
    expect(getRouteDefinition(pathname)?.title).toBe(title);
  });

  it.each([
    [routePaths.matchDetail("match-1"), "Utakmice"],
    ["/players/player-1", "Igrači"],
    ["/training-sessions/session-1", "Treninzi i GPS"],
  ])("resolves the legitimate detail route %s", (pathname, title) => {
    expect(getRouteDefinition(pathname)?.title).toBe(title);
  });

  it("does not label an unknown nested route as legitimate", () => {
    expect(getRouteDefinition("/players/player-1/unknown")).toBeNull();
  });
});
