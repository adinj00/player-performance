import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { LoadingState } from "@/components/common/loading-state";
import { useSession } from "@/features/auth/hooks/use-session";
import { resolveSafeReturnPath } from "@/features/auth/route-decisions";

interface PublicAuthRouteProps {
  children: ReactNode;
}

export function PublicAuthRoute({ children }: PublicAuthRouteProps) {
  const location = useLocation();
  const { isLoading, isAuthenticated, user } = useSession();

  if (isLoading) {
    return <LoadingState label="Provjera korisničke sesije..." />;
  }

  if (isAuthenticated) {
    return (
      <Navigate
        to={
          user?.mustChangePassword
            ? routePaths.changePassword
            : resolveSafeReturnPath(location.state?.from)
        }
        replace
      />
    );
  }

  return <>{children}</>;
}
