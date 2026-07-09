export function AppEmptyState() {
  return (
    <section className="flex min-h-[calc(100vh-10rem)] items-center justify-center">
      <div className="border-border bg-card w-full max-w-2xl rounded-xl border p-8 shadow-sm">
        <p className="text-club-red font-mono text-sm tracking-[0.18em] uppercase">
          Kontrolna ploča
        </p>
        <h1 className="font-heading text-foreground mt-4 text-3xl">
          Sistem je spreman za sljedeći korak.
        </h1>
        <p className="text-muted-foreground mt-3 max-w-xl text-sm leading-6">
          Osnovni okvir aplikacije je postavljen. Naredni feature specovi će
          dodati stvarne module i podatke.
        </p>
      </div>
    </section>
  );
}
