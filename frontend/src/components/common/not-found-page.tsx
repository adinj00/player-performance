import { Link } from "react-router-dom";

import { routePaths } from "@/app/route-paths";

export function NotFoundPage() {
  return (
    <section className="flex min-h-[calc(100vh-10rem)] items-center justify-center">
      <div className="border-border bg-card w-full max-w-3xl rounded-xl border p-8 shadow-sm">
        <p className="text-club-red font-mono text-sm tracking-[0.18em] uppercase">
          Nepoznata stranica
        </p>
        <h1 className="font-heading text-foreground mt-4 text-3xl">
          Tražena ruta nije pronađena.
        </h1>
        <p className="text-muted-foreground mt-3 max-w-2xl text-sm leading-6">
          Stranica koju pokušavate otvoriti trenutno nije dostupna u aplikaciji.
        </p>
        <Link
          to={routePaths.dashboard}
          className="border-border bg-background text-foreground hover:bg-muted focus-visible:ring-ring/50 mt-6 inline-flex rounded-lg border px-4 py-2 text-sm font-medium transition-colors focus-visible:ring-3 focus-visible:outline-none"
        >
          Nazad na kontrolnu ploču
        </Link>
      </div>
    </section>
  );
}
