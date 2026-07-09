interface LoadingStateProps {
  label?: string;
}

export function LoadingState({ label = "Učitavanje..." }: LoadingStateProps) {
  return (
    <div
      className="flex min-h-40 flex-col items-center justify-center gap-3 text-center"
      role="status"
      aria-live="polite"
    >
      <div className="border-border border-t-primary h-8 w-8 animate-spin rounded-full border-2" />
      <p className="text-muted-foreground text-sm font-medium">{label}</p>
    </div>
  );
}
