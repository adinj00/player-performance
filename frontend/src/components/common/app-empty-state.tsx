export function AppEmptyState() {
  return (
    <section className="flex min-h-[calc(100vh-10rem)] items-center justify-center">
      <div className="w-full max-w-2xl rounded-xl border border-border bg-card p-8 shadow-sm">
        <p className="font-mono text-sm uppercase tracking-[0.18em] text-club-red">
          Kontrolna ploča
        </p>
        <h1 className="mt-4 font-heading text-3xl text-foreground">
          Sistem je spreman za sljedeći korak.
        </h1>
        <p className="mt-3 max-w-xl text-sm leading-6 text-muted-foreground">
          Osnovni okvir aplikacije je postavljen. Naredni feature specovi će
          dodati stvarne module i podatke.
        </p>
      </div>
    </section>
  );
}
