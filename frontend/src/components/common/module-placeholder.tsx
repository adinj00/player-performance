interface ModulePlaceholderProps {
  title: string;
  description: string;
}

export function ModulePlaceholder({
  title,
  description,
}: ModulePlaceholderProps) {
  return (
    <section className="flex min-h-[calc(100vh-10rem)] items-center justify-center">
      <div className="w-full max-w-3xl rounded-xl border border-border bg-card p-8 shadow-sm">
        <p className="font-mono text-sm uppercase tracking-[0.18em] text-club-red">
          {title}
        </p>
        <h1 className="mt-4 font-heading text-3xl text-foreground">
          Modul je pripremljen za narednu fazu.
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground">
          {description}
        </p>
      </div>
    </section>
  );
}
