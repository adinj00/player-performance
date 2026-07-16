import { CalendarIcon } from "lucide-react";
import { useState, type ComponentProps } from "react";

import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import { formatDate } from "@/lib/date-format";

interface DatePickerProps {
  id: string;
  value?: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  disabledDates?: ComponentProps<typeof Calendar>["disabled"];
  className?: string;
  "aria-label"?: string;
  "aria-invalid"?: boolean;
}

function fromIso(value?: string) {
  if (!value) return undefined;
  const [year, month, day] = value.split("-").map(Number);
  return new Date(year, month - 1, day);
}

function toIso(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

export function DatePicker({
  id,
  value,
  onChange,
  placeholder = "Odaberite datum",
  disabled,
  disabledDates,
  className,
  "aria-label": ariaLabel,
  "aria-invalid": ariaInvalid,
}: DatePickerProps) {
  const [open, setOpen] = useState(false);
  const selected = fromIso(value);
  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button
            id={id}
            variant="outline"
            className={cn("w-full justify-start font-normal", className)}
            disabled={disabled}
            aria-invalid={ariaInvalid}
            aria-label={ariaLabel}
          />
        }
      >
        <CalendarIcon data-icon="inline-start" />
        {selected ? formatDate(value!) : placeholder}
      </PopoverTrigger>
      <PopoverContent align="start" className="w-auto overflow-hidden p-0">
        <Calendar
          mode="single"
          selected={selected}
          defaultMonth={selected}
          disabled={disabledDates}
          onSelect={(date) => {
            if (date) {
              onChange(toIso(date));
              setOpen(false);
            }
          }}
          captionLayout="dropdown"
          weekStartsOn={1}
        />
      </PopoverContent>
    </Popover>
  );
}
