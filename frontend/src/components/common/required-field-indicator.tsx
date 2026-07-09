import { cn } from "@/lib/utils";

interface RequiredFieldIndicatorProps {
  className?: string;
}

export function RequiredFieldIndicator({
  className,
}: RequiredFieldIndicatorProps) {
  return (
    <span
      className={cn(
        "text-muted-foreground inline-flex items-center text-xs font-medium",
        className,
      )}
    >
      <span aria-hidden="true">*</span>
      <span className="ml-1">(obavezno)</span>
    </span>
  );
}
