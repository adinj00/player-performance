import { Link } from "react-router-dom";

import { routePaths } from "@/app/route-paths";

export function NotFoundPage() {
  return (
    <section className="flex min-h-[calc(100vh-10rem)] items-center justify-center">
      <div className="w-full max-w-3xl rounded-xl border border-border bg-card p-8 shadow-sm">
        <p className="font-mono text-sm uppercase tracking-[0.18em] text-club-red">
          Nepoznata stranica
        </p>
        <h1 className="mt-4 font-heading text-3xl text-foreground">
          Tražena ruta nije pronađena.
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground">
          Stranica koju pokušavate otvoriti trenutno nije dostupna u aplikaciji.
        </p>
        <Link
          to={routePaths.dashboard}
          className="mt-6 inline-flex rounded-lg border border-border bg-background px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
        >
          Nazad na kontrolnu ploču
        </Link>
      </div>
    </section>
  );
}
