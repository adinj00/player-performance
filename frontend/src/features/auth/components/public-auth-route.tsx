import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { LoadingState } from "@/components/common/loading-state";
import { useSession } from "@/features/auth/hooks/use-session";

interface PublicAuthRouteProps {
  children: ReactNode;
}

function resolveAuthenticatedRedirect(
  redirectTarget: unknown,
  fallbackPath: string,
): string {
  if (typeof redirectTarget !== "string" || redirectTarget.length === 0) {
    return fallbackPath;
  }

  return redirectTarget.startsWith("/") ? redirectTarget : fallbackPath;
}

export function PublicAuthRoute({ children }: PublicAuthRouteProps) {
  const location = useLocation();
  const { isLoading, isAuthenticated } = useSession();

  if (isLoading) {
    return <LoadingState label="Provjera korisničke sesije..." />;
  }

  if (isAuthenticated) {
    return (
      <Navigate
        to={resolveAuthenticatedRedirect(
          location.state?.from,
          routePaths.dashboard,
        )}
        replace
      />
    );
  }

  return <>{children}</>;
}
