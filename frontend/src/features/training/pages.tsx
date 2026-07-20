import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { invalidateDashboardOverview } from "@/features/dashboard";
import { Plus, Search } from "lucide-react";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { z } from "zod";
import { PageHeader } from "@/components/common/page-header";
import { DatePicker } from "@/components/common/date-picker";
import { FilterSelect } from "@/components/common/filter-select";
import { ErrorState } from "@/components/common/error-state";
import { LoadingState } from "@/components/common/loading-state";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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
  FieldError,
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useSession } from "@/features/auth/hooks/use-session";
import { CreateImport } from "@/features/imports/imports-page";
import {
  importQueryKeys,
  importsApi,
} from "@/features/imports/api/imports-api";
import {
  AuditError,
  AuditHistory,
  AuditLoading,
  getTrainingSessionAudit,
} from "@/features/audit";
import { settingsApi } from "@/features/settings/api";
import { formatDate, formatUtcDateTime } from "@/lib/date-format";
import { isApiError } from "@/lib/api/api-client";
import { trainingApi, trainingKeys } from "./api";
import { WorkloadTable } from "./workloads";
import type { TrainingFilters, TrainingSessionStatus } from "./types";

const statusLabels: Record<TrainingSessionStatus, string> = {
  PLANNED: "Planiran",
  COMPLETED: "Završen",
  CANCELLED: "Otkazan",
};
const schema = z.object({
  teamId: z.string().uuid("Odaberite selekciju."),
  sessionDate: z.string().min(1, "Odaberite datum."),
  title: z.string().trim().min(1, "Unesite naziv.").max(200),
  location: z.string().max(200).optional(),
  description: z.string().max(2000).optional(),
});
type Values = z.infer<typeof schema>;
function canWrite(user: ReturnType<typeof useSession>["user"], teamId: string) {
  return (
    user?.primaryRole === "ADMIN" ||
    (user?.primaryRole === "DATA_OPERATOR" &&
      (user.teamScope.type === "ALL" ||
        user.teamScope.selectedTeamIds.includes(teamId)))
  );
}
function errorText(error: unknown) {
  if (isApiError(error)) {
    if (error.status === 409)
      return "Radnja nije dozvoljena u trenutnom stanju treninga.";
    if (error.status === 422)
      return "Igrač nema pripadnost selekciji na datum treninga.";
    return error.detail ?? "Radnja nije uspjela.";
  }
  return "Mrežna greška. Pokušajte ponovo.";
}

export function TrainingSessionsPage() {
  const { user } = useSession();
  const [params, setParams] = useSearchParams();
  const [create, setCreate] = useState(false);
  const filters: TrainingFilters = {
    search: params.get("search"),
    teamId: params.get("teamId"),
    status: params.get("status") as TrainingSessionStatus | null,
    dateFrom: params.get("dateFrom"),
    dateTo: params.get("dateTo"),
    page: Number(params.get("page") ?? 1),
  };
  const sessions = useQuery({
    queryKey: trainingKeys.list(filters),
    queryFn: () => trainingApi.list(filters),
    retry: false,
  });
  const teams = useQuery({
    queryKey: ["settings", "teams", false],
    queryFn: () => settingsApi.listTeams(false),
  });
  const set = (key: string, value: string | null) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    if (key !== "page") next.delete("page");
    setParams(next);
  };
  const hasFilters = Boolean(
    filters.search ||
    filters.teamId ||
    filters.status ||
    filters.dateFrom ||
    filters.dateTo,
  );
  const resetFilters = () => setParams(new URLSearchParams());
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Treninzi i GPS"
        description="Planiranje treninga, učesnici i potvrđeni fizički podaci."
        actions={
          user?.primaryRole === "ADMIN" ||
          user?.primaryRole === "DATA_OPERATOR" ? (
            <Button onClick={() => setCreate(true)}>
              <Plus data-icon="inline-start" />
              Novi trening
            </Button>
          ) : null
        }
      />
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 md:flex md:flex-wrap">
        <Input
          className="w-full md:w-56"
          aria-label="Pretraži treninge"
          placeholder="Pretraži treninge"
          value={filters.search ?? ""}
          onChange={(e) => set("search", e.target.value)}
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={filters.teamId}
          options={Object.fromEntries(
            (teams.data ?? []).map((team) => [team.id, team.name]),
          )}
          className="w-full md:w-48"
          onChange={(teamId) => set("teamId", teamId)}
        />
        <FilterSelect
          label="Status"
          emptyLabel="Svi statusi"
          value={filters.status}
          options={statusLabels}
          className="w-full md:w-40"
          onChange={(status) => set("status", status)}
        />
        <DatePicker
          id="training-date-from"
          className="w-full md:w-40"
          value={filters.dateFrom ?? undefined}
          onChange={(v) => set("dateFrom", v ?? null)}
          placeholder="Datum od"
        />
        <DatePicker
          id="training-date-to"
          className="w-full md:w-40"
          value={filters.dateTo ?? undefined}
          onChange={(v) => set("dateTo", v ?? null)}
          placeholder="Datum do"
        />
        {hasFilters ? (
          <Button
            className="md:ml-auto"
            variant="outline"
            onClick={resetFilters}
          >
            OÄisti filtere
          </Button>
        ) : null}
      </section>
      {sessions.isLoading ? (
        <LoadingState />
      ) : sessions.isError ? (
        <ErrorState
          description="Treninge nije moguće učitati."
          action={
            <Button onClick={() => void sessions.refetch()}>
              Pokušaj ponovo
            </Button>
          }
        />
      ) : (
        <div className="overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Datum</TableHead>
                <TableHead>Trening</TableHead>
                <TableHead>Selekcija</TableHead>
                <TableHead>Status</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {sessions.data?.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{formatDate(item.sessionDate)}</TableCell>
                  <TableCell>{item.title}</TableCell>
                  <TableCell>
                    {(teams.data ?? []).find((t) => t.id === item.teamId)
                      ?.name ?? "Nije dostupno"}
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary">
                      {statusLabels[item.status]}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Button
                      size="sm"
                      variant="outline"
                      nativeButton={false}
                      render={<Link to={`/training-sessions/${item.id}`} />}
                    >
                      Pregled
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {!sessions.data?.items.length ? (
                <TableRow>
                  <TableCell
                    colSpan={5}
                    className="text-muted-foreground text-center"
                  >
                    Nema treninga za odabrane filtere.
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </div>
      )}
      {sessions.data && sessions.data.totalCount > sessions.data.pageSize ? (
        <div className="flex justify-end gap-2">
          <Button
            variant="outline"
            disabled={sessions.data.page <= 1}
            onClick={() => set("page", String(sessions.data!.page - 1))}
          >
            Prethodna
          </Button>
          <Button
            variant="outline"
            disabled={
              sessions.data.page * sessions.data.pageSize >=
              sessions.data.totalCount
            }
            onClick={() => set("page", String(sessions.data!.page + 1))}
          >
            Sljedeća
          </Button>
        </div>
      ) : null}
      {create ? (
        <TrainingDialog
          teams={teams.data ?? []}
          onClose={() => setCreate(false)}
        />
      ) : null}
    </div>
  );
}
function TrainingDialog({
  teams,
  onClose,
}: {
  teams: { id: string; name: string }[];
  onClose: () => void;
}) {
  const qc = useQueryClient();
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      teamId: "",
      sessionDate: new Date().toISOString().slice(0, 10),
      title: "",
      location: "",
      description: "",
    },
  });
  const teamId = useWatch({ control: form.control, name: "teamId" });
  const sessionDate = useWatch({
    control: form.control,
    name: "sessionDate",
  });
  const mutation = useMutation({
    mutationFn: (v: Values) =>
      trainingApi.create({
        ...v,
        location: v.location || null,
        description: v.description || null,
        startsAtUtc: null,
        endsAtUtc: null,
      }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["trainingSessions"] });
      onClose();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Novi trening</DialogTitle>
          <DialogDescription>
            Trening se uvijek kreira sa statusom Planiran.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={form.handleSubmit((v) => mutation.mutate(v))}>
          <FieldGroup>
            <Field data-invalid={!!form.formState.errors.teamId}>
              <FieldLabel>Selekcija</FieldLabel>
              <Select
                value={teamId}
                onValueChange={(v) => form.setValue("teamId", v ?? "")}
              >
                <SelectTrigger aria-invalid={!!form.formState.errors.teamId}>
                  <SelectValue>Odaberite selekciju</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {teams.map((t) => (
                      <SelectItem key={t.id} value={t.id}>
                        {t.name}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
              <FieldError>{form.formState.errors.teamId?.message}</FieldError>
            </Field>
            <Field>
              <FieldLabel htmlFor="training-date">Datum</FieldLabel>
              <DatePicker
                id="training-date"
                value={sessionDate}
                onChange={(sessionDate) =>
                  form.setValue("sessionDate", sessionDate, {
                    shouldValidate: true,
                  })
                }
                placeholder="Odaberite datum"
              />
            </Field>
            <Field data-invalid={!!form.formState.errors.title}>
              <FieldLabel htmlFor="training-title">Naziv</FieldLabel>
              <Input
                id="training-title"
                aria-invalid={!!form.formState.errors.title}
                {...form.register("title")}
              />
              <FieldError>{form.formState.errors.title?.message}</FieldError>
            </Field>
            <Field>
              <FieldLabel htmlFor="training-location">Lokacija</FieldLabel>
              <Input id="training-location" {...form.register("location")} />
            </Field>
          </FieldGroup>
          <DialogFooter className="mt-2">
            <Button type="button" variant="outline" onClick={onClose}>
              Odustani
            </Button>
            <Button disabled={mutation.isPending} type="submit">
              Sačuvaj trening
            </Button>
          </DialogFooter>
          {mutation.error ? (
            <p className="text-destructive text-sm">
              {errorText(mutation.error)}
            </p>
          ) : null}
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function TrainingSessionDetailPage() {
  const { id = "" } = useParams();
  const { user } = useSession();
  const [params, setParams] = useSearchParams();
  const [search, setSearch] = useState("");
  const qc = useQueryClient();
  const tab = ["overview", "participants", "physical", "audit"].includes(
    params.get("tab") ?? "",
  )
    ? params.get("tab")!
    : "overview";
  const session = useQuery({
    queryKey: trainingKeys.detail(id),
    queryFn: () => trainingApi.get(id),
    retry: false,
  });
  const participants = useQuery({
    queryKey: trainingKeys.participants(id),
    queryFn: () => trainingApi.participants(id),
    enabled: !!id,
  });
  const candidates = useQuery({
    queryKey: trainingKeys.candidates(id, search),
    queryFn: () => trainingApi.candidates(id, search),
    enabled: tab === "participants" && !!id,
  });
  const workloads = useQuery({
    queryKey: trainingKeys.workloads(id),
    queryFn: () => trainingApi.workloads(id),
    enabled: tab === "physical" && !!id,
  });
  const importCapabilities = useQuery({
    queryKey: importQueryKeys.capabilities,
    queryFn: importsApi.capabilities,
    enabled:
      user?.primaryRole === "ADMIN" ||
      (user?.primaryRole === "DATA_OPERATOR" && user.permissions.canImportData),
  });
  const importTeams = useQuery({
    queryKey: ["settings", "teams", "training-import"],
    queryFn: () => settingsApi.listTeams(false),
  });
  const sessionImports = useQuery({
    queryKey: importQueryKeys.list({ trainingSessionId: id }),
    queryFn: () => importsApi.list({ trainingSessionId: id }),
    enabled: tab === "physical",
  });
  const audit = useQuery({
    queryKey: ["trainingSession", id, "audit"],
    queryFn: () => getTrainingSessionAudit(id, { page: 1 }),
    enabled: tab === "audit",
  });
  const refresh = () => {
    void qc.invalidateQueries({ queryKey: trainingKeys.detail(id) });
    void qc.invalidateQueries({ queryKey: trainingKeys.participants(id) });
    void qc.invalidateQueries({ queryKey: ["trainingSessions"] });
    void invalidateDashboardOverview(qc, session.data?.teamId);
  };
  const life = useMutation({
    mutationFn: (action: "complete" | "cancel") =>
      trainingApi.lifecycle(id, action),
    onSuccess: refresh,
  });
  const add = useMutation({
    mutationFn: (playerId: string) => trainingApi.addParticipant(id, playerId),
    onSuccess: () => {
      refresh();
      void qc.invalidateQueries({
        queryKey: ["trainingSession", id, "candidates"],
      });
    },
  });
  const remove = useMutation({
    mutationFn: (participantId: string) =>
      trainingApi.removeParticipant(id, participantId),
    onSuccess: refresh,
  });
  if (session.isLoading) return <LoadingState />;
  if (session.isError || !session.data)
    return (
      <Alert variant="destructive">
        <AlertTitle>Trening nije dostupan</AlertTitle>
        <AlertDescription>
          Nemate pristup ovom treningu ili zapis ne postoji.
        </AlertDescription>
      </Alert>
    );
  const data = session.data;
  const write = canWrite(user, data.teamId);
  const select = (v: string) => {
    const next = new URLSearchParams(params);
    if (v === "overview") next.delete("tab");
    else next.set("tab", v);
    setParams(next);
  };
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={data.title}
        description={`${formatDate(data.sessionDate)}${data.startsAtUtc ? ` · ${formatUtcDateTime(data.startsAtUtc)}` : ""}`}
        actions={
          write && data.status === "PLANNED" ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => life.mutate("complete")}>
                Završi trening
              </Button>
              <Button
                variant="destructive"
                onClick={() => life.mutate("cancel")}
              >
                Otkaži trening
              </Button>
            </div>
          ) : null
        }
      />
      <Badge variant="secondary">{statusLabels[data.status]}</Badge>
      {life.error ? (
        <Alert variant="destructive">
          <AlertTitle>Radnja nije uspjela</AlertTitle>
          <AlertDescription>{errorText(life.error)}</AlertDescription>
        </Alert>
      ) : null}
      <Tabs value={tab} onValueChange={select}>
        <TabsList aria-label="Sadržaj treninga">
          <TabsTrigger value="overview">Pregled</TabsTrigger>
          <TabsTrigger value="participants">Učesnici</TabsTrigger>
          <TabsTrigger value="physical">GPS / Fizički podaci</TabsTrigger>
          <TabsTrigger value="audit">Historija promjena</TabsTrigger>
        </TabsList>
        <TabsContent value="overview">
          <dl className="grid gap-4 rounded-xl border p-5 sm:grid-cols-2">
            <div>
              <dt className="text-muted-foreground text-sm">Lokacija</dt>
              <dd>{data.location ?? "Nije dostupno"}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Status</dt>
              <dd>{statusLabels[data.status]}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-muted-foreground text-sm">Opis</dt>
              <dd>{data.description ?? "Nije dodan opis."}</dd>
            </div>
          </dl>
        </TabsContent>
        <TabsContent value="participants">
          <div className="flex flex-col gap-4">
            <Alert>
              <AlertTitle>Učesnici se čuvaju u historiji</AlertTitle>
              <AlertDescription>
                Uklanjanje ne briše historijski zapis; učesnik s postojećim
                fizičkim opterećenjem ne može biti uklonjen.
              </AlertDescription>
            </Alert>
            {write && data.status !== "CANCELLED" ? (
              <>
                <div className="flex gap-2">
                  <Input
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    placeholder="Pretraži podobne igrače"
                    aria-label="Pretraži podobne igrače"
                  />
                  <Search aria-hidden="true" />
                </div>
                <div className="grid gap-2">
                  {candidates.data?.items.map((p) => (
                    <div
                      key={p.id}
                      className="flex items-center justify-between gap-3 rounded-xl border p-3"
                    >
                      <span>
                        {p.preferredName ?? `${p.firstName} ${p.lastName}`}
                      </span>
                      <Button size="sm" onClick={() => add.mutate(p.id)}>
                        Dodaj učesnika
                      </Button>
                    </div>
                  ))}
                  {!candidates.data?.items.length ? (
                    <p className="text-muted-foreground text-sm">
                      Nema podobnih igrača.
                    </p>
                  ) : null}
                </div>
              </>
            ) : null}
            <div className="overflow-x-auto rounded-xl border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Igrač</TableHead>
                    <TableHead>Radnja</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {participants.data?.map((p) => (
                    <TableRow key={p.id}>
                      <TableCell>{p.preferredName ?? p.playerName}</TableCell>
                      <TableCell>
                        {write && data.status !== "CANCELLED" ? (
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => remove.mutate(p.id)}
                          >
                            Ukloni učesnika
                          </Button>
                        ) : (
                          "—"
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            {add.error || remove.error ? (
              <p className="text-destructive text-sm">
                {errorText(add.error ?? remove.error)}
              </p>
            ) : null}
          </div>
        </TabsContent>
        <TabsContent value="physical">
          {data.status === "PLANNED" ? (
            <Alert>
              <AlertTitle>Potvrđeni fizički podaci nisu dostupni</AlertTitle>
              <AlertDescription>
                Službeno opterećenje može biti vezano samo za završen trening.
              </AlertDescription>
            </Alert>
          ) : (
            <div className="flex flex-col gap-4">
              {workloads.data?.length ? (
                <WorkloadTable workloads={workloads.data} />
              ) : (
                <Alert>
                  <AlertTitle>Nema potvrđenih fizičkih podataka</AlertTitle>
                  <AlertDescription>
                    Gpexe i Zone14 ostaju dostupni samo za generički pregled dok
                    se ne potvrde stvarni procesori.
                  </AlertDescription>
                </Alert>
              )}
              {write &&
              (user?.primaryRole === "ADMIN" ||
                user?.permissions.canImportData) ? (
                <CreateImport
                  disabled={data.status === "CANCELLED"}
                  capabilities={importCapabilities.data}
                  teams={importTeams.data ?? []}
                  context={{
                    teamId: data.teamId,
                    trainingSessionId: data.id,
                    importType: "TRAINING_GPS",
                  }}
                  onCreated={() => void sessionImports.refetch()}
                />
              ) : null}
              <section className="flex flex-col gap-2">
                <h3 className="font-medium">Izvorni importi</h3>
                {sessionImports.data?.items.length ? (
                  sessionImports.data.items.map((item) => (
                    <Link
                      className="text-primary text-sm"
                      key={item.id}
                      to={`/imports?importJobId=${item.id}`}
                    >
                      {item.originalFileName}
                    </Link>
                  ))
                ) : (
                  <p className="text-muted-foreground text-sm">
                    Nema importa vezanih za ovaj trening.
                  </p>
                )}
              </section>
            </div>
          )}
        </TabsContent>
        <TabsContent value="audit">
          {audit.isLoading ? (
            <AuditLoading />
          ) : audit.isError ? (
            <AuditError retry={() => void audit.refetch()} />
          ) : (
            <AuditHistory
              data={audit.data}
              expectedEntityType="TRAINING_SESSION"
              labels={{
                TRAINING_SESSION_CREATED: "Trening je kreiran",
                TRAINING_SESSION_UPDATED: "Trening je izmijenjen",
                TRAINING_SESSION_COMPLETED: "Trening je završen",
                TRAINING_SESSION_CANCELLED: "Trening je otkazan",
                TRAINING_SESSION_PARTICIPANT_ADDED: "Učesnik je dodan",
                TRAINING_SESSION_PARTICIPANT_REMOVED: "Učesnik je uklonjen",
              }}
              fields={{}}
              formatMetadata={(metadata) => {
                if (!metadata || typeof metadata !== "object")
                  return "Nema dodatnih detalja.";
                const record = metadata as Record<string, unknown>;
                if ("playerId" in record) {
                  const participant = participants.data?.find(
                    (item) => item.playerId === String(record.playerId),
                  );
                  return `Učesnik: ${participant?.preferredName ?? participant?.playerName ?? "Nije dostupan"}`;
                }
                if ("teamId" in record || "sessionDate" in record) {
                  const team = importTeams.data?.find(
                    (item) => item.id === String(record.teamId),
                  );
                  const date =
                    typeof record.sessionDate === "string"
                      ? formatDate(record.sessionDate)
                      : "Nije dostupan";
                  return `Selekcija: ${team?.name ?? "Nije dostupna"} · Datum treninga: ${date}`;
                }
                return "Detalji su evidentirani.";
              }}
            />
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
