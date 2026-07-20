import { describe, expect, it } from "vitest";

import { withLocationQuery } from "./location-query";

describe("withLocationQuery", () => {
  it("preserves active URL state while adding a detail parameter", () => {
    expect(
      withLocationQuery(
        {
          pathname: "/medical",
          search: "?teamId=team-1&injuryPage=2",
          hash: "#history",
        },
        { injuryId: "injury-1" },
      ),
    ).toBe("/medical?teamId=team-1&injuryPage=2&injuryId=injury-1#history");
  });

  it("replaces and removes only the requested parameters", () => {
    expect(
      withLocationQuery(
        {
          pathname: "/media",
          search: "?teamId=team-1&media=old-media&archived=true",
          hash: "",
        },
        { media: "new-media", archived: null },
      ),
    ).toBe("/media?teamId=team-1&media=new-media");
  });
});
