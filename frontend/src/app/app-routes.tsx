import { useQueryClient } from "@tanstack/react-query";
import { NuqsAdapter } from "nuqs/adapters/react-router/v7";
import { lazy, Suspense, useEffect } from "react";
import {
  BrowserRouter,
  Navigate,
  Outlet,
  Route,
  Routes,
  useNavigate,
} from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { NotFoundPage } from "@/components/common/not-found-page";
import { LoadingState } from "@/components/common/loading-state";
import { RouteFocusManager } from "@/components/common/route-focus-manager";
import { AppShell } from "@/components/layout/app-shell";
import {
  AuthUnavailablePage,
  ChangePasswordPage,
  ChangePasswordRoute,
  ProtectedRoute,
  PublicAuthRoute,
  SignInPage,
} from "@/features/auth";
import { sessionQueryKey } from "@/features/auth/hooks/use-session";
import { AcceptInvitationPage } from "@/pages/accept-invitation-page";
const DashboardPage = lazy(() =>
  import("@/pages/dashboard-page").then(({ DashboardPage }) => ({
    default: DashboardPage,
  })),
);
const ImportsPage = lazy(() =>
  import("@/pages/imports-page").then(({ ImportsPage }) => ({
    default: ImportsPage,
  })),
);
const MatchesPage = lazy(() =>
  import("@/pages/matches-page").then(({ MatchesPage }) => ({
    default: MatchesPage,
  })),
);
const MatchDetailPage = lazy(() =>
  import("@/features/matches").then(({ MatchDetailPage }) => ({
    default: MatchDetailPage,
  })),
);
const MatchReportsPage = lazy(() =>
  import("@/features/matches").then(({ MatchReportsPage }) => ({
    default: MatchReportsPage,
  })),
);
const MedicalPage = lazy(() =>
  import("@/pages/medical-page").then(({ MedicalPage }) => ({
    default: MedicalPage,
  })),
);
const MediaPage = lazy(() =>
  import("@/features/media").then(({ MediaPage }) => ({
    default: MediaPage,
  })),
);
const PlayersPage = lazy(() =>
  import("@/pages/players-page").then(({ PlayersPage }) => ({
    default: PlayersPage,
  })),
);
const PlayerDetailPage = lazy(() =>
  import("@/features/players").then(({ PlayerDetailPage }) => ({
    default: PlayerDetailPage,
  })),
);
const TeamsPage = lazy(() =>
  import("@/pages/teams-page").then(({ TeamsPage }) => ({
    default: TeamsPage,
  })),
);
const TrainingSessionsPage = lazy(() =>
  import("@/features/training/pages").then(({ TrainingSessionsPage }) => ({
    default: TrainingSessionsPage,
  })),
);
const TrainingSessionDetailPage = lazy(() =>
  import("@/features/training/pages").then(({ TrainingSessionDetailPage }) => ({
    default: TrainingSessionDetailPage,
  })),
);
const UsersPage = lazy(() =>
  import("@/pages/users-page").then(({ UsersPage }) => ({
    default: UsersPage,
  })),
);
const SettingsLayout = lazy(() =>
  import("@/features/settings/components").then(({ SettingsLayout }) => ({
    default: SettingsLayout,
  })),
);
const NamedSettingsPage = lazy(() =>
  import("@/features/settings/pages").then(({ NamedSettingsPage }) => ({
    default: NamedSettingsPage,
  })),
);
const SeasonsSettingsPage = lazy(() =>
  import("@/features/settings/pages").then(({ SeasonsSettingsPage }) => ({
    default: SeasonsSettingsPage,
  })),
);
const TeamsSettingsPage = lazy(() =>
  import("@/features/settings/pages").then(({ TeamsSettingsPage }) => ({
    default: TeamsSettingsPage,
  })),
);

function ProtectedAppShell() {
  return (
    <ProtectedRoute>
      <AppShell>
        <Suspense fallback={<LoadingState label="Učitavanje stranice..." />}>
          <Outlet />
        </Suspense>
      </AppShell>
    </ProtectedRoute>
  );
}

function PasswordChangeRequiredSignalHandler() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  useEffect(() => {
    function handlePasswordChangeRequired() {
      void queryClient.invalidateQueries({ queryKey: sessionQueryKey });
      navigate(routePaths.changePassword, { replace: true });
    }

    window.addEventListener(
      "auth:password-change-required",
      handlePasswordChangeRequired,
    );
    return () =>
      window.removeEventListener(
        "auth:password-change-required",
        handlePasswordChangeRequired,
      );
  }, [navigate, queryClient]);

  return null;
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <NuqsAdapter>
        <RouteFocusManager />
        <PasswordChangeRequiredSignalHandler />
        <Routes>
          <Route
            path={routePaths.signIn}
            element={
              <PublicAuthRoute>
                <SignInPage />
              </PublicAuthRoute>
            }
          />
          <Route
            path={routePaths.acceptInvitation}
            element={<AcceptInvitationPage />}
          />
          <Route
            path={routePaths.forgotPassword}
            element={
              <PublicAuthRoute>
                <AuthUnavailablePage
                  title="Povrat lozinke"
                  description="Prikaz za povrat lozinke je spreman, ali backend podrška još nije implementirana."
                />
              </PublicAuthRoute>
            }
          />
          <Route
            path={routePaths.resetPassword}
            element={
              <PublicAuthRoute>
                <AuthUnavailablePage
                  title="Postavljanje nove lozinke"
                  description="Forma za postavljanje nove lozinke bit će dostupna nakon što backend uvede sigurni reset tok."
                />
              </PublicAuthRoute>
            }
          />
          <Route
            path={routePaths.changePassword}
            element={
              <ChangePasswordRoute>
                <ChangePasswordPage />
              </ChangePasswordRoute>
            }
          />
          <Route element={<ProtectedAppShell />}>
            <Route path={routePaths.dashboard} element={<DashboardPage />} />
            <Route path={routePaths.matches} element={<MatchesPage />} />
            <Route
              path={routePaths.matchReports}
              element={<MatchReportsPage />}
            />
            <Route path="/matches/:matchId" element={<MatchDetailPage />} />
            <Route path={routePaths.players} element={<PlayersPage />} />
            <Route path="/players/:playerId" element={<PlayerDetailPage />} />
            <Route
              path={routePaths.trainingGps}
              element={<TrainingSessionsPage />}
            />
            <Route
              path="/training-sessions/:id"
              element={<TrainingSessionDetailPage />}
            />
            <Route path={routePaths.imports} element={<ImportsPage />} />
            <Route path={routePaths.teams} element={<TeamsPage />} />
            <Route path={routePaths.medical} element={<MedicalPage />} />
            <Route path={routePaths.media} element={<MediaPage />} />
            <Route path={routePaths.users} element={<UsersPage />} />
            <Route path={routePaths.settings} element={<SettingsLayout />}>
              <Route
                index
                element={<Navigate to={routePaths.settingsSeasons} replace />}
              />
              <Route path="seasons" element={<SeasonsSettingsPage />} />
              <Route
                path="competitions"
                element={<NamedSettingsPage resource="competitions" />}
              />
              <Route path="teams" element={<TeamsSettingsPage />} />
              <Route
                path="venues"
                element={<NamedSettingsPage resource="venues" />}
              />
              <Route
                path="opponents"
                element={<NamedSettingsPage resource="opponents" />}
              />
            </Route>
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </NuqsAdapter>
    </BrowserRouter>
  );
}
