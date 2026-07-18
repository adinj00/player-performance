import { useQuery } from "@tanstack/react-query";
import { parseAsString, useQueryStates } from "nuqs";
import { Bar, BarChart, XAxis, YAxis } from "recharts";
import { Link } from "react-router-dom";
import { RefreshCw } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { PageHeader } from "@/components/common/page-header";
import { routePaths } from "@/app/route-paths";
import { dashboardApi, dashboardKeys } from "@/features/dashboard";
import type {
  DashboardOverview,
  DashboardWorkloadGroup,
} from "@/features/dashboard/types";
import { formatDate, formatUtcDateTime } from "@/lib/date-format";

const labels: Record<string, string> = {
  WIN: "Pobjeda",
  DRAW: "Neriješeno",
  LOSS: "Poraz",
  DRAFT: "Nacrt",
  READY_FOR_REVIEW: "Spremno za pregled",
  VERIFIED: "Verifikovano",
  NEEDS_CORRECTION: "Potrebna korekcija",
  ARCHIVED: "Arhivirano",
  AVAILABLE: "Dostupni",
  LIMITED: "Ograničeno dostupni",
  UNAVAILABLE: "Nedostupni",
  REHAB: "Rehabilitacija",
  UNKNOWN: "Nepoznato",
  GOALS: "Golovi",
  ASSISTS: "Asistencije",
  MINUTES: "Minute",
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
const alertPresentation: Record<
  string,
  { title: string; description: string; destination: string }
> = {
  REPORT_DRAFT: {
    title: "Izvještaji u nacrtu",
    description: "Postoje izvještaji koji još nisu poslani na pregled.",
    destination: routePaths.matchReports,
  },
  REPORT_READY_FOR_REVIEW: {
    title: "Izvještaji čekaju pregled",
    description: "Postoje izvještaji spremni za pregled.",
    destination: routePaths.matchReports,
  },
  REPORT_NEEDS_CORRECTION: {
    title: "Izvještaji trebaju korekciju",
    description: "Postoje izvještaji kojima je potrebna korekcija.",
    destination: routePaths.matchReports,
  },
  PLAYED_MATCH_WITHOUT_REPORT: {
    title: "Odigrane utakmice bez izvještaja",
    description: "Za neke odigrane utakmice još nije kreiran izvještaj.",
    destination: routePaths.matchReports,
  },
  IMPORT_FAILED: {
    title: "Neuspješni importi",
    description: "Postoje importi koji zahtijevaju provjeru.",
    destination: routePaths.imports,
  },
  IMPORT_VALIDATION_FAILED: {
    title: "Importi s greškom validacije",
    description: "Neki importi nisu prošli validaciju podataka.",
    destination: routePaths.imports,
  },
  AVAILABILITY_UNKNOWN: {
    title: "Nepoznata dostupnost igrača",
    description: "Za dio igrača nije evidentiran trenutni status dostupnosti.",
    destination: routePaths.medical,
  },
  WORKLOAD_COMPARABILITY_SPLIT: {
    title: "Odvojeni konteksti fizičkog opterećenja",
    description:
      "Podaci imaju različite pragove ili metodologije i ne mogu se porediti zajedno.",
    destination: routePaths.trainingGps,
  },
};
const availability = [
  ["totalPlayers", "Ukupno igrača"],
  ["availableCount", "Dostupni"],
  ["limitedCount", "Ograničeno dostupni"],
  ["unavailableCount", "Nedostupni"],
  ["rehabCount", "Rehabilitacija"],
  ["unknownCount", "Nepoznato"],
] as const;

export function DashboardPage() {
  const [state, setState] = useQueryStates({
    teamId: parseAsString,
    seasonId: parseAsString,
    leaderMetric: parseAsString,
    workloadContext: parseAsString,
    workloadMetric: parseAsString,
    workloadComparison: parseAsString,
  });
  const options = useQuery({
    queryKey: dashboardKeys.contextOptions,
    queryFn: dashboardApi.contextOptions,
    retry: false,
  });
  const team =
    options.data?.teams.find((item) => item.id === state.teamId) ??
    options.data?.teams.find((item) => item.status === "ACTIVE") ??
    options.data?.teams[0];
  const season =
    options.data?.seasons.find((item) => item.id === state.seasonId) ??
    options.data?.seasons[0];
  const shouldNormalize = Boolean(
    options.data &&
    (team?.id !== state.teamId || season?.id !== state.seasonId),
  );
  if (shouldNormalize)
    void setState(
      { teamId: team?.id ?? null, seasonId: season?.id ?? null },
      { history: "replace" },
    );
  const overview = useQuery({
    queryKey: dashboardKeys.overview(team?.id ?? "", season?.id ?? ""),
    queryFn: () => dashboardApi.overview(team!.id, season!.id),
    enabled: Boolean(team && season),
    retry: false,
  });
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Kontrolna ploča"
        description="Operativni pregled odabrane selekcije i sezone."
      />
      {options.isLoading ? (
        <Skeleton className="h-32 w-full" />
      ) : options.isError ? (
        <Retry title="Nije moguće učitati kontekst" retry={options.refetch} />
      ) : !team ? (
        <Alert>
          <AlertTitle>Nema dostupnih selekcija</AlertTitle>
          <AlertDescription>
            Trenutno nemate pristup nijednoj selekciji.
          </AlertDescription>
        </Alert>
      ) : !season ? (
        <Alert>
          <AlertTitle>Nema dostupnih sezona</AlertTitle>
          <AlertDescription>
            Dodajte sezonu prije pregleda kontrolne ploče.
          </AlertDescription>
        </Alert>
      ) : (
        <>
          <Card>
            <CardContent className="pt-6">
              <FieldGroup className="md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]">
                <Field>
                  <FieldLabel>Selekcija</FieldLabel>
                  <Select
                    value={team.id}
                    onValueChange={(teamId) =>
                      void setState({
                        teamId,
                        leaderMetric: null,
                        workloadContext: null,
                        workloadMetric: null,
                        workloadComparison: null,
                      })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue>{team.name}</SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {options.data?.teams.map((item) => (
                          <SelectItem key={item.id} value={item.id}>
                            {item.name}
                            {item.status !== "ACTIVE" ? " (neaktivna)" : ""}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                </Field>
                <Field>
                  <FieldLabel>Sezona</FieldLabel>
                  <Select
                    value={season.id}
                    onValueChange={(seasonId) =>
                      void setState({
                        seasonId,
                        leaderMetric: null,
                        workloadContext: null,
                        workloadMetric: null,
                        workloadComparison: null,
                      })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue>{season.name}</SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {options.data?.seasons.map((item) => (
                          <SelectItem key={item.id} value={item.id}>
                            {item.name}
                            {item.isArchived ? " (arhivirana)" : ""}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                </Field>
                <Field>
                  <FieldLabel className="sr-only">Osvježi pregled</FieldLabel>
                  <Button
                    variant="outline"
                    disabled={overview.isFetching}
                    onClick={() => void overview.refetch()}
                  >
                    <RefreshCw data-icon="inline-start" />
                    {overview.isFetching ? "Osvježavanje…" : "Osvježi"}
                  </Button>
                </Field>
              </FieldGroup>
              <div className="text-muted-foreground mt-3 flex flex-wrap gap-2 text-sm">
                <Badge variant="secondary">
                  {team.status === "ACTIVE"
                    ? "Aktivna selekcija"
                    : "Neaktivna selekcija"}
                </Badge>
                {season.isArchived ? (
                  <Badge variant="secondary">Arhivirana sezona</Badge>
                ) : null}
                <span>
                  {formatDate(season.startDate)} – {formatDate(season.endDate)}
                </span>
              </div>
            </CardContent>
          </Card>
          {overview.isLoading ? (
            <DashboardSkeleton />
          ) : overview.isError ? (
            <Retry
              title="Nije moguće učitati pregled"
              retry={overview.refetch}
            />
          ) : overview.data ? (
            <DashboardContent
              data={overview.data}
              state={state}
              setState={setState}
            />
          ) : null}
        </>
      )}
    </div>
  );
}

function DashboardContent({
  data,
  state,
  setState,
}: {
  data: DashboardOverview;
  state: Record<string, string | null>;
  setState: (v: Record<string, string | null>) => Promise<URLSearchParams>;
}) {
  const leader =
    data.statisticsLeaders.groups.find(
      (x) => x.metricCode === state.leaderMetric,
    ) ?? data.statisticsLeaders.groups[0];
  const groups = data.physicalWorkload.groups;
  const context =
    groups.find((x) => x.contextType === state.workloadContext) ?? groups[0];
  const metricGroups = groups.filter(
    (x) =>
      x.contextType === context?.contextType &&
      x.metricCode === (state.workloadMetric ?? context?.metricCode),
  );
  const selectedWorkload =
    metricGroups.find((x) => x.comparabilityKey === state.workloadComparison) ??
    metricGroups[0];
  return (
    <>
      <p className="text-muted-foreground text-sm">
        Pregled je generisan: {formatUtcDateTime(data.generatedAtUtc)}
      </p>
      <QualityAlerts data={data} />
      <section className="grid gap-6 xl:grid-cols-2">
        <Recent data={data} />
        <Report data={data} />
      </section>
      <section className="grid gap-6 xl:grid-cols-2">
        <Availability data={data} />
        <Leaders
          group={leader}
          emptyReason={data.statisticsLeaders.emptyReason}
          value={state.leaderMetric}
          onChange={(leaderMetric) => void setState({ leaderMetric })}
          groups={data.statisticsLeaders.groups}
        />
      </section>
      <Workloads
        data={data}
        context={context}
        metricGroups={metricGroups}
        selected={selectedWorkload}
        setState={setState}
      />
    </>
  );
}
function QualityAlerts({ data }: { data: DashboardOverview }) {
  return (
    <section aria-labelledby="quality-alerts" className="flex flex-col gap-3">
      <h2 id="quality-alerts" className="text-lg font-semibold">
        Upozorenja kvaliteta podataka
      </h2>
      {data.qualityAlerts.length ? (
        data.qualityAlerts.map((item) => {
          const copy = alertPresentation[item.code] ?? {
            title: "Upozorenje kvaliteta podataka",
            description: "Potrebna je provjera dostupnih podataka.",
            destination: null,
          };
          return (
            <Alert
              key={`${item.code}-${item.destination}`}
              variant={item.severity === "ERROR" ? "destructive" : "default"}
            >
              <AlertTitle>
                {copy.title} ({item.count})
              </AlertTitle>
              <AlertDescription className="flex flex-wrap items-center gap-3">
                <span>
                  {copy.description} Ozbiljnost:{" "}
                  {item.severity === "ERROR" ? "greška" : "upozorenje"}.
                </span>
                {copy.destination ? (
                  <Button
                    variant="link"
                    className="px-0"
                    nativeButton={false}
                    render={<Link to={copy.destination} />}
                  >
                    Otvori povezani pregled
                  </Button>
                ) : null}
              </AlertDescription>
            </Alert>
          );
        })
      ) : (
        <Alert>
          <AlertTitle>Nema otvorenih upozorenja za odabrani pregled</AlertTitle>
          <AlertDescription>
            Ovo se odnosi samo na podatke koje trenutno možete vidjeti.
          </AlertDescription>
        </Alert>
      )}
    </section>
  );
}
function Recent({ data }: { data: DashboardOverview }) {
  const chart = data.recentMatches.map((x) => ({
    name: x.opponentName,
    za: x.teamScore,
    protiv: x.opponentScore,
  }));
  return (
    <Card>
      <CardHeader>
        <CardTitle>Posljednje utakmice</CardTitle>
        <CardDescription>
          Rezultati su prikazani iz perspektive FK Velež.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {data.recentMatches.length ? (
          <>
            <div className="flex flex-col gap-2">
              {data.recentMatches.map((match) => (
                <Link
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border p-3"
                  key={match.id}
                  to={routePaths.matchDetail(match.id)}
                  aria-label={`Otvori utakmicu protiv ${match.opponentName}`}
                >
                  <span>
                    {formatDate(match.kickoffAtUtc)} · {match.competitionName} ·{" "}
                    {match.opponentName}
                  </span>
                  <span className="flex items-center gap-2">
                    <Badge variant="secondary">
                      {labels[match.result] ?? match.result}
                    </Badge>
                    <strong>
                      {match.teamScore} : {match.opponentScore}
                    </strong>
                    {match.reportStatus ? (
                      <Badge variant="outline">
                        {labels[match.reportStatus] ?? match.reportStatus}
                      </Badge>
                    ) : null}
                  </span>
                </Link>
              ))}
            </div>
            {chart.length > 1 ? (
              <Chart title="Golovi u posljednjim utakmicama" data={chart} />
            ) : null}
          </>
        ) : (
          <p className="text-muted-foreground">
            Nema odigranih utakmica u odabranoj sezoni.
          </p>
        )}
        <div className="grid grid-cols-3 gap-3 text-center">
          <Stat label="Pobjede" value={data.teamForm.wins} />
          <Stat label="Neriješeno" value={data.teamForm.draws} />
          <Stat label="Porazi" value={data.teamForm.losses} />
        </div>
        <p
          aria-label="Forma, od najnovije prema najstarijoj"
          className="flex flex-wrap gap-2"
        >
          {data.teamForm.form.map((item) => (
            <Badge key={item.matchId} variant="secondary">
              {labels[item.result] ?? item.result}
            </Badge>
          ))}
        </p>
      </CardContent>
    </Card>
  );
}
function Report({ data }: { data: DashboardOverview }) {
  const report = data.reportWorkflow;
  return (
    <Card>
      <CardHeader>
        <CardTitle>Izvještaji utakmica</CardTitle>
        <CardDescription>
          {report.visibilityMode === "FULL_WORKFLOW"
            ? "Pregled radnog toka izvještaja."
            : "Prikazani su samo završni izvještaji dostupni vašoj ulozi."}
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {report.visibilityMode === "FULL_WORKFLOW" ? (
          <>
            {report.playedMatchCount !== undefined ? (
              <Stat label="Odigrane utakmice" value={report.playedMatchCount} />
            ) : null}
            {report.missingReportCount !== undefined ? (
              <Stat
                label="Nedostajući izvještaji"
                value={report.missingReportCount}
              />
            ) : null}
          </>
        ) : null}
        {report.statusCounts.map((item) => (
          <div className="flex justify-between" key={item.status}>
            <span>{labels[item.status] ?? item.status}</span>
            <strong>{item.count}</strong>
          </div>
        ))}
        <Button
          variant="outline"
          className="self-start"
          nativeButton={false}
          render={<Link to={routePaths.matchReports} />}
        >
          Otvori izvještaje utakmica
        </Button>
      </CardContent>
    </Card>
  );
}
function Availability({ data }: { data: DashboardOverview }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Trenutna dostupnost</CardTitle>
        <CardDescription>
          Stanje na dan {formatDate(data.availability.asOfDate)}. Ovo je
          trenutni snimak, a ne historijsko stanje odabrane sezone.
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {availability.map(([key, label]) => (
          <Link key={key} to={routePaths.medical}>
            <Stat label={label} value={data.availability[key]} />
          </Link>
        ))}
      </CardContent>
    </Card>
  );
}
function Leaders({
  group,
  groups,
  emptyReason,
  value,
  onChange,
}: {
  group: DashboardOverview["statisticsLeaders"]["groups"][number] | undefined;
  groups: DashboardOverview["statisticsLeaders"]["groups"];
  emptyReason: string | null;
  value: string | null;
  onChange: (v: string) => void;
}) {
  const reason: Record<string, string> = {
    NO_FINAL_REPORTS:
      "Nema verifikovanih ili arhiviranih izvještaja za odabrani kontekst.",
    NO_ELIGIBLE_STATISTICS:
      "Završni izvještaji ne sadrže podatke za ovu metriku.",
    NO_LEADERS: "Nema igrača koji ispunjavaju uslove za prikaz lidera.",
  };
  return (
    <Card>
      <CardHeader>
        <CardTitle>Lideri statistike</CardTitle>
        <CardDescription>
          Prikazuju se samo agregati iz završnih izvještaja.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {groups.length ? (
          <>
            <Field>
              <FieldLabel>Metrika</FieldLabel>
              <Select
                value={group?.metricCode ?? value ?? ""}
                onValueChange={(v) => onChange(v ?? "")}
              >
                <SelectTrigger>
                  <SelectValue>
                    {labels[group?.metricCode ?? ""] ?? group?.metricCode}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {groups.map((item) => (
                      <SelectItem key={item.metricCode} value={item.metricCode}>
                        {labels[item.metricCode] ?? item.metricCode}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            {group ? (
              <>
                <Chart
                  title={`${labels[group.metricCode] ?? group.metricCode} po igraču`}
                  data={group.leaders.map((x) => ({
                    name: x.player.displayName,
                    vrijednost: x.value,
                  }))}
                  horizontal
                />
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Rang</TableHead>
                      <TableHead>Igrač</TableHead>
                      <TableHead>Vrijednost</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {group.leaders.map((x) => (
                      <TableRow key={x.player.id}>
                        <TableCell>{x.rank}</TableCell>
                        <TableCell>
                          <Link
                            className="underline"
                            to={`${routePaths.players}?playerId=${x.player.id}`}
                          >
                            {x.player.displayName}
                          </Link>
                        </TableCell>
                        <TableCell>{x.value}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                {group.hasAdditionalTies ? (
                  <p className="text-muted-foreground text-sm">
                    Prikaz je ograničen; dodatni igrači dijele posljednji
                    prikazani rang.
                  </p>
                ) : null}
              </>
            ) : null}
          </>
        ) : (
          <p className="text-muted-foreground">
            {reason[emptyReason ?? ""] ?? "Nema dostupnih lidera statistike."}
          </p>
        )}
      </CardContent>
    </Card>
  );
}
function Workloads({
  data,
  context,
  metricGroups,
  selected,
  setState,
}: {
  data: DashboardOverview;
  context: DashboardWorkloadGroup | undefined;
  metricGroups: DashboardWorkloadGroup[];
  selected: DashboardWorkloadGroup | undefined;
  setState: (v: Record<string, string | null>) => Promise<URLSearchParams>;
}) {
  const contexts = [
    ...new Set(data.physicalWorkload.groups.map((x) => x.contextType)),
  ];
  const metrics = data.physicalWorkload.groups.filter(
    (x) => x.contextType === context?.contextType,
  );
  return (
    <Card>
      <CardHeader>
        <CardTitle>Fizičko opterećenje</CardTitle>
        <CardDescription>
          Samo potvrđeni podaci; različiti pragovi i metodologije ostaju
          odvojeni.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {data.physicalWorkload.groups.length ? (
          <>
            <FieldGroup className="md:grid-cols-3">
              <Selector
                label="Kontekst"
                value={context?.contextType ?? ""}
                options={contexts}
                onChange={(workloadContext) =>
                  void setState({
                    workloadContext,
                    workloadMetric: null,
                    workloadComparison: null,
                  })
                }
              />
              <Selector
                label="Metrika"
                value={selected?.metricCode ?? context?.metricCode ?? ""}
                options={[...new Set(metrics.map((x) => x.metricCode))]}
                formatter={(x) => labels[x] ?? x}
                onChange={(workloadMetric) =>
                  void setState({ workloadMetric, workloadComparison: null })
                }
              />
              <Selector
                label="Uporedivost"
                value={selected?.comparabilityKey ?? ""}
                options={metricGroups.map((x) => x.comparabilityKey)}
                formatter={(x) =>
                  `Kontekst ${metricGroups.find((g) => g.comparabilityKey === x)?.thresholdContext.value ?? "standardni"}`
                }
                onChange={(workloadComparison) =>
                  void setState({ workloadComparison })
                }
              />
            </FieldGroup>
            {data.physicalWorkload.hasMultipleComparabilityContexts ? (
              <Alert>
                <AlertTitle>Više konteksta uporedivosti</AlertTitle>
                <AlertDescription>
                  Grupe nisu spojene jer imaju različite pragove ili
                  metodologije.
                </AlertDescription>
              </Alert>
            ) : null}
            {selected ? (
              <>
                <section className="grid gap-3 sm:grid-cols-3">
                  <Stat
                    label={labels[selected.metricCode] ?? selected.metricCode}
                    value={formatWorkload(selected)}
                  />
                  <Stat label="Broj zapisa" value={selected.workloadCount} />
                  <Stat label="Igrača" value={selected.playerCount} />
                </section>
                <p className="text-muted-foreground text-sm">
                  Agregacija:{" "}
                  {selected.aggregationKind === "SUM" ? "zbir" : "maksimum"}.
                  Prag: {selected.thresholdContext.value ?? "standardni"}{" "}
                  {selected.thresholdContext.unitCode ?? ""}. Metodologija:{" "}
                  {selected.methodContext.key ?? "nije navedena"}{" "}
                  {selected.methodContext.version ?? ""}.
                </p>
                <ScrollArea className="h-44 rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Period</TableHead>
                        <TableHead>Kontekst</TableHead>
                        <TableHead>Zapisi</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      <TableRow>
                        <TableCell>
                          {formatDate(selected.firstOccurredOn)} –{" "}
                          {formatDate(selected.lastOccurredOn)}
                        </TableCell>
                        <TableCell>{selected.contextType}</TableCell>
                        <TableCell>{selected.contextCount}</TableCell>
                      </TableRow>
                    </TableBody>
                  </Table>
                </ScrollArea>
              </>
            ) : null}
            {data.physicalWorkload.truncated ? (
              <p className="text-muted-foreground text-sm">
                Prikazano {data.physicalWorkload.returnedGroupCount} od{" "}
                {data.physicalWorkload.availableGroupCount} dostupnih grupa.
              </p>
            ) : null}
          </>
        ) : (
          <p className="text-muted-foreground">
            Nema potvrđenih fizičkih podataka; pregledi i importi bez potvrde
            nisu službeni podaci.
          </p>
        )}
        <Button
          variant="outline"
          className="self-start"
          nativeButton={false}
          render={<Link to={routePaths.trainingGps} />}
        >
          Otvori treninge i GPS
        </Button>
      </CardContent>
    </Card>
  );
}
function Selector({
  label,
  value,
  options,
  formatter = (x: string) => x,
  onChange,
}: {
  label: string;
  value: string;
  options: string[];
  formatter?: (x: string) => string;
  onChange: (x: string) => void;
}) {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Select value={value} onValueChange={(x) => onChange(x ?? "")}>
        <SelectTrigger>
          <SelectValue>{formatter(value)}</SelectValue>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            {options.map((x) => (
              <SelectItem key={x} value={x}>
                {formatter(x)}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </Field>
  );
}
const goalsChartConfig = {
  za: { label: "FK Velež", color: "var(--chart-1)" },
  protiv: { label: "Protivnik", color: "var(--chart-2)" },
} satisfies ChartConfig;
const leadersChartConfig = {
  vrijednost: { label: "Vrijednost", color: "var(--chart-1)" },
} satisfies ChartConfig;

function Chart({
  title,
  data,
  horizontal = false,
}: {
  title: string;
  data: { name: string; za?: number; protiv?: number; vrijednost?: number }[];
  horizontal?: boolean;
}) {
  return (
    <figure aria-labelledby={`${title}-caption`}>
      <figcaption id={`${title}-caption`} className="sr-only">
        {title}. Tablica ili lista iznad sadrži isti podatak.
      </figcaption>
      <ChartContainer
        className="h-64 w-full"
        config={horizontal ? leadersChartConfig : goalsChartConfig}
      >
        <BarChart data={data} layout={horizontal ? "vertical" : "horizontal"}>
          <XAxis
            type={horizontal ? "number" : "category"}
            dataKey={horizontal ? undefined : "name"}
          />
          <YAxis
            type={horizontal ? "category" : "number"}
            dataKey={horizontal ? "name" : undefined}
            width={horizontal ? 110 : undefined}
          />
          <ChartTooltip content={<ChartTooltipContent />} />
          <Bar
            dataKey={horizontal ? "vrijednost" : "za"}
            fill={horizontal ? "var(--color-vrijednost)" : "var(--color-za)"}
          />
          {!horizontal ? (
            <Bar dataKey="protiv" fill="var(--color-protiv)" />
          ) : null}
        </BarChart>
      </ChartContainer>
    </figure>
  );
}
function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded-md border p-3">
      <p className="text-muted-foreground text-sm">{label}</p>
      <p className="text-xl font-semibold">{value}</p>
    </div>
  );
}
function formatWorkload(group: DashboardWorkloadGroup) {
  const value = group.aggregateValue.toLocaleString("bs-BA");
  return group.unitCode === "METERS"
    ? `${value} m`
    : group.unitCode === "METERS_PER_SECOND"
      ? `${value} m/s`
      : group.unitCode === "SECONDS"
        ? `${value} s`
        : group.unitCode === "ARBITRARY_UNITS"
          ? `${value} AU`
          : value;
}
function Retry({ title, retry }: { title: string; retry: () => unknown }) {
  return (
    <Alert variant="destructive">
      <AlertTitle>{title}</AlertTitle>
      <AlertDescription>
        <Button variant="outline" onClick={() => void retry()}>
          Pokušaj ponovo
        </Button>
      </AlertDescription>
    </Alert>
  );
}
function DashboardSkeleton() {
  return (
    <div className="grid gap-6 xl:grid-cols-2">
      <Skeleton className="h-96 w-full" />
      <Skeleton className="h-96 w-full" />
      <Skeleton className="h-96 w-full" />
      <Skeleton className="h-96 w-full" />
    </div>
  );
}
