import { BrowserRouter, Outlet, Route, Routes } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { NotFoundPage } from "@/components/common/not-found-page";
import { AppShell } from "@/components/layout/app-shell";
import {
  AuthUnavailablePage,
  ProtectedRoute,
  PublicAuthRoute,
  SignInPage,
} from "@/features/auth";
import { DashboardPage } from "@/pages/dashboard-page";
import { ImportsPage } from "@/pages/imports-page";
import { MatchesPage } from "@/pages/matches-page";
import { MedicalPage } from "@/pages/medical-page";
import { MediaPage } from "@/pages/media-page";
import { PlayersPage } from "@/pages/players-page";
import { SettingsPage } from "@/pages/settings-page";
import { TeamsPage } from "@/pages/teams-page";
import { TrainingGpsPage } from "@/pages/training-gps-page";
import { UsersPage } from "@/pages/users-page";

function ProtectedAppShell() {
  return (
    <ProtectedRoute>
      <AppShell>
        <Outlet />
      </AppShell>
    </ProtectedRoute>
  );
}

export function AppRoutes() {
  return (
    <BrowserRouter>
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
        <Route element={<ProtectedAppShell />}>
          <Route path={routePaths.dashboard} element={<DashboardPage />} />
          <Route path={routePaths.matches} element={<MatchesPage />} />
          <Route path={routePaths.players} element={<PlayersPage />} />
          <Route path={routePaths.trainingGps} element={<TrainingGpsPage />} />
          <Route path={routePaths.imports} element={<ImportsPage />} />
          <Route path={routePaths.teams} element={<TeamsPage />} />
          <Route path={routePaths.medical} element={<MedicalPage />} />
          <Route path={routePaths.media} element={<MediaPage />} />
          <Route path={routePaths.users} element={<UsersPage />} />
          <Route path={routePaths.settings} element={<SettingsPage />} />
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}
