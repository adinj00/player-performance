import { format, parseISO } from "date-fns";

export function formatDate(value: string) {
  return format(parseISO(value), "dd.MM.yyyy.");
}

export function formatUtcDateTime(value: string) {
  return format(new Date(value), "dd.MM.yyyy. HH:mm");
}
