import { LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { Button } from "@/components/ui/button";
import { useLogout } from "@/features/auth/hooks/use-logout";
import { useSession } from "@/features/auth/hooks/use-session";
import { isApiError } from "@/lib/api/api-client";

function getLogoutErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    return error.detail ?? error.title;
  }

  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message;
  }

  return "Odjava trenutno nije uspjela. Pokušajte ponovo.";
}

export function UserMenu() {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useSession();
  const logoutMutation = useLogout();

  if (!isAuthenticated || !user) {
    return (
      <div className="border-border bg-card text-muted-foreground rounded-lg border px-3 py-2 text-sm">
        Nema aktivne sesije
      </div>
    );
  }

  return (
    <div className="border-border bg-card flex flex-col gap-2 rounded-lg border px-3 py-2 sm:min-w-64 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <p className="text-muted-foreground text-xs tracking-[0.14em] uppercase">
          Prijavljeno osoblje
        </p>
        <p className="text-foreground truncate text-sm font-medium">
          {user.email}
        </p>
      </div>

      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="outline"
          onClick={() => {
            logoutMutation.mutate(undefined, {
              onSuccess: () => {
                navigate(routePaths.signIn, { replace: true });
              },
            });
          }}
          disabled={logoutMutation.isPending}
        >
          <LogOut className="h-4 w-4" />
          {logoutMutation.isPending ? "Odjava..." : "Odjava"}
        </Button>
      </div>

      {logoutMutation.isError ? (
        <p className="text-destructive text-sm sm:basis-full">
          {getLogoutErrorMessage(logoutMutation.error)}
        </p>
      ) : null}
    </div>
  );
}
