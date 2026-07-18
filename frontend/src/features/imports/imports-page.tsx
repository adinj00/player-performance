import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { UploadIcon } from "lucide-react";
import { invalidateDashboardOverview } from "@/features/dashboard";
import { useState } from "react";
import { useQueryStates, parseAsInteger, parseAsString } from "nuqs";
import { toast } from "sonner";
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
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
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
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { formatUtcDateTime } from "@/lib/date-format";
import { isApiError } from "@/lib/api/api-client";
import { useSession } from "@/features/auth/hooks/use-session";
import { settingsApi } from "@/features/settings/api";
import { matchesApi } from "@/features/matches/api/matches-api";
import {
  AuditError,
  AuditHistory,
  AuditLoading,
  AuditPagination,
  getImportAudit,
} from "@/features/audit";
import {
  importsApi,
  importQueryKeys,
} from "@/features/imports/api/imports-api";
import type {
  ImportAction,
  ImportCapabilities,
  ImportFilters,
  ImportJob,
  ImportSourceSystem,
  ImportStatus,
  ImportType,
} from "@/features/imports/types";

const types: Record<ImportType, string> = {
  PLAYER_ROSTER: "Spisak igrača",
  MATCH_PLAYER_STATISTICS: "Statistika igrača za utakmicu",
  MATCH_GPS: "GPS / fizički podaci utakmice",
  TRAINING_GPS: "GPS / fizički podaci treninga",
};
const sources: Record<ImportSourceSystem, string> = {
  GENERIC: "Generički format",
  GPEXE: "Gpexe",
  ZONE14: "Zone14",
  OTHER: "Drugi izvor",
};
const statuses: Record<ImportStatus, string> = {
  UPLOADED: "Učitan",
  PARSING: "Obrada u toku",
  VALIDATION_FAILED: "Validacija nije prošla",
  READY_TO_CONFIRM: "Spremno za potvrdu",
  IMPORTED: "Importovan",
  FAILED: "Obrada nije uspjela",
  CANCELLED: "Otkazan",
};
const filtersParser = {
  search: parseAsString,
  teamId: parseAsString,
  matchId: parseAsString,
  trainingSessionId: parseAsString,
  importType: parseAsString,
  sourceSystem: parseAsString,
  fileFormat: parseAsString,
  status: parseAsString,
  dateFrom: parseAsString,
  dateTo: parseAsString,
  page: parseAsInteger.withDefault(1),
  importJobId: parseAsString,
  detailTab: parseAsString,
  previewPage: parseAsInteger.withDefault(1),
  issuesPage: parseAsInteger.withDefault(1),
  issueSeverity: parseAsString,
  auditPage: parseAsInteger.withDefault(1),
};
function matchesRequired(type: string | null) {
  return type === "MATCH_PLAYER_STATISTICS" || type === "MATCH_GPS";
}
function size(bytes: number) {
  return new Intl.NumberFormat("bs-BA", {
    style: "unit",
    unit: "megabyte",
    unitDisplay: "narrow",
    maximumFractionDigits: 1,
  }).format(bytes / 1024 / 1024);
}
function errorMessage(error: unknown) {
  return isApiError(error)
    ? (error.detail ?? error.title)
    : "Zahtjev nije uspio. Pokušajte ponovo.";
}

export function ImportsPage() {
  const { user, isLoading: sessionLoading } = useSession();
  const [url, setUrl] = useQueryStates(filtersParser);
  const filters: ImportFilters = {
    ...url,
    importType: url.importType as ImportType | null,
    sourceSystem: url.sourceSystem as ImportSourceSystem | null,
    fileFormat: url.fileFormat as "CSV" | "XLSX" | null,
    status: url.status as ImportStatus | null,
  };
  const canUse =
    user?.primaryRole === "ADMIN" ||
    (user?.primaryRole === "DATA_OPERATOR" && user.permissions.canImportData);
  const capabilities = useQuery({
    queryKey: importQueryKeys.capabilities,
    queryFn: importsApi.capabilities,
    enabled: canUse,
  });
  const list = useQuery({
    queryKey: importQueryKeys.list(filters),
    queryFn: () => importsApi.list(filters),
    enabled: canUse,
  });
  const teams = useQuery({
    queryKey: ["teams", "imports"],
    queryFn: () => settingsApi.listTeams(false),
    enabled: canUse,
  });
  const matches = useQuery({
    queryKey: ["matches", "imports", url.teamId],
    queryFn: () =>
      matchesApi.list({ teamId: url.teamId, page: 1, pageSize: 100 }),
    enabled: canUse && Boolean(url.teamId),
  });
  const setFilter = (next: Partial<typeof url>) =>
    void setUrl({
      ...next,
      ...(Object.keys(next).some(
        (key) => key !== "page" && key !== "importJobId",
      )
        ? { page: 1 }
        : {}),
    });
  if (sessionLoading) return <LoadingState />;
  if (!canUse)
    return (
      <Alert variant="destructive">
        <AlertTitle>Pristup nije dozvoljen</AlertTitle>
        <AlertDescription>
          Nemate ovlaštenje za import podataka.
        </AlertDescription>
      </Alert>
    );
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Importi"
        description="Učitajte izvorne CSV/XLSX fajlove, pregledajte validaciju i eksplicitno potvrdite podržane importe."
        actions={
          <CreateImport
            disabled={capabilities.isError || !capabilities.data}
            capabilities={capabilities.data}
            teams={teams.data ?? []}
            onCreated={(job) => void setUrl({ importJobId: job.id })}
          />
        }
      />
      {capabilities.isError ? (
        <Alert variant="destructive">
          <AlertTitle>Mogućnosti importa nisu dostupne</AlertTitle>
          <AlertDescription className="flex flex-wrap gap-3">
            Historija importa je i dalje dostupna.{" "}
            <Button
              size="sm"
              variant="outline"
              onClick={() => void capabilities.refetch()}
            >
              Pokušaj ponovo
            </Button>
          </AlertDescription>
        </Alert>
      ) : null}
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 md:flex md:flex-wrap">
        <Input
          aria-label="Pretraži importe"
          className="w-full md:w-56"
          value={url.search ?? ""}
          onChange={(event) =>
            setFilter({ search: event.target.value || null })
          }
          placeholder="Pretraži"
        />
        <FilterSelect
          label="Vrsta importa"
          emptyLabel="Sve vrste importa"
          value={url.importType}
          options={Object.fromEntries(
            capabilities.data?.importTypes.map((value) => [
              value,
              types[value],
            ]) ?? [],
          )}
          className="w-full md:w-48"
          onChange={(value) => setFilter({ importType: value })}
        />
        <FilterSelect
          label="Izvor"
          emptyLabel="Svi izvori"
          value={url.sourceSystem}
          options={Object.fromEntries(
            capabilities.data?.sourceSystems.map((value) => [
              value,
              sources[value],
            ]) ?? [],
          )}
          className="w-full md:w-40"
          onChange={(value) => setFilter({ sourceSystem: value })}
        />
        <FilterSelect
          label="Format"
          emptyLabel="Svi formati"
          value={url.fileFormat}
          options={Object.fromEntries(
            capabilities.data?.fileTypes.map((value) => [
              value.fileFormat,
              value.fileFormat === "CSV" ? "CSV" : "Excel (.xlsx)",
            ]) ?? [],
          )}
          className="w-full md:w-40"
          onChange={(value) => setFilter({ fileFormat: value })}
        />
        <FilterSelect
          label="Status importa"
          emptyLabel="Svi statusi"
          value={url.status}
          options={statuses}
          className="w-full md:w-44"
          onChange={(value) => setFilter({ status: value })}
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={url.teamId}
          options={Object.fromEntries(
            teams.data?.map((team) => [team.id, team.name]) ?? [],
          )}
          className="w-full md:w-48"
          onChange={(teamId) => setFilter({ teamId, matchId: null })}
        />
        <FilterSelect
          label="Utakmica"
          emptyLabel="Sve utakmice"
          value={url.matchId}
          items={
            matches.data?.items.map((match) => [
              match.id,
              `${match.team.name} – ${match.opponent.name}`,
            ]) ?? []
          }
          className="w-full md:w-52"
          onChange={(matchId) => setFilter({ matchId })}
        />
        <DatePicker
          id="import-date-from"
          aria-label="Datum od"
          value={url.dateFrom ?? undefined}
          onChange={(dateFrom) => setFilter({ dateFrom })}
          placeholder="Datum od"
          className="w-full md:w-40"
        />
        <DatePicker
          id="import-date-to"
          aria-label="Datum do"
          value={url.dateTo ?? undefined}
          onChange={(dateTo) => setFilter({ dateTo })}
          placeholder="Datum do"
          className="w-full md:w-40"
        />
        {Object.entries(url).some(
          ([key, value]) => key !== "page" && key !== "importJobId" && value,
        ) ? (
          <Button
            className="md:ml-auto"
            variant="outline"
            onClick={() =>
              void setUrl({
                search: null,
                teamId: null,
                matchId: null,
                importType: null,
                sourceSystem: null,
                fileFormat: null,
                status: null,
                dateFrom: null,
                dateTo: null,
                page: 1,
              })
            }
          >
            Očisti filtere
          </Button>
        ) : null}
      </section>
      {list.isLoading ? (
        <LoadingState />
      ) : list.isError ? (
        <ErrorState
          action={
            <Button onClick={() => void list.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : list.data ? (
        <ImportTable
          data={list.data.items}
          teams={teams.data ?? []}
          matches={matches.data?.items ?? []}
          onOpen={(id) => void setUrl({ importJobId: id })}
        />
      ) : null}
      {list.data && list.data.totalPages > 1 ? (
        <div className="flex justify-between">
          <span className="text-muted-foreground text-sm">
            Stranica {list.data.page} od {list.data.totalPages} ·{" "}
            {list.data.totalCount} importa
          </span>
          <div className="flex gap-2">
            <Button
              size="sm"
              variant="outline"
              disabled={list.data.page <= 1}
              onClick={() => setFilter({ page: list.data!.page - 1 })}
            >
              Prethodna
            </Button>
            <Button
              size="sm"
              variant="outline"
              disabled={list.data.page >= list.data.totalPages}
              onClick={() => setFilter({ page: list.data!.page + 1 })}
            >
              Sljedeća
            </Button>
          </div>
        </div>
      ) : null}
      <ImportDetail
        id={url.importJobId}
        open={Boolean(url.importJobId)}
        tab={url.detailTab}
        previewPage={url.previewPage}
        issuesPage={url.issuesPage}
        issueSeverity={url.issueSeverity}
        auditPage={url.auditPage}
        onTab={(detailTab) => void setUrl({ detailTab })}
        onPreviewPage={(previewPage) => void setUrl({ previewPage })}
        onIssuesPage={(issuesPage) => void setUrl({ issuesPage })}
        onSeverity={(issueSeverity) =>
          void setUrl({ issueSeverity, issuesPage: 1 })
        }
        onAuditPage={(auditPage) => void setUrl({ auditPage })}
        onClose={() => void setUrl({ importJobId: null })}
      />
    </div>
  );
}
function Filter({
  label,
  value,
  items,
  onChange,
}: {
  label: string;
  value: string | null;
  items: Array<[string, string]>;
  onChange: (value: string | null) => void;
}) {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Select
        value={value ?? "all"}
        onValueChange={(next) => onChange(next === "all" ? null : next)}
      >
        <SelectTrigger className="w-full">
          <SelectValue>
            {value
              ? (items.find(([code]) => code === value)?.[1] ?? value)
              : "Sve"}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectItem value="all">Sve</SelectItem>
            {items.map(([code, label]) => (
              <SelectItem key={code} value={code}>
                {label}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </Field>
  );
}
function ImportTable({
  data,
  teams,
  matches,
  onOpen,
}: {
  data: ImportJob[];
  teams: { id: string; name: string }[];
  matches: { id: string; team: { name: string }; opponent: { name: string } }[];
  onOpen: (id: string) => void;
}) {
  const teamName = new Map(teams.map((team) => [team.id, team.name]));
  const matchName = new Map(
    matches.map((match) => [
      match.id,
      `${match.team.name} – ${match.opponent.name}`,
    ]),
  );
  return (
    <section className="overflow-x-auto rounded-xl border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Datum</TableHead>
            <TableHead>Izvorni fajl</TableHead>
            <TableHead>Selekcija</TableHead>
            <TableHead>Utakmica</TableHead>
            <TableHead>Vrsta</TableHead>
            <TableHead>Izvor</TableHead>
            <TableHead>Format</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Sažetak</TableHead>
            <TableHead>Radnja</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.length ? (
            data.map((job) => (
              <TableRow key={job.id}>
                <TableCell>{formatUtcDateTime(job.createdAtUtc)}</TableCell>
                <TableCell>{job.originalFileName}</TableCell>
                <TableCell>{teamName.get(job.teamId) ?? "—"}</TableCell>
                <TableCell>{matchName.get(job.matchId ?? "") ?? "—"}</TableCell>
                <TableCell>{types[job.importType]}</TableCell>
                <TableCell>{sources[job.sourceSystem]}</TableCell>
                <TableCell>
                  {job.fileFormat === "CSV" ? "CSV" : "Excel (.xlsx)"}
                </TableCell>
                <TableCell>
                  <Badge variant="secondary">{statuses[job.status]}</Badge>
                </TableCell>
                <TableCell>
                  {job.invalidRowCount === null
                    ? "—"
                    : `${job.validRowCount ?? 0} ispravnih · ${job.invalidRowCount} neispravnih`}
                </TableCell>
                <TableCell>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => onOpen(job.id)}
                  >
                    Otvori
                  </Button>
                </TableCell>
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell
                colSpan={10}
                className="text-muted-foreground py-10 text-center"
              >
                Nema importa za odabrane filtere.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </section>
  );
}

export function CreateImport({
  disabled,
  capabilities,
  teams,
  onCreated,
  context,
}: {
  disabled: boolean;
  capabilities: ImportCapabilities | undefined;
  teams: { id: string; name: string }[];
  onCreated: (job: ImportJob) => void;
  context?: {
    teamId: string;
    matchId?: string;
    trainingSessionId?: string;
    importType: "MATCH_GPS" | "TRAINING_GPS";
  };
}) {
  const [open, setOpen] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [type, setType] = useState<ImportType | null>(
    context?.importType ?? null,
  );
  const [source, setSource] = useState<ImportSourceSystem | null>(null);
  const [teamId, setTeamId] = useState(context?.teamId ?? "");
  const [matchId, setMatchId] = useState(context?.matchId ?? "");
  const [sourceLabel, setSourceLabel] = useState("");
  const [description, setDescription] = useState("");
  const [progress, setProgress] = useState<number | null>(null);
  const [controller, setController] = useState<AbortController | null>(null);
  const client = useQueryClient();
  const matchOptions = useQuery({
    queryKey: ["matches", "import-create", teamId],
    queryFn: () => matchesApi.list({ teamId, page: 1, pageSize: 100 }),
    enabled: Boolean(teamId),
  });
  const accepted =
    capabilities?.fileTypes.flatMap((item) => item.extensions).join(",") ?? "";
  const submit = async () => {
    if (
      !file ||
      !type ||
      !source ||
      !teamId ||
      (matchesRequired(type) && !matchId) ||
      (source === "OTHER" && !sourceLabel)
    )
      return;
    if (file.size === 0) {
      toast.error(
        "Odabrani fajl je prazan. Odaberite CSV ili XLSX fajl sa sadržajem.",
      );
      return;
    }
    const abort = new AbortController();
    setController(abort);
    setProgress(0);
    try {
      const job = await importsApi.upload(
        {
          teamId,
          matchId: matchesRequired(type) ? matchId : null,
          trainingSessionId:
            type === "TRAINING_GPS"
              ? (context?.trainingSessionId ?? null)
              : null,
          importType: type,
          sourceSystem: source,
          sourceLabel: source === "OTHER" ? sourceLabel : null,
          description: description || null,
        },
        file,
        setProgress,
        abort.signal,
      );
      await client.invalidateQueries({ queryKey: ["importList"] });
      await invalidateDashboardOverview(client);
      setOpen(false);
      onCreated(job);
      toast.success("Izvorni fajl je učitan.");
    } catch (error) {
      toast.error(
        abort.signal.aborted
          ? "Učitavanje je prekinuto. Server je možda završio zahtjev; historija je osvježena."
          : errorMessage(error),
      );
      await client.invalidateQueries({ queryKey: ["importList"] });
      await invalidateDashboardOverview(client);
    } finally {
      setProgress(null);
      setController(null);
    }
  };
  return (
    <>
      <Button disabled={disabled} onClick={() => setOpen(true)}>
        <UploadIcon data-icon="inline-start" />
        {context ? "Učitaj GPS podatke" : "Novi import"}
      </Button>
      <Dialog
        open={open}
        onOpenChange={(next) => {
          if (progress === null) setOpen(next);
        }}
      >
        <DialogContent className="flex max-h-[calc(100dvh-2rem)] max-w-lg flex-col gap-0 overflow-hidden p-0">
          <DialogHeader className="shrink-0 p-4">
            <DialogTitle>
              {context ? "Učitaj GPS podatke" : "Novi import"}
            </DialogTitle>
            <DialogDescription>
              Prvo odaberite izvorni fajl, zatim njegov nepromjenjivi kontekst.
            </DialogDescription>
          </DialogHeader>
          <div className="min-h-0 flex-1 overflow-y-auto px-4 pb-4">
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor="import-file">Odaberite fajl</FieldLabel>
                <Input
                  id="import-file"
                  type="file"
                  accept={accepted}
                  onChange={(event) => setFile(event.target.files?.[0] ?? null)}
                />
                <p className="text-muted-foreground text-xs">
                  Podržano: {accepted || "nije dostupno"}. Maksimalno{" "}
                  {capabilities ? size(capabilities.maxUploadSizeBytes) : "—"}.
                </p>
              </Field>
              {context ? (
                <Field>
                  <FieldLabel>Vrsta importa</FieldLabel>
                  <p className="text-sm">{types[context.importType]}</p>
                </Field>
              ) : (
                <Filter
                  label="Vrsta importa"
                  value={type}
                  items={
                    capabilities?.importTypes.map((value) => [
                      value,
                      types[value],
                    ]) ?? []
                  }
                  onChange={(value) => setType(value as ImportType | null)}
                />
              )}
              <Filter
                label="Izvor"
                value={source}
                items={
                  capabilities?.sourceSystems.map((value) => [
                    value,
                    sources[value],
                  ]) ?? []
                }
                onChange={(value) =>
                  setSource(value as ImportSourceSystem | null)
                }
              />
              {context ? (
                <Field>
                  <FieldLabel>Selekcija</FieldLabel>
                  <p className="text-sm">
                    {teams.find((team) => team.id === context.teamId)?.name ??
                      "Nije dostupno"}
                  </p>
                </Field>
              ) : (
                <Filter
                  label="Selekcija"
                  value={teamId || null}
                  items={teams.map((team) => [team.id, team.name])}
                  onChange={(value) => {
                    setTeamId(value ?? "");
                    setMatchId("");
                  }}
                />
              )}
              {matchesRequired(type) ? (
                <Filter
                  label="Utakmica"
                  value={matchId || null}
                  items={
                    matchOptions.data?.items.map((match) => [
                      match.id,
                      `${match.team.name} – ${match.opponent.name}`,
                    ]) ?? []
                  }
                  onChange={(value) => setMatchId(value ?? "")}
                />
              ) : null}
              {source === "OTHER" ? (
                <Field>
                  <FieldLabel htmlFor="source-label">Naziv izvora</FieldLabel>
                  <Input
                    id="source-label"
                    value={sourceLabel}
                    onChange={(event) => setSourceLabel(event.target.value)}
                  />
                </Field>
              ) : null}
              <Field>
                <FieldLabel htmlFor="import-description">Opis</FieldLabel>
                <Input
                  id="import-description"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                />
              </Field>
              {progress !== null ? (
                <Field>
                  <FieldLabel>Učitavanje: {progress}%</FieldLabel>
                  <Progress value={progress} aria-label="Napredak učitavanja" />
                </Field>
              ) : null}
            </FieldGroup>
          </div>
          <DialogFooter className="mx-0 mb-0 shrink-0 rounded-b-xl">
            <Button
              variant="outline"
              disabled={progress !== null}
              onClick={() => setOpen(false)}
            >
              Odustani
            </Button>
            {progress !== null ? (
              <Button variant="outline" onClick={() => controller?.abort()}>
                Prekini učitavanje
              </Button>
            ) : (
              <Button
                disabled={
                  !file ||
                  !type ||
                  !source ||
                  !teamId ||
                  (matchesRequired(type) && !matchId) ||
                  (source === "OTHER" && !sourceLabel) ||
                  (file
                    ? file.size > (capabilities?.maxUploadSizeBytes ?? 0)
                    : false)
                }
                onClick={() => void submit()}
              >
                Pokreni import
              </Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

function ImportDetail({
  id,
  open,
  tab,
  previewPage,
  issuesPage,
  issueSeverity,
  auditPage,
  onTab,
  onPreviewPage,
  onIssuesPage,
  onSeverity,
  onAuditPage,
  onClose,
}: {
  id: string | null;
  open: boolean;
  tab: string | null;
  previewPage: number;
  issuesPage: number;
  issueSeverity: string | null;
  auditPage: number;
  onTab: (value: string | null) => void;
  onPreviewPage: (value: number) => void;
  onIssuesPage: (value: number) => void;
  onSeverity: (value: string | null) => void;
  onAuditPage: (value: number) => void;
  onClose: () => void;
}) {
  const client = useQueryClient();
  const detail = useQuery({
    queryKey: importQueryKeys.detail(id ?? ""),
    queryFn: () => importsApi.get(id!),
    enabled: Boolean(id),
    refetchInterval: (query) =>
      query.state.data?.status === "PARSING" ? 5000 : false,
  });
  const [confirm, setConfirm] = useState<ImportAction | null>(null);
  const mutation = useMutation({
    mutationFn: ({ action, jobId }: { action: ImportAction; jobId: string }) =>
      action === "GENERATE_PREVIEW"
        ? importsApi.previewAction(jobId)
        : action === "VALIDATE"
          ? importsApi.validate(jobId)
          : action === "CONFIRM"
            ? importsApi.confirm(jobId)
            : importsApi.cancel(jobId),
    onSuccess: async () => {
      await client.invalidateQueries({ queryKey: ["importDetail"] });
      await client.invalidateQueries({ queryKey: ["importList"] });
      await invalidateDashboardOverview(client);
      toast.success("Tok importa je ažuriran.");
    },
    onError: async (error) => {
      toast.error(errorMessage(error));
      await client.invalidateQueries({ queryKey: ["importDetail"] });
    },
  });
  const job = detail.data;
  const run = (action: ImportAction) => {
    if (!id) return;
    if (action === "CONFIRM" || action === "CANCEL") setConfirm(action);
    else mutation.mutate({ action, jobId: id });
  };
  return (
    <>
      <Sheet
        open={open}
        onOpenChange={(value) => {
          if (!value) onClose();
        }}
      >
        <SheetContent className="w-full sm:max-w-3xl">
          <SheetHeader>
            <SheetTitle>{job?.originalFileName ?? "Import"}</SheetTitle>
            <SheetDescription>
              {job
                ? `${types[job.importType]} · ${sources[job.sourceSystem]}`
                : "Učitavanje detalja importa"}
            </SheetDescription>
          </SheetHeader>
          <div className="overflow-y-auto px-4 pb-4">
            {detail.isLoading ? (
              <LoadingState />
            ) : detail.isError ? (
              <ErrorState
                action={
                  <Button onClick={() => void detail.refetch()}>
                    Pokušaj ponovo
                  </Button>
                }
              />
            ) : job ? (
              <div className="flex flex-col gap-5">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant="secondary">{statuses[job.status]}</Badge>
                  <span className="text-muted-foreground text-sm">
                    {size(job.sizeBytes)} ·{" "}
                    {formatUtcDateTime(job.createdAtUtc)}
                  </span>
                </div>
                <section className="rounded-xl border p-4">
                  <h3 className="font-medium">Izvorni fajl</h3>
                  <p className="text-muted-foreground mt-1 text-sm">
                    Izvor ostaje sačuvan za ovlašteni operativni pregled.
                  </p>
                  <Button
                    className="mt-3"
                    variant="outline"
                    render={<a href={importsApi.sourceUrl(job.id)} download />}
                  >
                    Preuzmi izvorni fajl
                  </Button>
                </section>
                {job.status === "PARSING" ? (
                  <Alert>
                    <AlertTitle>Obrada u toku</AlertTitle>
                    <AlertDescription>
                      Prikaz se osvježava svakih 5 sekundi dok backend ne završi
                      obradu.
                    </AlertDescription>
                  </Alert>
                ) : null}
                {job.failureMessage ? (
                  <Alert variant="destructive">
                    <AlertTitle>
                      {job.failureCode ?? "Obrada nije uspjela"}
                    </AlertTitle>
                    <AlertDescription>{job.failureMessage}</AlertDescription>
                  </Alert>
                ) : null}
                <section className="rounded-xl border p-4">
                  <h3 className="font-medium">Tok importa</h3>
                  <ol className="text-muted-foreground mt-3 flex flex-col gap-2 text-sm">
                    <li>1. Učitan izvor</li>
                    <li>2. Pregled i validacija kada ih backend podrži</li>
                    <li>3. Eksplicitna potvrda kada je dostupna</li>
                  </ol>
                </section>
                {job.allowedActions.length ? (
                  <div className="flex flex-wrap gap-2">
                    {job.allowedActions
                      .filter((action): action is ImportAction =>
                        [
                          "GENERATE_PREVIEW",
                          "VALIDATE",
                          "CONFIRM",
                          "CANCEL",
                        ].includes(action),
                      )
                      .map((action) => (
                        <Button
                          key={action}
                          variant={action === "CANCEL" ? "outline" : "default"}
                          disabled={mutation.isPending}
                          onClick={() => run(action)}
                        >
                          {
                            (
                              {
                                GENERATE_PREVIEW: "Generiši pregled",
                                VALIDATE: "Validiraj import",
                                CONFIRM: "Potvrdi import",
                                CANCEL: "Otkaži import",
                              } as Record<ImportAction, string>
                            )[action]
                          }
                        </Button>
                      ))}
                  </div>
                ) : (
                  <Alert>
                    <AlertTitle>Procesor nije dostupan</AlertTitle>
                    <AlertDescription>
                      Ovaj izvor je sačuvan, ali obrada za tačnu kombinaciju
                      vrste, izvora i formata trenutno nije podržana.
                    </AlertDescription>
                  </Alert>
                )}
                <ImportResults job={job} />
                <ImportDetailTabs
                  job={job}
                  tab={tab}
                  previewPage={previewPage}
                  issuesPage={issuesPage}
                  issueSeverity={issueSeverity}
                  auditPage={auditPage}
                  onTab={onTab}
                  onPreviewPage={onPreviewPage}
                  onIssuesPage={onIssuesPage}
                  onSeverity={onSeverity}
                  onAuditPage={onAuditPage}
                />
              </div>
            ) : null}
          </div>
        </SheetContent>
      </Sheet>
      <Dialog
        open={confirm !== null}
        onOpenChange={(value) => {
          if (!value) setConfirm(null);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {confirm === "CONFIRM" ? "Potvrdi import" : "Otkaži import"}
            </DialogTitle>
            <DialogDescription>
              {confirm === "CONFIRM"
                ? "Potvrda primjenjuje samo backend-odobren import. Status se ne šalje iz preglednika."
                : "Otkazani import ostaje terminalan, a izvorni fajl i historija ostaju dostupni."}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirm(null)}>
              Nazad
            </Button>
            <Button
              disabled={mutation.isPending}
              onClick={() => {
                if (confirm && id)
                  mutation.mutate(
                    { action: confirm, jobId: id },
                    { onSettled: () => setConfirm(null) },
                  );
              }}
            >
              {confirm === "CONFIRM" ? "Potvrdi import" : "Otkaži import"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
function ImportResults({ job }: { job: ImportJob }) {
  if (job.totalRowCount === null && !job.previewGeneratedAtUtc) return null;
  return (
    <section className="rounded-xl border p-4">
      <h3 className="font-medium">Rezultat i validacija</h3>
      <dl className="mt-3 grid gap-2 text-sm sm:grid-cols-2">
        <div>Ukupno redova: {job.totalRowCount ?? "—"}</div>
        <div>Pregled: {job.previewRowCount ?? "—"}</div>
        <div>Ispravni: {job.validRowCount ?? "—"}</div>
        <div>Neispravni: {job.invalidRowCount ?? "—"}</div>
        <div>Upozorenja: {job.warningCount ?? "—"}</div>
      </dl>
    </section>
  );
}

const importAuditActions = {
  IMPORT_JOB_UPLOADED: "Import učitan",
  IMPORT_JOB_PREVIEW_GENERATED: "Pregled generisan",
  IMPORT_JOB_VALIDATED: "Import validiran",
  IMPORT_JOB_READY_TO_CONFIRM: "Import spreman za potvrdu",
  IMPORT_JOB_CONFIRMED: "Import potvrđen",
  IMPORT_JOB_CANCELLED: "Import otkazan",
  IMPORT_JOB_PROCESSING_FAILED: "Obrada importa nije uspjela",
};
const importAuditFields = {
  teamId: "Selekcija",
  matchId: "Utakmica",
  importType: "Vrsta importa",
  sourceSystem: "Izvor",
  sourceLabel: "Naziv izvora",
  fileFormat: "Format",
  status: "Status",
  originalFileName: "Izvorni fajl",
  contentType: "Tip sadržaja",
  sizeBytes: "Veličina",
  processorKey: "Procesor",
  processorVersion: "Verzija procesora",
  configurationRevision: "Revizija konfiguracije",
  totalRowCount: "Ukupno redova",
  previewRowCount: "Redovi u pregledu",
  validRowCount: "Ispravni redovi",
  invalidRowCount: "Neispravni redovi",
  warningCount: "Upozorenja",
  failureCode: "Kod greške",
};
function ImportDetailTabs({
  job,
  tab,
  previewPage,
  issuesPage,
  issueSeverity,
  auditPage,
  onTab,
  onPreviewPage,
  onIssuesPage,
  onSeverity,
  onAuditPage,
}: {
  job: ImportJob;
  tab: string | null;
  previewPage: number;
  issuesPage: number;
  issueSeverity: string | null;
  auditPage: number;
  onTab: (value: string | null) => void;
  onPreviewPage: (value: number) => void;
  onIssuesPage: (value: number) => void;
  onSeverity: (value: string | null) => void;
  onAuditPage: (value: number) => void;
}) {
  const active = tab === "validation" || tab === "audit" ? tab : "preview";
  const preview = useQuery({
    queryKey: importQueryKeys.preview(job.id, previewPage),
    queryFn: () => importsApi.preview(job.id, previewPage),
    enabled: active === "preview" && Boolean(job.previewGeneratedAtUtc),
  });
  const issues = useQuery({
    queryKey: importQueryKeys.issues(job.id, issuesPage, issueSeverity),
    queryFn: () => importsApi.issues(job.id, issuesPage, issueSeverity),
    enabled: active === "validation" && Boolean(job.validationCompletedAtUtc),
  });
  const audit = useQuery({
    queryKey: importQueryKeys.audit(job.id, auditPage),
    queryFn: () => getImportAudit(job.id, { page: auditPage }),
    enabled: active === "audit",
  });
  const teams = useQuery({
    queryKey: ["teams", "import-audit"],
    queryFn: () => settingsApi.listTeams(false),
    enabled: active === "audit",
  });
  const match = useQuery({
    queryKey: ["match", "import-audit", job.matchId],
    queryFn: () => matchesApi.get(job.matchId!),
    enabled: active === "audit" && Boolean(job.matchId),
  });
  const formatAuditValue = (key: string, value: unknown) => {
    if (value === null || value === undefined) return "Nije postavljeno";
    if (key === "teamId" && typeof value === "string")
      return (
        teams.data?.find((team) => team.id === value)?.name ??
        "Selekcija nije dostupna"
      );
    if (key === "matchId" && typeof value === "string")
      return match.data?.id === value
        ? `${match.data.team.name} – ${match.data.opponent.name}`
        : "Utakmica nije dostupna";
    if (key === "importType") {
      const code =
        typeof value === "number"
          ? (
              [
                "PLAYER_ROSTER",
                "MATCH_PLAYER_STATISTICS",
                "MATCH_GPS",
                "TRAINING_GPS",
              ] as const
            )[value]
          : value;
      return typeof code === "string"
        ? (types[code as ImportType] ?? code)
        : undefined;
    }
    if (key === "sourceSystem") {
      const code =
        typeof value === "number"
          ? (["GENERIC", "GPEXE", "ZONE14", "OTHER"] as const)[value]
          : value;
      return typeof code === "string"
        ? (sources[code as ImportSourceSystem] ?? code)
        : undefined;
    }
    if (key === "fileFormat") {
      const code =
        typeof value === "number" ? (["CSV", "XLSX"] as const)[value] : value;
      return code === "CSV"
        ? "CSV"
        : code === "XLSX"
          ? "Excel (.xlsx)"
          : typeof code === "string"
            ? code
            : undefined;
    }
    if (key === "status") {
      const code =
        typeof value === "number"
          ? (
              [
                "UPLOADED",
                "PARSING",
                "VALIDATION_FAILED",
                "READY_TO_CONFIRM",
                "IMPORTED",
                "FAILED",
                "CANCELLED",
              ] as const
            )[value]
          : value;
      return typeof code === "string"
        ? (statuses[code as ImportStatus] ?? code)
        : undefined;
    }
    if (key === "sizeBytes" && typeof value === "number") return size(value);
    if (key === "contentType" && typeof value === "string")
      return value === "text/csv" || value === "application/csv"
        ? "CSV"
        : value ===
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
          ? "Excel (.xlsx)"
          : "Prepoznat format";
    if (
      key === "sourceLabel" ||
      key === "originalFileName" ||
      key === "processorKey" ||
      key === "processorVersion" ||
      key === "failureCode"
    )
      return typeof value === "string" ? value : undefined;
    return undefined;
  };
  return (
    <Tabs
      value={active}
      onValueChange={(value) => onTab(value === "preview" ? null : value)}
    >
      <TabsList>
        <TabsTrigger value="preview">Pregled</TabsTrigger>
        <TabsTrigger value="validation">Validacija</TabsTrigger>
        <TabsTrigger value="audit">Historija promjena</TabsTrigger>
      </TabsList>
      <div className="mt-4">
        {active === "preview" ? (
          <PreviewView
            job={job}
            data={preview.data}
            loading={preview.isLoading}
            error={preview.isError}
            retry={() => void preview.refetch()}
            onPage={onPreviewPage}
          />
        ) : active === "validation" ? (
          <ValidationView
            job={job}
            data={issues.data}
            loading={issues.isLoading}
            error={issues.isError}
            severity={issueSeverity}
            onSeverity={onSeverity}
            retry={() => void issues.refetch()}
            onPage={onIssuesPage}
          />
        ) : audit.isLoading ? (
          <AuditLoading />
        ) : audit.isError ? (
          <AuditError retry={() => void audit.refetch()} />
        ) : (
          <div className="flex flex-col gap-4">
            <h3 className="font-medium">Tok importa</h3>
            <AuditHistory
              data={audit.data}
              labels={importAuditActions}
              fields={importAuditFields}
              expectedEntityType="IMPORT_JOB"
              formatValue={formatAuditValue}
            />
            <>
              {audit.data ? (
                <AuditPagination data={audit.data} onPage={onAuditPage} />
              ) : null}
            </>
          </div>
        )}
      </div>
    </Tabs>
  );
}
function PreviewView({
  job,
  data,
  loading,
  error,
  retry,
  onPage,
}: {
  job: ImportJob;
  data: import("@/features/imports/types").Preview | undefined;
  loading: boolean;
  error: boolean;
  retry: () => void;
  onPage: (page: number) => void;
}) {
  if (!job.previewGeneratedAtUtc)
    return (
      <Alert>
        <AlertTitle>Pregled nije generisan</AlertTitle>
        <AlertDescription>
          Pregled će biti dostupan samo nakon backend-odobrene radnje.
        </AlertDescription>
      </Alert>
    );
  if (loading) return <LoadingState />;
  if (error)
    return (
      <ErrorState action={<Button onClick={retry}>Pokušaj ponovo</Button>} />
    );
  if (!data?.rows.length)
    return (
      <Alert>
        <AlertTitle>Nema redova u pregledu</AlertTitle>
        <AlertDescription>
          Backend nije vratio redove za prikaz.
        </AlertDescription>
      </Alert>
    );
  return (
    <div className="overflow-x-auto rounded-xl border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Red</TableHead>
            {data.columns.map((column) => (
              <TableHead key={column.ordinal}>{column.sourceHeader}</TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.rows.map((row) => (
            <TableRow key={row.sourceRowNumber}>
              <TableCell>{row.sourceRowNumber}</TableCell>
              {data.columns.map((column) => (
                <TableCell key={column.ordinal}>
                  {String(
                    row.values[column.normalizedHeader] ??
                      row.values[column.sourceHeader] ??
                      "—",
                  )}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {data.totalCount > data.pageSize ? (
        <div className="flex justify-between p-3">
          <Button
            size="sm"
            variant="outline"
            disabled={data.page <= 1}
            onClick={() => onPage(data.page - 1)}
          >
            Prethodna
          </Button>
          <Button
            size="sm"
            variant="outline"
            disabled={data.page * data.pageSize >= data.totalCount}
            onClick={() => onPage(data.page + 1)}
          >
            Sljedeća
          </Button>
        </div>
      ) : null}
    </div>
  );
}
function ValidationView({
  job,
  data,
  loading,
  error,
  severity,
  onSeverity,
  retry,
  onPage,
}: {
  job: ImportJob;
  data: import("@/features/imports/types").ValidationIssues | undefined;
  loading: boolean;
  error: boolean;
  severity: string | null;
  onSeverity: (value: string | null) => void;
  retry: () => void;
  onPage: (page: number) => void;
}) {
  if (!job.validationCompletedAtUtc)
    return (
      <Alert>
        <AlertTitle>Validacija nije pokrenuta</AlertTitle>
        <AlertDescription>
          Rezultati će biti dostupni nakon backend-odobrene validacije.
        </AlertDescription>
      </Alert>
    );
  if (loading) return <LoadingState />;
  if (error)
    return (
      <ErrorState action={<Button onClick={retry}>Pokušaj ponovo</Button>} />
    );
  return (
    <div className="flex flex-col gap-3">
      <Filter
        label="Ozbiljnost"
        value={severity}
        items={[
          ["ERROR", "Greška"],
          ["WARNING", "Upozorenje"],
        ]}
        onChange={onSeverity}
      />
      <div className="overflow-x-auto rounded-xl border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Ozbiljnost</TableHead>
              <TableHead>Red</TableHead>
              <TableHead>Kolona</TableHead>
              <TableHead>Poruka</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data?.items.length ? (
              data.items.map((issue) => (
                <TableRow
                  key={`${issue.code}-${issue.sourceRowNumber}-${issue.createdAtUtc}`}
                >
                  <TableCell>
                    <Badge variant="secondary">
                      {issue.severity === "ERROR" ? "Greška" : "Upozorenje"}
                    </Badge>
                  </TableCell>
                  <TableCell>{issue.sourceRowNumber ?? "—"}</TableCell>
                  <TableCell>{issue.columnKey ?? "—"}</TableCell>
                  <TableCell>{issue.message}</TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={4}
                  className="text-muted-foreground text-center"
                >
                  Nema validacijskih problema.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
      {data && data.totalPages > 1 ? (
        <div className="flex justify-between">
          <Button
            size="sm"
            variant="outline"
            disabled={data.page <= 1}
            onClick={() => onPage(data.page - 1)}
          >
            Prethodna
          </Button>
          <Button
            size="sm"
            variant="outline"
            disabled={data.page >= data.totalPages}
            onClick={() => onPage(data.page + 1)}
          >
            Sljedeća
          </Button>
        </div>
      ) : null}
    </div>
  );
}
