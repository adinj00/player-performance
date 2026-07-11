import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
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
import { DashboardPage } from "@/pages/dashboard-page";
import { ImportsPage } from "@/pages/imports-page";
import { MatchesPage } from "@/pages/matches-page";
import { MedicalPage } from "@/pages/medical-page";
import { MediaPage } from "@/pages/media-page";
import { PlayersPage } from "@/pages/players-page";
import { PlayerDetailPage } from "@/features/players";
import { TeamsPage } from "@/pages/teams-page";
import { TrainingGpsPage } from "@/pages/training-gps-page";
import { UsersPage } from "@/pages/users-page";
import { AcceptInvitationPage } from "@/pages/accept-invitation-page";
import { SettingsLayout } from "@/features/settings/components";
import {
  NamedSettingsPage,
  SeasonsSettingsPage,
  TeamsSettingsPage,
} from "@/features/settings/pages";

function ProtectedAppShell() {
  return (
    <ProtectedRoute>
      <AppShell>
        <Outlet />
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
          <Route path={routePaths.players} element={<PlayersPage />} />
          <Route path="/players/:playerId" element={<PlayerDetailPage />} />
          <Route path={routePaths.trainingGps} element={<TrainingGpsPage />} />
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
    </BrowserRouter>
  );
}
