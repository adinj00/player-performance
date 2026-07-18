import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table";
import { parseAsInteger, parseAsString, useQueryStates } from "nuqs";
import { useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
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
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { PageHeader } from "@/components/common/page-header";
import { DatePicker } from "@/components/common/date-picker";
import { FilterSelect } from "@/components/common/filter-select";
import { useSession } from "@/features/auth/hooks/use-session";
import { invalidateDashboardOverview } from "@/features/dashboard";
import {
  AuditError,
  AuditHistory,
  AuditLoading,
  AuditPagination,
} from "@/features/audit";
import { settingsApi } from "@/features/settings/api";
import { formatDate } from "@/lib/date-format";
import { isApiError } from "@/lib/api/api-client";
import {
  medicalApi,
  type AvailabilityItem,
  type AvailabilityStatus,
} from "./api";

const statuses: Record<AvailabilityStatus, string> = {
  AVAILABLE: "Dostupan",
  LIMITED: "Ograničeno dostupan",
  UNAVAILABLE: "Nedostupan",
  REHAB: "Rehabilitacija",
  UNKNOWN: "Nepoznato",
};
const cards: {
  status: AvailabilityStatus | null;
  label: string;
  key:
    | "totalPlayers"
    | "availableCount"
    | "limitedCount"
    | "unavailableCount"
    | "rehabCount"
    | "unknownCount";
}[] = [
  { status: null, label: "Ukupno igrača", key: "totalPlayers" },
  { status: "AVAILABLE", label: "Dostupni", key: "availableCount" },
  { status: "LIMITED", label: "Ograničeno dostupni", key: "limitedCount" },
  { status: "UNAVAILABLE", label: "Nedostupni", key: "unavailableCount" },
  { status: "REHAB", label: "Rehabilitacija", key: "rehabCount" },
  { status: "UNKNOWN", label: "Nepoznato", key: "unknownCount" },
];
const today = new Date().toISOString().slice(0, 10);
const queryKey = ["medical", "restricted"] as const;

export function MedicalPage() {
  const { user } = useSession();
  const availabilityTabRef = useRef<HTMLButtonElement>(null);
  const canDetail =
    user?.primaryRole === "ADMIN" ||
    user?.permissions.canViewMedicalDetails === true;
  const [url, setUrl] = useQueryStates({
    teamId: parseAsString,
    medicalView: parseAsString.withDefault("availability"),
    availabilityStatus: parseAsString,
    availabilitySearch: parseAsString,
    availabilityPage: parseAsInteger.withDefault(1),
    availabilityPlayerId: parseAsString,
    availabilityDetailView: parseAsString,
    availabilityAuditPage: parseAsInteger.withDefault(1),
    injuryStatus: parseAsString,
    injuryPlayerId: parseAsString,
    injuryOccurredFrom: parseAsString,
    injuryOccurredTo: parseAsString,
    injuryPage: parseAsInteger.withDefault(1),
    injuryId: parseAsString,
    injuryDetailView: parseAsString.withDefault("details"),
    injuryRevisionPage: parseAsInteger.withDefault(1),
    injuryAuditPage: parseAsInteger.withDefault(1),
    injuryCreatePlayerId: parseAsString,
  });
  const teams = useQuery({
    queryKey: ["settings", "teams", false],
    queryFn: () => settingsApi.listTeams(false),
    retry: false,
  });
  const accessible = (teams.data ?? []).filter(
    (team) =>
      team.status === "ACTIVE" &&
      (user?.primaryRole === "ADMIN" ||
        user?.teamScope.type === "ALL_TEAMS" ||
        user?.teamScope.selectedTeamIds.includes(team.id)),
  );
  const teamId = accessible.some((x) => x.id === url.teamId)
    ? url.teamId
    : null;
  useEffect(() => {
    if (!url.teamId && accessible.length === 1)
      void setUrl({ teamId: accessible[0].id });
  }, [accessible, setUrl, url.teamId]);
  useEffect(() => {
    if (url.medicalView === "injuries" && !canDetail) {
      void setUrl({
        medicalView: "availability",
        injuryStatus: null,
        injuryPlayerId: null,
        injuryOccurredFrom: null,
        injuryOccurredTo: null,
        injuryPage: 1,
        injuryId: null,
        injuryDetailView: "details",
        injuryRevisionPage: 1,
        injuryAuditPage: 1,
        injuryCreatePlayerId: null,
      });
      window.requestAnimationFrame(() => availabilityTabRef.current?.focus());
    }
  }, [canDetail, setUrl, url.medicalView]);
  const client = useQueryClient();
  useEffect(() => {
    if (!canDetail || !teamId) {
      void client.cancelQueries({ queryKey });
      client.removeQueries({ queryKey });
    }
  }, [canDetail, client, teamId]);
  if (!teamId)
    return (
      <div className="flex flex-col gap-6">
        <PageHeader
          title="Dostupnost igrača"
          description="Odaberite selekciju za pregled dostupnosti."
        />
        <TeamSelect
          teams={accessible}
          value={url.teamId}
          onChange={(id) =>
            void setUrl({
              teamId: id,
              availabilityStatus: null,
              availabilitySearch: null,
              availabilityPage: 1,
              availabilityPlayerId: null,
              availabilityDetailView: null,
              injuryStatus: null,
              injuryPlayerId: null,
              injuryOccurredFrom: null,
              injuryOccurredTo: null,
              injuryPage: 1,
              injuryId: null,
              injuryDetailView: "details",
              injuryRevisionPage: 1,
              injuryAuditPage: 1,
              injuryCreatePlayerId: null,
            })
          }
        />
      </div>
    );
  const view =
    canDetail && url.medicalView === "injuries" ? "injuries" : "availability";
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Dostupnost igrača"
        description="Pratite operativnu dostupnost igrača i, kada imate dozvolu, vodite zaštićenu evidenciju povreda."
      />
      <TeamSelect
        teams={accessible}
        value={teamId}
        onChange={(id) =>
          void setUrl({
            teamId: id,
            availabilityStatus: null,
            availabilitySearch: null,
            availabilityPage: 1,
            availabilityPlayerId: null,
            availabilityDetailView: null,
            injuryStatus: null,
            injuryPlayerId: null,
            injuryOccurredFrom: null,
            injuryOccurredTo: null,
            injuryPage: 1,
            injuryId: null,
            injuryDetailView: "details",
            injuryRevisionPage: 1,
            injuryAuditPage: 1,
            injuryCreatePlayerId: null,
          })
        }
      />
      <Tabs
        value={view}
        onValueChange={(medicalView) => void setUrl({ medicalView })}
      >
        <TabsList>
          <TabsTrigger ref={availabilityTabRef} value="availability">
            Dostupnost
          </TabsTrigger>
          {canDetail ? (
            <TabsTrigger value="injuries">Povrede</TabsTrigger>
          ) : null}
        </TabsList>
        <TabsContent value="availability">
          <Availability
            teamId={teamId}
            status={url.availabilityStatus}
            search={url.availabilitySearch}
            page={url.availabilityPage}
            playerId={url.availabilityPlayerId}
            detailView={url.availabilityDetailView}
            auditPage={url.availabilityAuditPage}
            setUrl={setUrl}
          />
        </TabsContent>
        {canDetail ? (
          <TabsContent value="injuries">
            <Injuries
              teamId={teamId}
              status={url.injuryStatus}
              page={url.injuryPage}
              injuryId={url.injuryId}
              playerId={url.injuryPlayerId}
              occurredFrom={url.injuryOccurredFrom}
              occurredTo={url.injuryOccurredTo}
              detailView={url.injuryDetailView}
              revisionPage={url.injuryRevisionPage}
              auditPage={url.injuryAuditPage}
              createPlayerId={url.injuryCreatePlayerId}
              setUrl={setUrl}
            />
          </TabsContent>
        ) : null}
      </Tabs>
    </div>
  );
}
function TeamSelect({
  teams,
  value,
  onChange,
}: {
  teams: { id: string; name: string }[];
  value: string | null;
  onChange: (id: string) => void;
}) {
  return (
    <Field>
      <FieldLabel>Selekcija</FieldLabel>
      <Select value={value ?? ""} onValueChange={(id) => onChange(id ?? "")}>
        <SelectTrigger className="w-full md:w-80">
          <SelectValue>
            {teams.find((x) => x.id === value)?.name ?? "Odaberite selekciju"}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            {teams.map((team) => (
              <SelectItem key={team.id} value={team.id}>
                {team.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </Field>
  );
}
function Availability({
  teamId,
  status,
  search,
  page,
  playerId,
  detailView,
  auditPage,
  setUrl,
}: {
  teamId: string;
  status: string | null;
  search: string | null;
  page: number;
  playerId: string | null;
  detailView: string | null;
  auditPage: number;
  setUrl: (value: Record<string, unknown>) => Promise<URLSearchParams>;
}) {
  const data = useQuery({
    queryKey: ["availability", teamId, status, search, page],
    queryFn: () => medicalApi.availability({ teamId, status, search, page }),
    retry: false,
  });
  const summary = useQuery({
    queryKey: ["availability", "summary", teamId],
    queryFn: () => medicalApi.summary(teamId),
    retry: false,
  });
  const [editing, setEditing] = useState<AvailabilityItem | null>(null);
  const selected =
    data.data?.items.find((item) => item.playerId === playerId) ?? null;
  const columns: ColumnDef<AvailabilityItem>[] = [
    {
      accessorKey: "displayName",
      header: "Igrač",
      cell: ({ row }) => (
        <span className="font-medium">{row.original.displayName}</span>
      ),
    },
    {
      accessorKey: "status",
      header: "Dostupnost",
      cell: ({ row }) => (
        <Badge variant="secondary">{statuses[row.original.status]}</Badge>
      ),
    },
    {
      accessorKey: "effectiveOn",
      header: "Datum važenja",
      cell: ({ row }) =>
        row.original.effectiveOn
          ? formatDate(row.original.effectiveOn)
          : "Nije uneseno",
    },
    {
      accessorKey: "expectedReturnOn",
      header: "Očekivani povratak",
      cell: ({ row }) =>
        ["LIMITED", "UNAVAILABLE", "REHAB"].includes(row.original.status)
          ? row.original.expectedReturnOn
            ? formatDate(row.original.expectedReturnOn)
            : "Nije uneseno"
          : "—",
    },
    {
      accessorKey: "coachVisibleNote",
      header: "Napomena vidljiva stručnom štabu",
      cell: ({ row }) => (
        <span className="block max-w-72 truncate">
          {row.original.coachVisibleNote ?? "—"}
        </span>
      ),
    },
    {
      id: "actions",
      header: "Radnje",
      cell: ({ row }) =>
        row.original.allowedActions.includes("UPDATE") ? (
          <Button
            size="sm"
            variant="outline"
            onClick={(event) => {
              event.stopPropagation();
              setEditing(row.original);
            }}
          >
            Ažuriraj
          </Button>
        ) : null,
    },
  ];
  const table = useReactTable({
    data: data.data?.items ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });
  return (
    <div className="flex flex-col gap-5">
      <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-6">
        {cards.map((card) => (
          <Card key={card.key} className="gap-0 py-0">
            <CardHeader className="px-4 pt-4 pb-0">
              <CardTitle className="text-muted-foreground text-sm font-normal">
                {card.label}
              </CardTitle>
            </CardHeader>
            <CardContent className="px-4 pt-1 pb-4 text-xl">
              {summary.data ? summary.data[card.key] : "—"}
            </CardContent>
          </Card>
        ))}
      </section>
      <div className="flex flex-wrap gap-3">
        <Input
          className="max-w-sm"
          value={search ?? ""}
          onChange={(event) =>
            void setUrl({
              availabilitySearch: event.target.value || null,
              availabilityPage: 1,
            })
          }
          placeholder="Pretraži igrače"
          aria-label="Pretraži igrače"
        />
        <FilterSelect
          label="Dostupnost"
          emptyLabel="Svi statusi"
          value={status}
          options={statuses}
          className="w-56"
          onChange={(availabilityStatus) =>
            void setUrl({ availabilityStatus, availabilityPage: 1 })
          }
        />
      </div>
      {data.isLoading ? (
        <Skeleton className="h-72 w-full" />
      ) : data.isError ? (
        <Alert variant="destructive">
          <AlertTitle>Nije moguće učitati dostupnost</AlertTitle>
          <AlertDescription>
            <Button variant="outline" onClick={() => void data.refetch()}>
              Pokušaj ponovo
            </Button>
          </AlertDescription>
        </Alert>
      ) : (
        <>
          <div className="overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Igrač</TableHead>
                  <TableHead>Dostupnost</TableHead>
                  <TableHead>Datum važenja</TableHead>
                  <TableHead>Očekivani povratak</TableHead>
                  <TableHead>Napomena vidljiva stručnom štabu</TableHead>
                  <TableHead>Radnje</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.map((row) => {
                  const item = row.original;
                  return (
                    <TableRow
                      key={item.playerId}
                      className="cursor-pointer"
                      onClick={() => {
                        if (item.status !== "UNKNOWN")
                          void setUrl({
                            availabilityPlayerId: item.playerId,
                            availabilityDetailView: "revisions",
                          });
                      }}
                    >
                      <TableCell className="font-medium">
                        {item.displayName}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary">
                          {statuses[item.status]}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {item.effectiveOn
                          ? formatDate(item.effectiveOn)
                          : "Nije uneseno"}
                      </TableCell>
                      <TableCell>
                        {["LIMITED", "UNAVAILABLE", "REHAB"].includes(
                          item.status,
                        )
                          ? item.expectedReturnOn
                            ? formatDate(item.expectedReturnOn)
                            : "Nije uneseno"
                          : "—"}
                      </TableCell>
                      <TableCell className="max-w-72 truncate">
                        {item.coachVisibleNote ?? "—"}
                      </TableCell>
                      <TableCell>
                        {item.allowedActions.includes("UPDATE") ? (
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={(event) => {
                              event.stopPropagation();
                              setEditing(item);
                            }}
                          >
                            Ažuriraj
                          </Button>
                        ) : null}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
          {data.data?.items.length === 0 ? (
            <Alert>
              <AlertTitle>
                Nema igrača koji odgovaraju odabranim filterima.
              </AlertTitle>
            </Alert>
          ) : null}
          <Pagination
            page={page}
            total={data.data?.totalPages ?? 1}
            onChange={(availabilityPage) => void setUrl({ availabilityPage })}
          />
        </>
      )}
      {editing ? (
        <AvailabilityDialog
          item={editing}
          teamId={teamId}
          close={() => setEditing(null)}
        />
      ) : null}
      {selected ? (
        <AvailabilityHistorySheet
          item={selected}
          teamId={teamId}
          view={detailView === "audit" ? "audit" : "revisions"}
          auditPage={auditPage}
          setUrl={setUrl}
          close={() =>
            void setUrl({
              availabilityPlayerId: null,
              availabilityDetailView: null,
            })
          }
        />
      ) : null}
    </div>
  );
}

function AvailabilityHistorySheet({
  item,
  teamId,
  view,
  auditPage,
  setUrl,
  close,
}: {
  item: AvailabilityItem;
  teamId: string;
  view: "revisions" | "audit";
  auditPage: number;
  setUrl: (value: Record<string, unknown>) => Promise<URLSearchParams>;
  close: () => void;
}) {
  const history = useQuery({
    queryKey: ["availability", "player-history", item.playerId, teamId],
    queryFn: () => medicalApi.playerAvailability(item.playerId, teamId),
    enabled: item.status !== "UNKNOWN",
    retry: false,
  });
  const audit = useQuery({
    queryKey: ["availability", "audit", item.playerId, teamId],
    queryFn: () =>
      medicalApi.availabilityAudit(
        history.data?.items[0]?.availabilityId ?? "",
        { page: auditPage },
      ),
    enabled:
      view === "audit" && Boolean(history.data?.items[0]?.availabilityId),
    retry: false,
  });
  return (
    <Sheet open onOpenChange={(open) => !open && close()}>
      <SheetContent className="overflow-y-auto sm:max-w-2xl">
        <SheetHeader className="border-b p-5 pr-12">
          <SheetTitle>{item.displayName}</SheetTitle>
          <SheetDescription>
            Revizije dostupnosti i historija promjena.
          </SheetDescription>
        </SheetHeader>
        {item.status === "UNKNOWN" ? (
          <Alert className="mt-6">
            <AlertTitle>
              Još nema evidentiranih revizija dostupnosti.
            </AlertTitle>
          </Alert>
        ) : (
          <div className="flex flex-col gap-6 px-5 pb-6">
            <Tabs
              value={view}
              onValueChange={(availabilityDetailView) =>
                void setUrl({
                  availabilityDetailView,
                  availabilityAuditPage: 1,
                })
              }
            >
              <TabsList>
                <TabsTrigger value="revisions">
                  Revizije dostupnosti
                </TabsTrigger>
                <TabsTrigger value="audit">Historija promjena</TabsTrigger>
              </TabsList>
            </Tabs>
            {view === "revisions" ? (
              <section className="flex flex-col gap-3">
                <h3 className="font-medium">Revizije dostupnosti</h3>
                {history.isLoading ? (
                  <Skeleton className="h-24 w-full" />
                ) : (
                  history.data?.items.map((record) => (
                    <Card className="gap-2 p-4" key={record.availabilityId}>
                      <p className="font-medium">
                        {statuses[record.status]} ·{" "}
                        {record.effectiveOn
                          ? formatDate(record.effectiveOn)
                          : "Nije uneseno"}
                      </p>
                      {["LIMITED", "UNAVAILABLE", "REHAB"].includes(
                        record.status,
                      ) ? (
                        <p className="text-muted-foreground">
                          Očekivani povratak:{" "}
                          {record.expectedReturnOn
                            ? formatDate(record.expectedReturnOn)
                            : "Nije uneseno"}
                        </p>
                      ) : null}
                      <p className="text-muted-foreground">
                        {record.coachVisibleNote ?? "Bez napomene"}
                      </p>
                    </Card>
                  ))
                )}
              </section>
            ) : null}
            {view === "audit" ? (
              <section className="flex flex-col gap-3">
                <h3 className="font-medium">Historija promjena</h3>
                {audit.isLoading ? (
                  <AuditLoading />
                ) : audit.isError ? (
                  <AuditError retry={() => void audit.refetch()} />
                ) : (
                  <div className="flex flex-col gap-4">
                    <AuditHistory
                      data={audit.data}
                      expectedEntityType="PLAYER_AVAILABILITY"
                      labels={{
                        PLAYER_AVAILABILITY_RECORDED:
                          "Zabilježena dostupnost igrača",
                      }}
                      fields={{
                        playerId: "Igrač",
                        teamId: "Selekcija",
                        previousStatus: "Prethodni status",
                        newStatus: "Novi status",
                        previousEffectiveOn: "Prethodni datum važenja",
                        newEffectiveOn: "Novi datum važenja",
                        previousExpectedReturnOn:
                          "Prethodni očekivani povratak",
                        newExpectedReturnOn: "Novi očekivani povratak",
                        coachVisibleNoteChanged:
                          "Izmijenjena napomena za stručni štab",
                      }}
                      formatMetadata={() =>
                        "Prikazani su samo sigurni operativni podaci."
                      }
                    />
                    {audit.data ? (
                      <AuditPagination
                        data={audit.data}
                        onPage={(availabilityAuditPage) =>
                          void setUrl({ availabilityAuditPage })
                        }
                      />
                    ) : null}
                  </div>
                )}
              </section>
            ) : null}
          </div>
        )}
      </SheetContent>
    </Sheet>
  );
}
function AvailabilityDialog({
  item,
  teamId,
  close,
}: {
  item: AvailabilityItem;
  teamId: string;
  close: () => void;
}) {
  const [status, setStatus] = useState<AvailabilityStatus>(item.status);
  const [effectiveOn, setEffectiveOn] = useState(item.effectiveOn ?? today);
  const [expectedReturnOn, setExpectedReturnOn] = useState(
    item.expectedReturnOn ?? "",
  );
  const [coachVisibleNote, setCoachVisibleNote] = useState(
    item.coachVisibleNote ?? "",
  );
  const client = useQueryClient();
  const mutation = useMutation({
    mutationFn: () =>
      medicalApi.recordAvailability(item.playerId, {
        teamId,
        status,
        effectiveOn,
        expectedReturnOn: ["LIMITED", "UNAVAILABLE", "REHAB"].includes(status)
          ? expectedReturnOn || null
          : null,
        coachVisibleNote: coachVisibleNote || null,
        expectedCurrentRevisionId: item.currentRevisionId,
      }),
    retry: false,
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: ["availability"] });
      void invalidateDashboardOverview(client, teamId);
      toast.success("Dostupnost igrača je ažurirana.");
      close();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ažuriraj dostupnost</DialogTitle>
          <DialogDescription>{item.displayName}</DialogDescription>
        </DialogHeader>
        <FieldGroup>
          <Field>
            <FieldLabel>Status</FieldLabel>
            <Select
              value={status}
              onValueChange={(value) => setStatus(value as AvailabilityStatus)}
            >
              <SelectTrigger>
                <SelectValue>{statuses[status]}</SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  {Object.entries(statuses).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectGroup>
              </SelectContent>
            </Select>
          </Field>
          <Field>
            <FieldLabel htmlFor="effectiveOn">Datum važenja</FieldLabel>
            <DatePicker
              id="effectiveOn"
              value={effectiveOn}
              onChange={setEffectiveOn}
            />
          </Field>
          {["LIMITED", "UNAVAILABLE", "REHAB"].includes(status) ? (
            <Field>
              <FieldLabel htmlFor="expectedReturn">
                Očekivani povratak
              </FieldLabel>
              <DatePicker
                id="expectedReturn"
                value={expectedReturnOn}
                onChange={setExpectedReturnOn}
                disabledDates={{ before: new Date(`${effectiveOn}T00:00:00`) }}
              />
            </Field>
          ) : null}
          <Field>
            <FieldLabel htmlFor="coachNote">
              Napomena vidljiva stručnom štabu
            </FieldLabel>
            <Textarea
              id="coachNote"
              value={coachVisibleNote}
              onChange={(event) => setCoachVisibleNote(event.target.value)}
            />
            <FieldDescription>
              Ovu napomenu mogu vidjeti svi ovlašteni članovi stručnog štaba za
              selekciju. Ne unosite dijagnozu, rezultate pregleda, terapiju ili
              druge osjetljive medicinske detalje.
            </FieldDescription>
          </Field>
          {mutation.error ? (
            <Alert variant="destructive">
              <AlertTitle>
                {isApiError(mutation.error) && mutation.error.status === 409
                  ? "Stanje dostupnosti je u međuvremenu promijenjeno. Pregledajte najnoviju reviziju prije ponovnog čuvanja."
                  : "Ažuriranje nije uspjelo."}
              </AlertTitle>
            </Alert>
          ) : null}
        </FieldGroup>
        <DialogFooter>
          <Button
            disabled={
              mutation.isPending ||
              !effectiveOn ||
              Boolean(expectedReturnOn && expectedReturnOn < effectiveOn)
            }
            onClick={() => mutation.mutate()}
          >
            Sačuvaj
          </Button>
          <Button variant="outline" onClick={close}>
            Odustani
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
function Injuries({
  teamId,
  status,
  page,
  injuryId,
  playerId,
  occurredFrom,
  occurredTo,
  detailView,
  revisionPage,
  auditPage,
  createPlayerId,
  setUrl,
}: {
  teamId: string;
  status: string | null;
  page: number;
  injuryId: string | null;
  playerId: string | null;
  occurredFrom: string | null;
  occurredTo: string | null;
  detailView: string;
  revisionPage: number;
  auditPage: number;
  createPlayerId: string | null;
  setUrl: (value: Record<string, unknown>) => Promise<URLSearchParams>;
}) {
  const data = useQuery({
    queryKey: [
      "medical",
      "restricted",
      "injuries",
      teamId,
      status,
      playerId,
      occurredFrom,
      occurredTo,
      page,
    ],
    queryFn: () =>
      medicalApi.injuries({
        teamId,
        status,
        playerId,
        occurredFrom,
        occurredTo,
        page,
      }),
    retry: false,
    gcTime: 0,
  });
  const { user } = useSession();
  const canWrite =
    user?.primaryRole === "ADMIN" || user?.primaryRole === "MEDICAL_STAFF";
  return (
    <div className="flex flex-col gap-5">
      <div className="flex flex-wrap justify-between gap-3">
        <FilterSelect
          label="Status"
          emptyLabel="Sve povrede"
          value={status}
          options={{ OPEN: "Otvorena", RESOLVED: "Riješena" }}
          className="w-48"
          onChange={(injuryStatus) =>
            void setUrl({ injuryStatus, injuryPage: 1 })
          }
        />
        {canWrite ? (
          <Button onClick={() => void setUrl({ injuryCreatePlayerId: "new" })}>
            Evidentiraj povredu
          </Button>
        ) : null}
      </div>
      <div className="flex flex-wrap gap-3">
        <Input
          value={playerId ?? ""}
          onChange={(event) =>
            void setUrl({
              injuryPlayerId: event.target.value || null,
              injuryPage: 1,
            })
          }
          placeholder="ID igrača"
          aria-label="Filtriraj po igraču"
        />
        <DatePicker
          id="injury-occurred-from"
          value={occurredFrom ?? undefined}
          onChange={(injuryOccurredFrom) =>
            void setUrl({ injuryOccurredFrom, injuryPage: 1 })
          }
          placeholder="Datum od"
        />
        <DatePicker
          id="injury-occurred-to"
          value={occurredTo ?? undefined}
          onChange={(injuryOccurredTo) =>
            void setUrl({ injuryOccurredTo, injuryPage: 1 })
          }
          placeholder="Datum do"
        />
      </div>
      <Alert>
        <AlertTitle>Zaštićeni medicinski detalji</AlertTitle>
        <AlertDescription>
          Ovaj prikaz je dostupan samo ovlaštenim korisnicima. Podaci se ne
          čuvaju u trajnoj memoriji preglednika.
        </AlertDescription>
      </Alert>
      {data.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : data.isError ? (
        <Alert variant="destructive">
          <AlertTitle>Povrede nije moguće učitati.</AlertTitle>
          <AlertDescription>
            <Button variant="outline" onClick={() => void data.refetch()}>
              Pokušaj ponovo
            </Button>
          </AlertDescription>
        </Alert>
      ) : (
        <div className="overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>ID zapisa</TableHead>
                <TableHead>Datum nastanka</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Datum rješenja</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.data?.items.map((item) => (
                <TableRow
                  key={item.id}
                  className="cursor-pointer"
                  onClick={() => void setUrl({ injuryId: item.id })}
                >
                  <TableCell className="font-mono text-xs">{item.id}</TableCell>
                  <TableCell>{formatDate(item.occurredOn)}</TableCell>
                  <TableCell>
                    <Badge variant="secondary">
                      {item.status === "OPEN" ? "Otvorena" : "Riješena"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    {item.resolvedOn ? formatDate(item.resolvedOn) : "—"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      {!data.isLoading && !data.isError && data.data?.items.length === 0 ? (
        <Alert>
          <AlertTitle>
            Nema povreda koje odgovaraju odabranim filterima.
          </AlertTitle>
        </Alert>
      ) : null}
      <Pagination
        page={page}
        total={data.data?.totalPages ?? 1}
        onChange={(injuryPage) => void setUrl({ injuryPage })}
      />
      {createPlayerId ? (
        <InjuryDialog
          teamId={teamId}
          initialPlayerId={createPlayerId === "new" ? null : createPlayerId}
          close={() => void setUrl({ injuryCreatePlayerId: null })}
        />
      ) : null}
      {injuryId ? (
        <InjuryDetailSheet
          injuryId={injuryId}
          view={detailView}
          revisionPage={revisionPage}
          auditPage={auditPage}
          setUrl={setUrl}
          close={() => void setUrl({ injuryId: null })}
        />
      ) : null}
    </div>
  );
}
function InjuryDetailSheet({
  injuryId,
  view,
  revisionPage,
  auditPage,
  setUrl,
  close,
}: {
  injuryId: string;
  view: string;
  revisionPage: number;
  auditPage: number;
  setUrl: (value: Record<string, unknown>) => Promise<URLSearchParams>;
  close: () => void;
}) {
  const detailView =
    view === "revisions" || view === "audit" ? view : "details";
  const detail = useQuery({
    queryKey: ["medical", "restricted", "injury", injuryId],
    queryFn: () => medicalApi.injury(injuryId),
    retry: false,
    gcTime: 0,
  });
  const revisions = useQuery({
    queryKey: ["medical", "restricted", "injury-revisions", injuryId],
    queryFn: () => medicalApi.injuryRevisions(injuryId, revisionPage),
    enabled: detail.isSuccess && detailView === "revisions",
    retry: false,
    gcTime: 0,
  });
  const audit = useQuery({
    queryKey: ["medical", "restricted", "injury-audit", injuryId],
    queryFn: () => medicalApi.injuryAudit(injuryId, { page: auditPage }),
    enabled: detail.isSuccess && detailView === "audit",
    retry: false,
    gcTime: 0,
  });
  const client = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [resolving, setResolving] = useState(false);
  const refresh = () =>
    void client.invalidateQueries({ queryKey: ["medical", "restricted"] });
  return (
    <Sheet open onOpenChange={(open) => !open && close()}>
      <SheetContent className="overflow-y-auto sm:max-w-xl">
        <SheetHeader>
          <SheetTitle>Zaštićeni medicinski detalji</SheetTitle>
          <SheetDescription>
            Detalji su dostupni samo ovlaštenim korisnicima.
          </SheetDescription>
        </SheetHeader>
        {detail.isLoading ? <Skeleton className="mt-6 h-64 w-full" /> : null}
        {detail.isError || !detail.data ? (
          <Alert variant="destructive" className="mt-6">
            <AlertTitle>Zapis nije dostupan.</AlertTitle>
          </Alert>
        ) : (
          <div className="mt-6 flex flex-col gap-5">
            <Tabs
              value={detailView}
              onValueChange={(injuryDetailView) =>
                void setUrl({
                  injuryDetailView,
                  injuryRevisionPage: 1,
                  injuryAuditPage: 1,
                })
              }
            >
              <TabsList>
                <TabsTrigger value="details">Detalji</TabsTrigger>
                <TabsTrigger value="revisions">Revizije</TabsTrigger>
                <TabsTrigger value="audit">Historija promjena</TabsTrigger>
              </TabsList>
            </Tabs>
            {detailView === "details" ? (
              <>
                <dl className="grid gap-3 text-sm">
                  <div>
                    <dt className="text-muted-foreground">Datum nastanka</dt>
                    <dd>{formatDate(detail.data.occurredOn)}</dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">Status</dt>
                    <dd>
                      {detail.data.status === "OPEN" ? "Otvorena" : "Riješena"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">Dio tijela</dt>
                    <dd>
                      {detail.data.currentRevision.bodyArea ?? "Nije uneseno"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">Dijagnoza</dt>
                    <dd>
                      {detail.data.currentRevision.diagnosis ?? "Nije uneseno"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">
                      Zaštićena medicinska napomena
                    </dt>
                    <dd className="whitespace-pre-wrap">
                      {detail.data.currentRevision.restrictedNotes ??
                        "Nije uneseno"}
                    </dd>
                  </div>
                </dl>
                {detail.data.allowedActions.includes("UPDATE") ? (
                  <Button variant="outline" onClick={() => setEditing(true)}>
                    Ažuriraj medicinske detalje
                  </Button>
                ) : null}
                {detail.data.allowedActions.includes("RESOLVE") ? (
                  <Button variant="outline" onClick={() => setResolving(true)}>
                    Označi kao riješenu
                  </Button>
                ) : null}
              </>
            ) : null}
            {detailView === "revisions" ? (
              <section className="flex flex-col gap-3">
                <h3 className="font-medium">Revizije medicinskog zapisa</h3>
                {revisions.isLoading ? (
                  <Skeleton className="h-28 w-full" />
                ) : null}
                {revisions.isError ? (
                  <Alert variant="destructive">
                    <AlertTitle>Revizije nije moguće učitati.</AlertTitle>
                  </Alert>
                ) : null}
                {revisions.data?.items.map((revision) => (
                  <article
                    key={revision.id}
                    className="rounded-xl border p-3 text-sm"
                  >
                    <p className="font-medium">
                      Revizija {revision.revisionNumber}
                    </p>
                    <p className="text-muted-foreground">
                      {revision.bodyArea ?? "Nije uneseno"} ·{" "}
                      {revision.diagnosis ?? "Nije uneseno"}
                    </p>
                  </article>
                ))}
                {revisions.data &&
                revisions.data.totalCount > revisions.data.pageSize ? (
                  <Pagination
                    page={revisions.data.page}
                    total={Math.ceil(
                      revisions.data.totalCount / revisions.data.pageSize,
                    )}
                    onChange={(injuryRevisionPage) =>
                      void setUrl({ injuryRevisionPage })
                    }
                  />
                ) : null}
              </section>
            ) : null}
            {detailView === "audit" ? (
              <section className="flex flex-col gap-3">
                <h3 className="font-medium">Historija promjena</h3>
                {audit.isLoading ? (
                  <AuditLoading />
                ) : audit.isError ? (
                  <AuditError retry={() => void audit.refetch()} />
                ) : (
                  <div className="flex flex-col gap-4">
                    <AuditHistory
                      data={audit.data}
                      expectedEntityType="INJURY_RECORD"
                      labels={{
                        INJURY_RECORD_CREATED: "Evidentirana povreda",
                        INJURY_RECORD_REVISED: "Ažurirani medicinski detalji",
                        INJURY_RECORD_RESOLVED: "Povreda riješena",
                      }}
                      fields={{}}
                      formatMetadata={() =>
                        "Prikazana su samo nazivi izmijenjenih polja."
                      }
                    />
                    {audit.data ? (
                      <AuditPagination
                        data={audit.data}
                        onPage={(injuryAuditPage) =>
                          void setUrl({ injuryAuditPage })
                        }
                      />
                    ) : null}
                  </div>
                )}
              </section>
            ) : null}
          </div>
        )}
      </SheetContent>
      {editing && detail.data ? (
        <InjuryUpdateDialog
          detail={detail.data}
          close={() => setEditing(false)}
          saved={refresh}
        />
      ) : null}
      {resolving && detail.data ? (
        <InjuryResolveDialog
          detail={detail.data}
          close={() => setResolving(false)}
          saved={refresh}
        />
      ) : null}
    </Sheet>
  );
}

function InjuryUpdateDialog({
  detail,
  close,
  saved,
}: {
  detail: import("./api").InjuryDetail;
  close: () => void;
  saved: () => void;
}) {
  const [bodyArea, setBodyArea] = useState(
    detail.currentRevision.bodyArea ?? "",
  );
  const [diagnosis, setDiagnosis] = useState(
    detail.currentRevision.diagnosis ?? "",
  );
  const [restrictedNotes, setRestrictedNotes] = useState(
    detail.currentRevision.restrictedNotes ?? "",
  );
  const mutation = useMutation({
    mutationFn: () =>
      medicalApi.updateInjury(detail.id, {
        bodyArea: bodyArea || null,
        diagnosis: diagnosis || null,
        restrictedNotes: restrictedNotes || null,
        expectedCurrentRevisionId: detail.currentRevision.id,
      }),
    retry: false,
    onSuccess: () => {
      saved();
      toast.success("Medicinski detalji su ažurirani.");
      close();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ažuriraj medicinske detalje</DialogTitle>
          <DialogDescription>
            Izmjena stvara novu reviziju zapisa.
          </DialogDescription>
        </DialogHeader>
        <FieldGroup>
          <Field>
            <FieldLabel htmlFor="update-body-area">Dio tijela</FieldLabel>
            <Input
              id="update-body-area"
              value={bodyArea}
              onChange={(event) => setBodyArea(event.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="update-diagnosis">Dijagnoza</FieldLabel>
            <Input
              id="update-diagnosis"
              value={diagnosis}
              onChange={(event) => setDiagnosis(event.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="update-notes">
              Zaštićena medicinska napomena
            </FieldLabel>
            <Textarea
              id="update-notes"
              value={restrictedNotes}
              onChange={(event) => setRestrictedNotes(event.target.value)}
            />
          </Field>
          {mutation.error ? (
            <Alert variant="destructive">
              <AlertTitle>
                Izmjena nije uspjela. Pregledajte najnoviju reviziju prije
                ponovnog čuvanja.
              </AlertTitle>
            </Alert>
          ) : null}
        </FieldGroup>
        <DialogFooter>
          <Button
            disabled={
              mutation.isPending || !(bodyArea || diagnosis || restrictedNotes)
            }
            onClick={() => mutation.mutate()}
          >
            Sačuvaj
          </Button>
          <Button variant="outline" onClick={close}>
            Odustani
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function InjuryResolveDialog({
  detail,
  close,
  saved,
}: {
  detail: import("./api").InjuryDetail;
  close: () => void;
  saved: () => void;
}) {
  const [resolvedOn, setResolvedOn] = useState(today);
  const mutation = useMutation({
    mutationFn: () =>
      medicalApi.resolveInjury(detail.id, {
        resolvedOn,
        expectedCurrentRevisionId: detail.currentRevision.id,
      }),
    retry: false,
    onSuccess: () => {
      saved();
      toast.success("Povreda je označena kao riješena.");
      close();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Označi kao riješenu</DialogTitle>
          <DialogDescription>
            Ova radnja ne mijenja automatski dostupnost igrača.
          </DialogDescription>
        </DialogHeader>
        <Field>
          <FieldLabel htmlFor="resolved-on">Datum rješenja</FieldLabel>
          <DatePicker
            id="resolved-on"
            value={resolvedOn}
            onChange={setResolvedOn}
            disabledDates={{
              before: new Date(`${detail.occurredOn}T00:00:00`),
              after: new Date(`${today}T00:00:00`),
            }}
          />
        </Field>
        {mutation.error ? (
          <Alert variant="destructive">
            <AlertTitle>Rješavanje nije uspjelo.</AlertTitle>
          </Alert>
        ) : null}
        <DialogFooter>
          <Button
            disabled={
              mutation.isPending ||
              !resolvedOn ||
              resolvedOn < detail.occurredOn
            }
            onClick={() => mutation.mutate()}
          >
            Potvrdi
          </Button>
          <Button variant="outline" onClick={close}>
            Odustani
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function InjuryDialog({
  teamId,
  initialPlayerId,
  close,
}: {
  teamId: string;
  initialPlayerId: string | null;
  close: () => void;
}) {
  const [occurredOn, setOccurredOn] = useState(today);
  const [playerId, setPlayerId] = useState(initialPlayerId ?? "");
  const [candidateSearch, setCandidateSearch] = useState("");
  const [bodyArea, setBodyArea] = useState("");
  const [diagnosis, setDiagnosis] = useState("");
  const [restrictedNotes, setRestrictedNotes] = useState("");
  const candidates = useQuery({
    queryKey: [
      "medical",
      "restricted",
      "candidates",
      teamId,
      occurredOn,
      initialPlayerId,
      candidateSearch,
    ],
    queryFn: () =>
      medicalApi.candidates({
        teamId,
        occurredOn,
        playerId: initialPlayerId ?? undefined,
        search: initialPlayerId ? undefined : candidateSearch,
      }),
    enabled: Boolean(occurredOn),
    retry: false,
    gcTime: 0,
  });
  const client = useQueryClient();
  const mutation = useMutation({
    mutationFn: () =>
      medicalApi.createInjury({
        playerId,
        teamId,
        occurredOn,
        bodyArea: bodyArea || null,
        diagnosis: diagnosis || null,
        restrictedNotes: restrictedNotes || null,
      }),
    retry: false,
    onSuccess: () => {
      void client.invalidateQueries({
        queryKey: ["medical", "restricted", "injuries"],
      });
      toast.success("Povreda je evidentirana.");
      close();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Evidentiraj povredu</DialogTitle>
          <DialogDescription>
            Dostupnost igrača se neće automatski promijeniti.
          </DialogDescription>
        </DialogHeader>
        <FieldGroup>
          <Field>
            <FieldLabel htmlFor="occurredOn">Datum nastanka</FieldLabel>
            <DatePicker
              id="occurredOn"
              value={occurredOn}
              disabledDates={{ after: new Date(`${today}T00:00:00`) }}
              onChange={(value) => {
                setOccurredOn(value);
                setPlayerId(initialPlayerId ?? "");
              }}
            />
          </Field>
          <Field>
            <FieldLabel>Igrač</FieldLabel>
            {!initialPlayerId ? (
              <Input
                value={candidateSearch}
                onChange={(event) => {
                  setCandidateSearch(event.target.value);
                  setPlayerId("");
                }}
                placeholder="Pretraži igrače"
                aria-label="Pretraži kandidate za povredu"
              />
            ) : null}
            <Select
              value={playerId}
              onValueChange={(value) => setPlayerId(value ?? "")}
            >
              <SelectTrigger>
                <SelectValue>Odaberite igrača</SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  {candidates.data?.items.map((candidate) => (
                    <SelectItem key={candidate.id} value={candidate.id}>
                      {candidate.displayName}
                    </SelectItem>
                  ))}
                  {!candidates.isLoading && !candidates.data?.items.length ? (
                    <SelectItem value="none" disabled>
                      Nema podobnih igrača za odabrani datum
                    </SelectItem>
                  ) : null}
                </SelectGroup>
              </SelectContent>
            </Select>
          </Field>
          <Field>
            <FieldLabel htmlFor="bodyArea">Dio tijela</FieldLabel>
            <Input
              id="bodyArea"
              value={bodyArea}
              onChange={(event) => setBodyArea(event.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="diagnosis">Dijagnoza</FieldLabel>
            <Input
              id="diagnosis"
              value={diagnosis}
              onChange={(event) => setDiagnosis(event.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="restrictedNotes">
              Zaštićena medicinska napomena
            </FieldLabel>
            <Textarea
              id="restrictedNotes"
              value={restrictedNotes}
              onChange={(event) => setRestrictedNotes(event.target.value)}
            />
          </Field>
          {mutation.error ? (
            <Alert variant="destructive">
              <AlertTitle>Spremanje nije uspjelo.</AlertTitle>
            </Alert>
          ) : null}
        </FieldGroup>
        <DialogFooter>
          <Button
            disabled={
              mutation.isPending ||
              !playerId ||
              !candidates.data?.items.some(
                (candidate) => candidate.id === playerId,
              ) ||
              !(bodyArea || diagnosis || restrictedNotes)
            }
            onClick={() => mutation.mutate()}
          >
            Sačuvaj
          </Button>
          <Button variant="outline" onClick={close}>
            Odustani
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
function Pagination({
  page,
  total,
  onChange,
}: {
  page: number;
  total: number;
  onChange: (page: number) => void;
}) {
  return (
    <div className="flex justify-end gap-2">
      <Button
        variant="outline"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
      >
        Prethodna
      </Button>
      <Button
        variant="outline"
        disabled={page >= total}
        onClick={() => onChange(page + 1)}
      >
        Sljedeća
      </Button>
    </div>
  );
}
