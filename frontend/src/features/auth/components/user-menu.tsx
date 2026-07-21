import { ChevronsUpDown, CircleUserRound, LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useLogout } from "@/features/auth/hooks/use-logout";
import { useSession } from "@/features/auth/hooks/use-session";

const roleLabels: Record<string, string> = {
  ADMIN: "Administrator",
  DATA_OPERATOR: "Operater podataka",
  ANALYST: "Analitičar",
  COACH: "Trener",
  MEDICAL_STAFF: "Medicinsko osoblje",
  VIEWER: "Pregled",
};

export function UserMenu() {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useSession();
  const logoutMutation = useLogout();

  if (!isAuthenticated || !user) {
    return null;
  }

  const roleLabel = user.primaryRole
    ? (roleLabels[user.primaryRole] ?? "Osoblje")
    : "Osoblje";

  const logout = () => {
    logoutMutation.mutate(undefined, {
      onSuccess: () => {
        navigate(routePaths.signIn, { replace: true });
      },
    });
  };

  return (
    <div className="flex min-w-0 flex-col gap-2">
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button
              aria-label={`Otvori korisnički meni za ${user.email}`}
              className="h-auto w-full min-w-0 justify-start px-2 py-2 text-left"
              variant="ghost"
            />
          }
        >
          <CircleUserRound data-icon="inline-start" />
          <span className="min-w-0 flex-1">
            <span className="text-foreground block truncate text-sm font-medium">
              {user.email}
            </span>
            <span className="text-muted-foreground block truncate text-xs font-normal">
              {roleLabel}
            </span>
          </span>
          <ChevronsUpDown className="ml-auto" data-icon="inline-end" />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start" side="top" sideOffset={8}>
          <DropdownMenuGroup>
            <DropdownMenuLabel>Korisnički meni</DropdownMenuLabel>
            <DropdownMenuItem
              disabled={logoutMutation.isPending}
              onClick={logout}
              variant="destructive"
            >
              <LogOut />
              {logoutMutation.isPending ? "Odjava..." : "Odjava"}
            </DropdownMenuItem>
          </DropdownMenuGroup>
        </DropdownMenuContent>
      </DropdownMenu>

      {logoutMutation.isError ? (
        <p className="text-destructive px-2 text-xs" role="status">
          Odjava trenutno nije uspjela. Pokušajte ponovo.
        </p>
      ) : null}
    </div>
  );
}
