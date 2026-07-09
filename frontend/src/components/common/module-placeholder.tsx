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
      <div className="border-border bg-card w-full max-w-3xl rounded-xl border p-8 shadow-sm">
        <p className="text-club-red font-mono text-sm tracking-[0.18em] uppercase">
          {title}
        </p>
        <h1 className="font-heading text-foreground mt-4 text-3xl">
          Modul je pripremljen za narednu fazu.
        </h1>
        <p className="text-muted-foreground mt-3 max-w-2xl text-sm leading-6">
          {description}
        </p>
      </div>
    </section>
  );
}
