import { useState } from "react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export interface PhysicalMetric {
  metricCode: string;
  value: number;
  unitCode: string;
  thresholdValue: number | null;
  thresholdUnitCode: string | null;
  thresholdDirection: string | null;
  thresholdScope: string | null;
  methodKey: string | null;
  methodVersion: string | null;
}
export interface PhysicalWorkload {
  id: string;
  playerId: string;
  playerName: string;
  occurredOn: string;
  importJobId: string | null;
  sourceSystem: string | null;
  processorKey: string | null;
  processorVersion: string | null;
  revisionNumber: number;
  metrics: PhysicalMetric[];
}

const labels: Record<string, string> = {
  TOTAL_DISTANCE_METERS: "Ukupna udaljenost",
  HIGH_SPEED_RUNNING_DISTANCE_METERS: "Udaljenost velike brzine",
  SPRINT_DISTANCE_METERS: "Sprint udaljenost",
  SPRINT_COUNT: "Broj sprinteva",
  MAX_SPEED_METERS_PER_SECOND: "Maksimalna brzina",
  ACCELERATION_COUNT: "Ubrzanja",
  DECELERATION_COUNT: "Usporavanja",
  PLAYER_LOAD_ARBITRARY_UNITS: "Player load",
  SESSION_DURATION_SECONDS: "Trajanje sesije",
};
function value(metric: PhysicalMetric) {
  if (metric.unitCode === "METERS")
    return `${metric.value.toLocaleString("bs-BA")} m`;
  if (metric.unitCode === "METERS_PER_SECOND")
    return `${metric.value.toLocaleString("bs-BA")} m/s (${(metric.value * 3.6).toLocaleString("bs-BA")} km/h)`;
  if (metric.unitCode === "SECONDS")
    return `${Math.floor(metric.value / 60)} min ${metric.value % 60} s`;
  if (metric.unitCode === "ARBITRARY_UNITS")
    return `${metric.value.toLocaleString("bs-BA")} AU`;
  return metric.value.toLocaleString("bs-BA");
}
function context(metric: PhysicalMetric) {
  const threshold =
    metric.thresholdValue === null
      ? null
      : `${metric.thresholdScope === "TEAM" ? "Timski" : metric.thresholdScope === "PLAYER" ? "Individualni" : "Prag izvora"}: ${metric.thresholdValue} ${metric.thresholdUnitCode ?? ""}`;
  const method = metric.methodKey
    ? `Metodologija: ${metric.methodKey} ${metric.methodVersion ?? ""}`
    : null;
  return (
    [threshold, method].filter(Boolean).join(" · ") || "Standardni kontekst"
  );
}
function exactKey(metric: PhysicalMetric) {
  return [
    metric.metricCode,
    metric.thresholdValue,
    metric.thresholdUnitCode,
    metric.thresholdDirection,
    metric.thresholdScope,
    metric.methodKey,
    metric.methodVersion,
  ].join("|");
}

export function WorkloadTable({
  workloads,
}: {
  workloads: PhysicalWorkload[];
}) {
  const all = workloads.flatMap((w) => w.metrics);
  const groups = [...new Set(all.map(exactKey))];
  const [selected, setSelected] = useState(groups[0] ?? "");
  const selectedMetric = all.find((m) => exactKey(m) === selected);
  const rows = workloads.flatMap((workload) =>
    workload.metrics
      .filter((m) => exactKey(m) === selected)
      .map((metric) => ({ workload, metric })),
  );
  if (!all.length)
    return (
      <Alert>
        <AlertTitle>Nema potvrđenih fizičkih podataka</AlertTitle>
        <AlertDescription>
          Nula je prikazana kao vrijednost kada je potvrđena; nepostojeća
          metrika se ne prikazuje.
        </AlertDescription>
      </Alert>
    );
  return (
    <div className="flex flex-col gap-4">
      <div className="grid gap-3 md:grid-cols-2">
        <Select value={selected} onValueChange={(v) => setSelected(v ?? "")}>
          <SelectTrigger aria-label="Kontekst uporedivosti">
            <SelectValue>
              {selectedMetric
                ? `${labels[selectedMetric.metricCode] ?? selectedMetric.metricCode} — ${context(selectedMetric)}`
                : "Odaberite metriku"}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectGroup>
              {groups.map((key) => {
                const metric = all.find((m) => exactKey(m) === key)!;
                return (
                  <SelectItem key={key} value={key}>
                    {labels[metric.metricCode] ??
                      `Nepoznata metrika: ${metric.metricCode}`}{" "}
                    — {context(metric)}
                  </SelectItem>
                );
              })}
            </SelectGroup>
          </SelectContent>
        </Select>
        <p className="text-muted-foreground text-sm">
          Poređenje se vrši samo unutar jednog tačnog konteksta praga i
          metodologije.
        </p>
      </div>
      <div className="overflow-x-auto rounded-xl border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Igrač</TableHead>
              <TableHead>Metrika</TableHead>
              <TableHead>Kontekst uporedivosti</TableHead>
              <TableHead>Revizija / izvor</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {rows.map(({ workload, metric }) => (
              <TableRow key={`${workload.id}-${metric.metricCode}`}>
                <TableCell>{workload.playerName}</TableCell>
                <TableCell>{value(metric)}</TableCell>
                <TableCell>{context(metric)}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    <Badge variant="secondary">
                      Revizija {workload.revisionNumber}
                    </Badge>
                    <Badge variant="outline">
                      {workload.sourceSystem ?? "Nije dostupno"}
                    </Badge>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
