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
  options: Record<string, string>;
  onChange: (value: string | null) => void;
  className?: string;
}

export function FilterSelect({
  label,
  emptyLabel,
  value,
  options,
  onChange,
  className = "w-40",
}: FilterSelectProps) {
  return (
    <Select
      value={value ?? "__clear__"}
      onValueChange={(nextValue) =>
        onChange(nextValue === "__clear__" ? null : nextValue)
      }
    >
      <SelectTrigger className={cn(className, "shrink-0")} aria-label={label}>
        <SelectValue>{value ? options[value] : label}</SelectValue>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>{label}</SelectLabel>
          <SelectItem value="__clear__">{emptyLabel}</SelectItem>
          {Object.entries(options).map(([optionValue, optionLabel]) => (
            <SelectItem key={optionValue} value={optionValue}>
              {optionLabel}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
