import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";

interface FilterSelectProps {
  label: string;
  emptyLabel: string;
  value?: string | null;
  options?: Record<string, string>;
  items?: Array<[string, string]>;
  onChange: (value: string | null) => void;
  className?: string;
}

export function FilterSelect({
  label,
  emptyLabel,
  value,
  options,
  items,
  onChange,
  className = "w-40",
}: FilterSelectProps) {
  const resolvedOptions = options ?? Object.fromEntries(items ?? []);
  return (
    <Select
      value={value ?? "__clear__"}
      onValueChange={(nextValue) =>
        onChange(nextValue === "__clear__" ? null : nextValue)
      }
    >
      <SelectTrigger className={cn(className, "shrink-0")} aria-label={label}>
        <SelectValue>{value ? resolvedOptions[value] : label}</SelectValue>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>{label}</SelectLabel>
          <SelectItem value="__clear__">{emptyLabel}</SelectItem>
          {Object.entries(resolvedOptions).map(([optionValue, optionLabel]) => (
            <SelectItem key={optionValue} value={optionValue}>
              {optionLabel}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
