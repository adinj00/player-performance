import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import {
  type UseSessionResult,
  useSession,
} from "@/features/auth/hooks/use-session";
import type { SessionUser } from "@/features/auth/types/session";

import { ProtectedRoute } from "./protected-route";

vi.mock("@/features/auth/hooks/use-session", () => ({
  useSession: vi.fn(),
}));

const sessionUser: SessionUser = {
  id: "user-1",
  email: "staff@velez.ba",
  accountStatus: "ACTIVE",
  mustChangePassword: false,
  primaryRole: "COACH",
  permissions: {
    canVerifyReports: false,
    canImportData: false,
    canViewMedicalDetails: false,
  },
  teamScope: { type: "ALL", selectedTeamIds: [] },
};

function setSession(overrides: Partial<UseSessionResult> = {}) {
  vi.mocked(useSession).mockReturnValue({
    session: { isAuthenticated: false, user: null },
    user: null,
    isLoading: false,
    isAuthenticated: false,
    isUnauthenticated: true,
    isError: false,
    error: null,
    refetchSession: vi.fn(),
    ...overrides,
  });
}

function SignInProbe() {
  const location = useLocation();
  return <p>{String(location.state?.from ?? "no return path")}</p>;
}

function renderProtectedRoute() {
  return render(
    <MemoryRouter initialEntries={["/players?teamId=team-1#availability"]}>
      <Routes>
        <Route
          path="/players"
          element={
            <ProtectedRoute>
              <p>Zaštićeni sadržaj</p>
            </ProtectedRoute>
          }
        />
        <Route path={routePaths.signIn} element={<SignInProbe />} />
        <Route
          path={routePaths.changePassword}
          element={<p>Promjena lozinke</p>}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProtectedRoute", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("redirects an unauthenticated user to sign-in with the full return path", () => {
    setSession();

    renderProtectedRoute();

    expect(
      screen.getByText("/players?teamId=team-1#availability"),
    ).toBeInTheDocument();
  });

  it("renders its children for an authenticated user", () => {
    setSession({
      session: { isAuthenticated: true, user: sessionUser },
      user: sessionUser,
      isAuthenticated: true,
      isUnauthenticated: false,
    });

    renderProtectedRoute();

    expect(screen.getByText("Zaštićeni sadržaj")).toBeInTheDocument();
  });

  it("redirects users who must change their password", () => {
    const passwordChangeUser = { ...sessionUser, mustChangePassword: true };
    setSession({
      session: { isAuthenticated: true, user: passwordChangeUser },
      user: passwordChangeUser,
      isAuthenticated: true,
      isUnauthenticated: false,
    });

    renderProtectedRoute();

    expect(screen.getByText("Promjena lozinke")).toBeInTheDocument();
  });
});
