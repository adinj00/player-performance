import { Link } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { ErrorState } from "@/components/common/error-state";
import { Button } from "@/components/ui/button";
import { useSession } from "@/features/auth/hooks/use-session";

import { AuthPageShell } from "./auth-page-shell";

export function SignInPage() {
  const { isError, refetchSession } = useSession();

  return (
    <AuthPageShell
      title="Prijava za osoblje"
      description="Player Performance Data System koristi zatvoreni pristup za klupsko osoblje. Ova stranica je spremna za povezivanje sa budućim backend login endpointom."
    >
      <div className="space-y-6">
        {isError ? (
          <ErrorState
            title="Provjera sesije nije uspjela"
            description="Aplikacija trenutno ne može potvrditi postojeću prijavu. Možete osvježiti provjeru dok login endpoint ne bude dostupan."
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
        ) : null}

        <div className="space-y-5">
          <div className="space-y-2">
            <label
              className="text-foreground text-sm font-medium"
              htmlFor="sign-in-email"
            >
              Email adresa
            </label>
            <input
              id="sign-in-email"
              type="email"
              disabled
              placeholder="ime.prezime@velez.ba"
              className="border-input bg-surface-muted text-muted-foreground placeholder:text-muted-foreground/80 flex h-11 w-full rounded-lg border px-3 text-sm"
            />
          </div>

          <div className="space-y-2">
            <label
              className="text-foreground text-sm font-medium"
              htmlFor="sign-in-password"
            >
              Lozinka
            </label>
            <input
              id="sign-in-password"
              type="password"
              disabled
              placeholder="Privremeno nedostupno"
              className="border-input bg-surface-muted text-muted-foreground placeholder:text-muted-foreground/80 flex h-11 w-full rounded-lg border px-3 text-sm"
            />
          </div>
        </div>

        <div className="border-border bg-surface-muted rounded-xl border p-4">
          <p className="text-foreground text-sm font-medium">
            Prijava će biti omogućena nakon povezivanja backend login endpointa.
          </p>
          <p className="text-muted-foreground mt-2 text-sm leading-6">
            Ovaj frontend prikaz već koristi stvarnu provjeru sesije, zaštitu
            ruta i odjavu, ali još ne šalje podatke za prijavu jer backend login
            API nije implementiran.
          </p>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <Button type="button" disabled className="sm:min-w-40">
            Prijava uskoro
          </Button>

          <div className="flex flex-col gap-2 text-sm sm:items-end">
            <Link
              to={routePaths.forgotPassword}
              className="text-primary hover:text-primary/80 underline-offset-4 hover:underline"
            >
              Zaboravljena lozinka
            </Link>
            <p className="text-muted-foreground">
              Javna registracija nije dostupna.
            </p>
          </div>
        </div>
      </div>
    </AuthPageShell>
  );
}
