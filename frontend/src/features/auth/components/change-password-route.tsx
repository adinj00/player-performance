import { Navigate } from "react-router-dom";
import type { ReactNode } from "react";

import { routePaths } from "@/app/route-paths";
import { ErrorState } from "@/components/common/error-state";
import { LoadingState } from "@/components/common/loading-state";
import { Button } from "@/components/ui/button";
import { useSession } from "@/features/auth/hooks/use-session";

interface ChangePasswordRouteProps {
  children: ReactNode;
}

export function ChangePasswordRoute({ children }: ChangePasswordRouteProps) {
  const { isLoading, isError, isAuthenticated, user, refetchSession } =
    useSession();

  if (isLoading) {
    return <LoadingState label="Provjera korisničke sesije..." />;
  }

  if (isError) {
    return (
      <ErrorState
        title="Sesiju nije moguće provjeriti"
        description="Pokušajte ponovo prije promjene lozinke."
        action={
          <Button onClick={() => void refetchSession()} variant="outline">
            Pokušaj ponovo
          </Button>
        }
      />
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.signIn} replace />;
  }

  if (!user?.mustChangePassword) {
    return <Navigate to={routePaths.dashboard} replace />;
  }

  return <>{children}</>;
}
