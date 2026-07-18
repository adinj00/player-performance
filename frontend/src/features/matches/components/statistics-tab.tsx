import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type CellContext,
  type ColumnDef,
} from "@tanstack/react-table";
import { Plus, Save, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { invalidateDashboardOverview } from "@/features/dashboard";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Input } from "@/components/ui/input";
import {
  Progress,
  ProgressLabel,
  ProgressValue,
} from "@/components/ui/progress";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useSession } from "@/features/auth/hooks/use-session";
import { matchesApi } from "@/features/matches/api/matches-api";
import type {
  MatchResponse,
  MatchReportStatisticsResponse,
  StatisticsAppearance,
} from "@/features/matches/types/match";
import {
  createStatisticsFormValues,
  createStatisticsPayload,
  isComplete,
  isKnownStatisticField,
  parseNullableInteger,
  statisticFieldRegistry,
  type StatisticsFormRow,
  type StatisticsFormValues,
  validateStatisticRelationship,
} from "@/features/matches/utils/statistics";
import { isApiError } from "@/lib/api/api-client";

const trackingLabels = {
  BASIC: "Osnovni",
  STANDARD: "Standardni",
  FULL: "Puni",
} as const;
const statusLabels = {
  DRAFT: "Nacrt",
  READY_FOR_REVIEW: "Spremno za pregled",
  VERIFIED: "Potvrđeno",
  NEEDS_CORRECTION: "Potrebna ispravka",
  ARCHIVED: "Arhivirano",
} as const;
const nameOf = (a: StatisticsAppearance | undefined) =>
  a ? a.preferredName || `${a.firstName} ${a.lastName}` : "Odaberite igrača";
const canCreate = (role: string | null | undefined) =>
  role === "ADMIN" || role === "DATA_OPERATOR";
const fields = (codes: string[]) =>
  codes
    .filter(isKnownStatisticField)
    .sort(
      (a, b) =>
        statisticFieldRegistry[a].order - statisticFieldRegistry[b].order,
    );

export function StatisticsTab({ match }: { match: MatchResponse }) {
  const client = useQueryClient();
  const { user } = useSession();
  const report = useQuery({
    queryKey: ["match-report", match.id],
    queryFn: () => matchesApi.getReport(match.id),
    enabled: match.status === "PLAYED",
    retry: false,
  });
  const statistics = useQuery({
    queryKey: ["match-statistics", report.data?.id],
    queryFn: () => matchesApi.getStatistics(report.data!.id),
    enabled: !!report.data?.id,
    retry: false,
  });
  const create = useMutation({
    mutationFn: () => matchesApi.createReport(match.id),
    onSuccess: async () => {
      await client.invalidateQueries({ queryKey: ["match-report", match.id] });
      await invalidateDashboardOverview(client, match.team.id);
      toast.success("Izvještaj je započet.");
    },
    onError: async (error) => {
      if (isApiError(error) && error.status === 409) {
        await client.invalidateQueries({
          queryKey: ["match-report", match.id],
        });
        return;
      }
      toast.error(
        isApiError(error) ? error.message : "Izvještaj nije moguće započeti.",
      );
    },
  });
  if (match.status !== "PLAYED") return <Unavailable status={match.status} />;
  if (match.isArchived && !report.data) return <Unavailable archived />;
  if (report.isLoading || statistics.isLoading)
    return <Skeleton className="h-80 w-full" />;
  if (report.isError)
    return (
      <Alert variant="destructive">
        <AlertTitle>Izvještaj nije moguće učitati</AlertTitle>
        <AlertDescription>
          Provjerite vezu i pokušajte ponovo.{" "}
          <Button variant="link" onClick={() => report.refetch()}>
            Pokušajte ponovo
          </Button>
        </AlertDescription>
      </Alert>
    );
  if (!report.data)
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>Izvještaj utakmice još nije dostupan</EmptyTitle>
          <EmptyDescription>
            Statistika je dostupna nakon pokretanja izvještaja.
          </EmptyDescription>
          {canCreate(user?.primaryRole) && !match.isArchived ? (
            <Button onClick={() => create.mutate()} disabled={create.isPending}>
              Započni izvještaj
            </Button>
          ) : null}
        </EmptyHeader>
      </Empty>
    );
  if (statistics.isError || !statistics.data)
    return (
      <Alert variant="destructive">
        <AlertTitle>Statistiku nije moguće učitati</AlertTitle>
        <AlertDescription>
          Provjerite vezu i pokušajte ponovo.{" "}
          <Button variant="link" onClick={() => statistics.refetch()}>
            Pokušajte ponovo
          </Button>
        </AlertDescription>
      </Alert>
    );
  return <Editor match={match} data={statistics.data} />;
}

function Unavailable({
  status,
  archived,
}: {
  status?: MatchResponse["status"];
  archived?: boolean;
}) {
  const message = archived
    ? "Arhivirana utakmica nema dostupan izvještaj statistike."
    : status === "SCHEDULED"
      ? "Statistika se unosi nakon što je utakmica označena kao odigrana."
      : status === "POSTPONED"
        ? "Statistika se može unijeti nakon što se odgođena utakmica odigra."
        : "Statistika nije dostupna za otkazanu utakmicu.";
  return (
    <Empty>
      <EmptyHeader>
        <EmptyTitle>Statistika nije dostupna</EmptyTitle>
        <EmptyDescription>{message}</EmptyDescription>
      </EmptyHeader>
    </Empty>
  );
}

function Editor({
  match,
  data,
}: {
  match: MatchResponse;
  data: MatchReportStatisticsResponse;
}) {
  const client = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [discard, setDiscard] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const form = useForm<StatisticsFormValues>({
    defaultValues: createStatisticsFormValues(data),
  });
  const players = useWatch({ control: form.control, name: "players" }) ?? [];
  const keepers =
    useWatch({ control: form.control, name: "goalkeepers" }) ?? [];
  const keeperArray = useFieldArray({
    control: form.control,
    name: "goalkeepers",
  });
  const addGoalkeeper = (playerMatchAppearanceId: string) => {
    setEditing(true);
    keeperArray.append({
      playerMatchAppearanceId,
      values: Object.fromEntries(
        data.enabledGoalkeeperFields.map((field) => [field, ""]),
      ),
    });
  };
  const removeGoalkeeper = (index: number) => keeperArray.remove(index);
  useEffect(() => {
    if (!editing) form.reset(createStatisticsFormValues(data));
  }, [data, editing, form]);
  const unknown = [
    ...data.enabledPlayerFields,
    ...data.enabledGoalkeeperFields,
  ].filter((field) => !isKnownStatisticField(field));
  const playerFields = fields(data.enabledPlayerFields);
  const keeperFields = fields(data.enabledGoalkeeperFields);
  const editable =
    data.canEditStatistics && !match.isArchived && unknown.length === 0;
  const save = useMutation({
    mutationFn: (value: StatisticsFormValues) =>
      matchesApi.saveStatistics(
        data.reportId,
        createStatisticsPayload(value, data),
      ),
    onSuccess: async (response) => {
      form.reset(createStatisticsFormValues(response));
      client.setQueryData(["match-statistics", data.reportId], response);
      setEditing(false);
      await Promise.all([
        client.invalidateQueries({
          queryKey: ["match-statistics", data.reportId],
        }),
        client.invalidateQueries({ queryKey: ["match-report", match.id] }),
        invalidateDashboardOverview(client, match.team.id),
      ]);
      toast.success("Statistika je sačuvana.");
    },
    onError: async (cause) => {
      if (isApiError(cause) && cause.status === 409) {
        setEditing(false);
        await client.invalidateQueries({
          queryKey: ["match-statistics", data.reportId],
        });
        setError(
          "Statistika je zaključana ili je sastav promijenjen. Podaci su ponovno učitani; nesačuvane vrijednosti nisu primijenjene.",
        );
        return;
      }
      setError(
        isApiError(cause)
          ? (cause.detail ?? cause.message)
          : "Statistiku nije moguće sačuvati.",
      );
    },
  });
  const submit = form.handleSubmit((value) => {
    for (const row of [...value.players, ...value.goalkeepers]) {
      for (const [field, entry] of Object.entries(row.values)) {
        if (
          field !== "cleanSheet" &&
          parseNullableInteger(entry) === undefined
        ) {
          setError(
            "Statističke vrijednosti moraju biti nenegativni cijeli brojevi.",
          );
          return;
        }
      }
    }
    for (const row of value.players) {
      const relationship = validateStatisticRelationship(row.values);
      if (relationship) {
        setError(relationship);
        return;
      }
    }
    setError(null);
    save.mutate(value);
  });
  const completePlayers = players.filter((row) =>
    isComplete(row.values, data.enabledPlayerFields),
  ).length;
  const keeperRows: StatisticsFormRow[] = keeperArray.fields.map(
    (field, index) => ({
      playerMatchAppearanceId: field.playerMatchAppearanceId,
      values: keepers[index]?.values ?? field.values,
    }),
  );
  const selectedKeepers = keeperRows.filter(
    (row) => row.playerMatchAppearanceId,
  );
  const goalkeeperAppearanceIds = new Set(
    selectedKeepers.map((row) => row.playerMatchAppearanceId),
  );
  const completeKeepers = selectedKeepers.filter((row) =>
    isComplete(row.values, data.enabledGoalkeeperFields),
  ).length;
  const total = players.length + selectedKeepers.length;
  const percent = total
    ? Math.round(((completePlayers + completeKeepers) / total) * 100)
    : 0;
  const columns = useMemo<ColumnDef<StatisticsFormRow>[]>(
    () => [
      {
        accessorKey: "playerMatchAppearanceId",
        header: "Igrač",
        cell: ({ row }: CellContext<StatisticsFormRow, unknown>) => (
          <Identity
            appearance={data.appearances.find(
              (a) =>
                a.playerMatchAppearanceId ===
                row.original.playerMatchAppearanceId,
            )!}
            complete={isComplete(row.original.values, data.enabledPlayerFields)}
          />
        ),
      },
      ...playerFields.map((field) => ({
        id: field,
        header: () => <Header field={field} />,
        cell: ({ row }: CellContext<StatisticsFormRow, unknown>) => (
          <NumberCell
            editing={editing}
            field={field}
            path={`players.${row.index}.values.${field}`}
            form={form}
            name={nameOf(data.appearances[row.index])}
          />
        ),
      })),
    ],
    [data.appearances, data.enabledPlayerFields, editing, form, playerFields],
  );
  // TanStack Table creates a mutable table instance; React Compiler must not memoize it.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: players,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });
  if (unknown.length)
    return (
      <Alert variant="destructive">
        <AlertTitle>Nepodržano polje statistike</AlertTitle>
        <AlertDescription>
          Uređivanje je blokirano: {unknown.join(", ")}.
        </AlertDescription>
      </Alert>
    );
  if (!data.appearances.length)
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>Nema evidentiranih nastupa</EmptyTitle>
          <EmptyDescription>
            Prije unosa statistike potrebno je evidentirati nastupe igrača u
            tabu Sastav.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  return (
    <div className="flex flex-col gap-5">
      <section className="flex flex-col gap-3 rounded-xl border p-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h2 className="font-heading text-xl">Statistika</h2>
              <Badge variant="secondary">
                {statusLabels[data.reportStatus]}
              </Badge>
            </div>
            <p className="text-muted-foreground text-sm">
              Nivo praćenja:{" "}
              {data.appliedTrackingLevel
                ? trackingLabels[data.appliedTrackingLevel]
                : "Nije određen"}
            </p>
          </div>
          {editable && !editing ? (
            <Button onClick={() => setEditing(true)}>Uredi statistiku</Button>
          ) : null}
        </div>
        <Progress value={percent}>
          <ProgressLabel>Kompletnost statistike</ProgressLabel>
          <ProgressValue />
        </Progress>
        <p className="text-muted-foreground text-sm">
          Igrači: {completePlayers}/{players.length} · Golmani:{" "}
          {completeKeepers}/{keeperRows.length}
          {keeperRows.length === 0
            ? " — golmanska statistika nije unesena"
            : ""}
        </p>
        {!editable && !match.isArchived ? (
          <Alert>
            <AlertTitle>Statistika je zaključana</AlertTitle>
            <AlertDescription>
              Statistika je zaključana zbog trenutnog statusa izvještaja
              utakmice.
            </AlertDescription>
          </Alert>
        ) : null}
      </section>
      {error ? (
        <Alert variant="destructive">
          <AlertTitle>Provjerite unesene podatke</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}
      <form onSubmit={submit} className="flex flex-col gap-4">
        <Tabs defaultValue="players">
          <TabsList>
            <TabsTrigger value="players">
              Igrači (uključujući golmane)
            </TabsTrigger>
            <TabsTrigger value="keepers">Golmani (dodatno)</TabsTrigger>
          </TabsList>
          <TabsContent value="players">
            <div className="overflow-x-auto rounded-xl border">
              <Table className="min-w-max">
                <TableHeader>
                  {table.getHeaderGroups().map((group) => (
                    <TableRow key={group.id}>
                      {group.headers.map((header) => (
                        <TableHead key={header.id}>
                          {header.isPlaceholder
                            ? null
                            : flexRender(
                                header.column.columnDef.header,
                                header.getContext(),
                              )}
                        </TableHead>
                      ))}
                    </TableRow>
                  ))}
                </TableHeader>
                <TableBody>
                  {table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id}>
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id}>
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext(),
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </TabsContent>
          <TabsContent value="keepers">
            <div className="flex flex-col gap-3">
              {editable ? (
                <div className="flex justify-end">
                  <Select
                    onValueChange={(value) => {
                      if (typeof value === "string") addGoalkeeper(value);
                    }}
                  >
                    <SelectTrigger size="sm">
                      <Plus data-icon="inline-start" />
                      <SelectValue placeholder="Dodaj golmana" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {data.appearances
                          .filter(
                            (appearance) =>
                              !goalkeeperAppearanceIds.has(
                                appearance.playerMatchAppearanceId,
                              ),
                          )
                          .map((appearance) => (
                            <SelectItem
                              key={appearance.playerMatchAppearanceId}
                              value={appearance.playerMatchAppearanceId}
                            >
                              {nameOf(appearance)}
                            </SelectItem>
                          ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                </div>
              ) : null}
              {keeperRows.length === 0 ? (
                <Empty>
                  <EmptyHeader>
                    <EmptyTitle>Golmanska statistika nije unesena</EmptyTitle>
                    <EmptyDescription>
                      Dodajte evidentirani nastup za golmansku statistiku.
                      Golmanska statistika dopunjuje statistiku igrača za taj
                      nastup.
                    </EmptyDescription>
                  </EmptyHeader>
                </Empty>
              ) : (
                <KeeperGrid
                  appearances={data.appearances}
                  fields={keeperFields}
                  rows={keeperRows}
                  editing={editing}
                  form={form}
                  onRemove={removeGoalkeeper}
                />
              )}
            </div>
          </TabsContent>
        </Tabs>
        {editing ? (
          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() =>
                form.formState.isDirty ? setDiscard(true) : setEditing(false)
              }
            >
              Odustani
            </Button>
            <Button type="submit" disabled={save.isPending}>
              <Save data-icon="inline-start" />
              Sačuvaj statistiku
            </Button>
          </div>
        ) : null}
      </form>
      <Dialog open={discard} onOpenChange={setDiscard}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>Odbaciti promjene?</DialogTitle>
            <DialogDescription>
              Imate nesačuvane promjene statistike. Želite li ih odbaciti?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDiscard(false)}>
              Nastavi uređivanje
            </Button>
            <Button
              variant="destructive"
              onClick={() => {
                form.reset(createStatisticsFormValues(data));
                setDiscard(false);
                setEditing(false);
              }}
            >
              Odbaci promjene
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function Header({ field }: { field: keyof typeof statisticFieldRegistry }) {
  const item = statisticFieldRegistry[field];
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger aria-label={item.label}>
          {item.shortLabel}
        </TooltipTrigger>
        <TooltipContent>
          {item.label}: {item.description}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}
function Identity({
  appearance,
  complete,
}: {
  appearance: StatisticsAppearance | undefined;
  complete: boolean;
}) {
  if (!appearance)
    return (
      <div className="min-w-48">
        <p className="font-medium">Nastup nije dostupan</p>
        <p className="text-muted-foreground text-xs">Podaci se osvježavaju</p>
      </div>
    );

  return (
    <div className="min-w-48">
      <p className="font-medium">{nameOf(appearance)}</p>
      <p className="text-muted-foreground text-xs">
        {appearance.minutesPlayed} min ·{" "}
        {complete ? "Kompletno" : "Nedostaju podaci"}
      </p>
    </div>
  );
}
function NumberCell({
  editing,
  field,
  path,
  form,
  name,
}: {
  editing: boolean;
  field: keyof typeof statisticFieldRegistry;
  path:
    | `players.${number}.values.${string}`
    | `goalkeepers.${number}.values.${string}`;
  form: ReturnType<typeof useForm<StatisticsFormValues>>;
  name: string;
}) {
  if (!editing) {
    const value = form.getValues(path);
    const displayValue =
      field === "cleanSheet"
        ? value === "true"
          ? "Da"
          : value === "false"
            ? "Ne"
            : "—"
        : value || "—";
    return <span className="font-mono">{displayValue}</span>;
  }
  return (
    <Input
      aria-label={`${name}, ${statisticFieldRegistry[field].label}`}
      inputMode="numeric"
      className="w-20 font-mono"
      {...form.register(path)}
    />
  );
}
function KeeperGrid({
  appearances,
  fields,
  rows,
  editing,
  form,
  onRemove,
}: {
  appearances: StatisticsAppearance[];
  fields: Array<keyof typeof statisticFieldRegistry>;
  rows: StatisticsFormRow[];
  editing: boolean;
  form: ReturnType<typeof useForm<StatisticsFormValues>>;
  onRemove: (index: number) => void;
}) {
  return (
    <div className="overflow-x-auto rounded-xl border">
      <Table className="min-w-max">
        <TableHeader>
          <TableRow>
            <TableHead>Golman</TableHead>
            {fields.map((field) => (
              <TableHead key={field}>
                <Header field={field} />
              </TableHead>
            ))}
            {editing ? (
              <TableHead>
                <span className="sr-only">Radnje</span>
              </TableHead>
            ) : null}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row, index) => (
            <TableRow key={row.playerMatchAppearanceId || `new-${index}`}>
              <TableCell>
                <Identity
                  appearance={appearances.find(
                    (a) =>
                      a.playerMatchAppearanceId === row.playerMatchAppearanceId,
                  )}
                  complete={isComplete(row.values, fields)}
                />
              </TableCell>
              {fields.map((field) => (
                <TableCell key={field}>
                  {field === "cleanSheet" && editing ? (
                    <Select
                      value={row.values.cleanSheet || null}
                      onValueChange={(value) =>
                        form.setValue(
                          `goalkeepers.${index}.values.cleanSheet`,
                          value ?? "",
                          { shouldDirty: true },
                        )
                      }
                    >
                      <SelectTrigger
                        aria-label={`${nameOf(appearances.find((a) => a.playerMatchAppearanceId === row.playerMatchAppearanceId) ?? appearances[0])}, Sačuvana mreža`}
                        className="w-32"
                      >
                        <SelectValue>
                          {row.values.cleanSheet === "true"
                            ? "Da"
                            : row.values.cleanSheet === "false"
                              ? "Ne"
                              : "Nije uneseno"}
                        </SelectValue>
                      </SelectTrigger>
                      <SelectContent>
                        <SelectGroup>
                          <SelectItem value="true">Da</SelectItem>
                          <SelectItem value="false">Ne</SelectItem>
                        </SelectGroup>
                      </SelectContent>
                    </Select>
                  ) : (
                    <NumberCell
                      editing={editing}
                      field={field}
                      path={`goalkeepers.${index}.values.${field}`}
                      form={form}
                      name={nameOf(
                        appearances.find(
                          (appearance) =>
                            appearance.playerMatchAppearanceId ===
                            row.playerMatchAppearanceId,
                        ),
                      )}
                    />
                  )}
                </TableCell>
              ))}
              {editing ? (
                <TableCell>
                  <Button
                    type="button"
                    variant="destructive"
                    size="icon-xs"
                    aria-label="Ukloni golmansku statistiku"
                    onClick={() => onRemove(index)}
                  >
                    <Trash2 />
                  </Button>
                </TableCell>
              ) : null}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
