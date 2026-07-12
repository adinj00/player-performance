import type {
  MatchLocationType,
  MatchStatus,
} from "@/features/matches/types/match";

export const matchStatusLabels: Record<MatchStatus, string> = {
  SCHEDULED: "Zakazana",
  PLAYED: "Odigrana",
  POSTPONED: "Odgođena",
  CANCELLED: "Otkazana",
};

export const locationLabels: Record<MatchLocationType, string> = {
  HOME: "Domaćin",
  AWAY: "Gost",
  NEUTRAL: "Neutralni teren",
};

export function localDateTimeParts(value: string) {
  const date = new Date(value);
  const offsetDate = new Date(
    date.getTime() - date.getTimezoneOffset() * 60_000,
  );
  const [day, time] = offsetDate.toISOString().split("T");
  return { date: day, time: time.slice(0, 5) };
}

export function toUtc(date: string, time: string) {
  return new Date(`${date}T${time}:00`).toISOString();
}
