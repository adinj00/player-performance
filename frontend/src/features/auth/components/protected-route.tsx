import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { ErrorState } from "@/components/common/error-state";
import { LoadingState } from "@/components/common/loading-state";
import { Button } from "@/components/ui/button";
import { useSession } from "@/features/auth/hooks/use-session";

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation();
  const { isLoading, isAuthenticated, isError, refetchSession } = useSession();

  if (isLoading) {
    return <LoadingState label="Provjera korisničke sesije..." />;
  }

  if (isError) {
    return (
      <ErrorState
        title="Sesiju nije moguće provjeriti"
        description="Veza sa backend servisom trenutno nije dostupna ili odgovor nije ispravan. Pokušajte ponovo."
        action={
          <Button
            type="button"
            variant="outline"
            onClick={() => void refetchSession()}
          >
            Pokušaj ponovo
          </Button>
        }
      />
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to={routePaths.signIn}
        replace
        state={{ from: location.pathname + location.search + location.hash }}
      />
    );
  }

  return <>{children}</>;
}
