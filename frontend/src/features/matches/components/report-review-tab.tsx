import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ClipboardCheck,
  Pencil,
  Send,
  ShieldCheck,
  Archive,
} from "lucide-react";
import { useForm } from "react-hook-form";
import { useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { parseAsInteger, parseAsString, useQueryStates } from "nuqs";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { invalidateDashboardOverview } from "@/features/dashboard";

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
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { matchesApi } from "@/features/matches/api/matches-api";
import { useSession } from "@/features/auth/hooks/use-session";
import type {
  MatchLineupResponse,
  MatchResponse,
} from "@/features/matches/types/match";
import {
  isKnownStatisticField,
  statisticFieldRegistry,
} from "@/features/matches/utils/statistics";
import { asAuditRecord } from "@/features/audit/types";
import {
  hasReportAction,
  reportStatusLabels,
} from "@/features/matches/utils/reports";
import { isApiError } from "@/lib/api/api-client";
import { formatUtcDateTime } from "@/lib/date-format";
import {
  AuditError,
  AuditFilterBar,
  AuditHistory,
  AuditLoading,
  AuditPagination,
  getMatchReportAudit,
} from "@/features/audit";

const reportAuditLabels = {
  MATCH_REPORT_CREATED: "Izvještaj je kreiran",
  MATCH_REPORT_SUBMITTED: "Izvještaj je poslan na pregled",
  MATCH_REPORT_VERIFIED: "Izvještaj je verificiran",
  MATCH_REPORT_CORRECTION_REQUESTED: "Zatražena je korekcija",
  MATCH_REPORT_ARCHIVED: "Izvještaj je arhiviran",
  MATCH_REPORT_STATISTICS_UPDATED: "Statistika je ažurirana",
};
const reportAuditFields = {
  status: "Status",
  appliedTrackingLevel: "Nivo praćenja",
  playerStatistics: "Statistika igrača",
  goalkeeperStatistics: "Statistika golmana",
};

function ReportAudit({
  reportId,
  lineup,
}: {
  reportId: string;
  lineup: MatchLineupResponse | undefined;
}) {
  const playersByAppearance = new Map(
    lineup?.appearances.map((appearance) => [
      appearance.id,
      (() => {
        const player = lineup.entries.find(
          (entry) => entry.player.id === appearance.playerId,
        )?.player;
        return player
          ? player.preferredName || `${player.firstName} ${player.lastName}`
          : `Nastup (${appearance.id})`;
      })(),
    ]),
  );
  const formatStatistics = (value: unknown) =>
    Array.isArray(value)
      ? value
          .map((row) => {
            const source = asAuditRecord(row);
            if (!source) return "Nepodržan red statistike";
            const appearanceId =
              typeof source.playerMatchAppearanceId === "string"
                ? source.playerMatchAppearanceId
                : "";
            const values = Object.entries(source)
              .filter(([key]) => key !== "playerMatchAppearanceId")
              .map(
                ([key, item]) =>
                  `${isKnownStatisticField(key) ? statisticFieldRegistry[key].label : key}: ${item === null ? "Nije postavljeno" : item === true ? "Da" : item === false ? "Ne" : String(item)}`,
              )
              .join(", ");
            return `${playersByAppearance.get(appearanceId) ?? `Nastup (${appearanceId || "nepoznat"})`}: ${values}`;
          })
          .join("; ")
      : undefined;
  const [filters, setFilters] = useQueryStates({
    auditAction: parseAsString,
    auditFrom: parseAsString,
    auditTo: parseAsString,
    auditPage: parseAsInteger.withDefault(1),
  });
  const audit = useQuery({
    queryKey: ["match-report-audit", reportId, filters],
    queryFn: () =>
      getMatchReportAudit(reportId, {
        action: filters.auditAction,
        dateFrom: filters.auditFrom,
        dateTo: filters.auditTo,
        page: filters.auditPage,
      }),
    retry: false,
  });
  return (
    <section className="flex flex-col gap-4 rounded-xl border p-5">
      <div>
        <h2 className="font-heading text-lg">Historija promjena</h2>
        <p className="text-muted-foreground text-sm">
          Audit zapisi su prikazani najnoviji prvo.
        </p>
      </div>
      <AuditFilterBar
        actions={reportAuditLabels}
        value={{
          action: filters.auditAction,
          dateFrom: filters.auditFrom,
          dateTo: filters.auditTo,
        }}
        onChange={(next) =>
          void setFilters({
            auditAction: next.action,
            auditFrom: next.dateFrom,
            auditTo: next.dateTo,
            auditPage: 1,
          })
        }
      />
      {audit.isLoading ? (
        <AuditLoading />
      ) : audit.isError ? (
        <AuditError retry={() => void audit.refetch()} />
      ) : (
        <>
          <AuditHistory
            data={audit.data}
            labels={reportAuditLabels}
            fields={reportAuditFields}
            expectedEntityType="MATCH_REPORT"
            formatValue={(key, value) =>
              key === "playerStatistics" || key === "goalkeeperStatistics"
                ? formatStatistics(value)
                : undefined
            }
          />
          <AuditPagination
            data={audit.data!}
            onPage={(page) => void setFilters({ auditPage: page })}
          />
        </>
      )}
    </section>
  );
}

const correctionSchema = z.object({
  reason: z
    .string()
    .trim()
    .min(1, "Unesite razlog korekcije.")
    .max(2000, "Razlog je predugačak."),
});
type CorrectionValues = z.infer<typeof correctionSchema>;
type ConfirmAction = "submit" | "verify" | "archive" | null;

function errorMessage(error: unknown) {
  if (!isApiError(error)) return "Mrežna greška. Pokušajte ponovo.";
  if (error.status === 403) return "Više nemate ovlaštenje za ovu radnju.";
  if (error.status === 404) return "Izvještaj više nije dostupan.";
  if (error.status === 409)
    return "Izvještaj je u međuvremenu promijenjen. Učitano je trenutno stanje.";
  if (error.status === 422)
    return "Nedostaju obavezni nastupi ili potpuna statistika. Provjerite Sastav i Statistiku prije slanja na pregled.";
  return error.detail ?? "Radnju nije moguće dovršiti.";
}

export function ReportReviewTab({
  match,
  onNavigate,
}: {
  match: MatchResponse;
  onNavigate: (tab: "lineup" | "statistics") => void;
}) {
  const queryClient = useQueryClient();
  const { user } = useSession();
  const report = useQuery({
    queryKey: ["match-report", match.id],
    queryFn: () => matchesApi.getReport(match.id),
    retry: false,
  });
  const lineup = useQuery({
    queryKey: ["match-lineup", match.id],
    queryFn: () => matchesApi.getLineup(match.id),
    enabled: !!report.data,
    retry: false,
  });
  const statistics = useQuery({
    queryKey: ["match-report-statistics", report.data?.id],
    queryFn: () => matchesApi.getStatistics(report.data!.id),
    enabled: !!report.data,
    retry: false,
  });
  const [confirm, setConfirm] = useState<ConfirmAction>(null);
  const [correctionOpen, setCorrectionOpen] = useState(false);
  const form = useForm<CorrectionValues>({
    resolver: zodResolver(correctionSchema),
    defaultValues: { reason: "" },
  });
  const [reviewState, setReviewState] = useQueryStates({
    reviewView: parseAsString.withDefault("workflow"),
  });
  const refresh = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["match-report", match.id] }),
      queryClient.invalidateQueries({ queryKey: ["match-reports"] }),
      queryClient.invalidateQueries({
        queryKey: ["match-report-statistics", report.data?.id],
      }),
      queryClient.invalidateQueries({ queryKey: ["match-lineup", match.id] }),
      queryClient.invalidateQueries({ queryKey: ["match", match.id] }),
      queryClient.invalidateQueries({ queryKey: ["match-report-audit"] }),
      invalidateDashboardOverview(queryClient, match.team.id),
    ]);
  };
  const transition = useMutation({
    mutationFn: async (action: Exclude<ConfirmAction, null>) => {
      if (!report.data) throw new Error("Izvještaj nije dostupan.");
      if (action === "submit") return matchesApi.submitReport(report.data.id);
      if (action === "verify") return matchesApi.verifyReport(report.data.id);
      return matchesApi.archiveReport(report.data.id);
    },
    onSuccess: async () => {
      setConfirm(null);
      await refresh();
      toast.success("Status izvještaja je ažuriran.");
    },
    onError: async (error) => {
      await refresh();
      toast.error(errorMessage(error));
    },
  });
  const correction = useMutation({
    mutationFn: (values: CorrectionValues) =>
      matchesApi.requestReportCorrection(report.data!.id, values.reason.trim()),
    onSuccess: async () => {
      setCorrectionOpen(false);
      form.reset();
      await refresh();
      toast.success("Korekcija je zatražena.");
    },
    onError: async (error) => {
      await refresh();
      toast.error(errorMessage(error));
    },
  });
  if (report.isLoading) return <Skeleton className="h-96 w-full" />;
  if (report.isError)
    return (
      <Alert variant="destructive">
        <AlertTitle>Izvještaj nije moguće učitati</AlertTitle>
        <AlertDescription>
          Pokušajte ponovo. Ako problem ostane, izvještaj možda više nije
          dostupan za vaš pristup.
        </AlertDescription>
      </Alert>
    );
  if (!report.data) {
    const canStartReport =
      (user?.primaryRole === "ADMIN" ||
        user?.primaryRole === "DATA_OPERATOR") &&
      match.status === "PLAYED" &&
      !match.isArchived;
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>
            {canStartReport
              ? "Izvještaj još nije započet"
              : "Izvještaj utakmice još nije dostupan"}
          </EmptyTitle>
          <EmptyDescription>
            {canStartReport
              ? "Otvorite Statistiku kako biste započeli unos izvještaja."
              : "Ovaj izvještaj nije vidljiv za vaš pristup ili još nije kreiran."}
          </EmptyDescription>
        </EmptyHeader>
        {canStartReport ? (
          <EmptyContent>
            <Button variant="outline" onClick={() => onNavigate("statistics")}>
              Otvori statistiku
            </Button>
          </EmptyContent>
        ) : null}
      </Empty>
    );
  }
  const data = report.data;
  if (reviewState.reviewView === "audit")
    return (
      <Tabs
        value="audit"
        onValueChange={(reviewView) => void setReviewState({ reviewView })}
      >
        <TabsList>
          <TabsTrigger value="workflow">Tok izvještaja</TabsTrigger>
          <TabsTrigger value="audit">Historija promjena</TabsTrigger>
        </TabsList>
        <ReportAudit reportId={data.id} lineup={lineup.data} />
      </Tabs>
    );
  const canEdit = hasReportAction(data.allowedActions, "EDIT");
  const pending = transition.isPending || correction.isPending;
  const readinessFailed = lineup.isError || statistics.isError;
  return (
    <div className="flex flex-col gap-4">
      <Tabs
        value="workflow"
        onValueChange={(reviewView) => void setReviewState({ reviewView })}
      >
        <TabsList>
          <TabsTrigger value="workflow">Tok izvještaja</TabsTrigger>
          <TabsTrigger value="audit">Historija promjena</TabsTrigger>
        </TabsList>
      </Tabs>
      <section className="border-border bg-card grid gap-4 rounded-xl border p-5 md:grid-cols-2">
        <div className="flex flex-col gap-2">
          <p className="text-muted-foreground text-sm">Status izvještaja</p>
          <Badge variant="secondary" className="w-fit">
            {reportStatusLabels[data.status]}
          </Badge>
          <p className="text-muted-foreground text-sm">
            Pregled spremnosti je informativan; konačnu provjeru vrši server.
          </p>
        </div>
        <div className="flex flex-wrap content-start gap-2">
          {canEdit ? (
            <>
              <Button variant="outline" onClick={() => onNavigate("lineup")}>
                <Pencil data-icon="inline-start" />
                Uredi sastav
              </Button>
              <Button
                variant="outline"
                onClick={() => onNavigate("statistics")}
              >
                <Pencil data-icon="inline-start" />
                Uredi statistiku
              </Button>
            </>
          ) : (
            <>
              <Button variant="outline" onClick={() => onNavigate("lineup")}>
                Pregledaj sastav
              </Button>
              <Button
                variant="outline"
                onClick={() => onNavigate("statistics")}
              >
                Pregledaj statistiku
              </Button>
            </>
          )}
          {hasReportAction(data.allowedActions, "SUBMIT_FOR_REVIEW") ? (
            <Button disabled={pending} onClick={() => setConfirm("submit")}>
              <Send data-icon="inline-start" />
              Pošalji na pregled
            </Button>
          ) : null}
          {hasReportAction(data.allowedActions, "VERIFY") ? (
            <Button disabled={pending} onClick={() => setConfirm("verify")}>
              <ShieldCheck data-icon="inline-start" />
              Verificiraj izvještaj
            </Button>
          ) : null}
          {hasReportAction(data.allowedActions, "REQUEST_CORRECTION") ? (
            <Button
              disabled={pending}
              variant="outline"
              onClick={() => setCorrectionOpen(true)}
            >
              <ClipboardCheck data-icon="inline-start" />
              Zatraži korekciju
            </Button>
          ) : null}
          {hasReportAction(data.allowedActions, "ARCHIVE") ? (
            <Button
              disabled={pending}
              variant="destructive"
              onClick={() => setConfirm("archive")}
            >
              <Archive data-icon="inline-start" />
              Arhiviraj izvještaj
            </Button>
          ) : null}
        </div>
      </section>
      <section className="border-border bg-card rounded-xl border p-5">
        <h2 className="font-heading text-lg">Provjera spremnosti</h2>
        {readinessFailed ? (
          <Alert variant="destructive" className="mt-4">
            <AlertTitle>Dio podataka nije dostupan</AlertTitle>
            <AlertDescription>
              Pregled je djelimičan. Server će provjeriti potpune podatke pri
              slanju.
            </AlertDescription>
          </Alert>
        ) : (
          <div className="mt-4 grid gap-3 sm:grid-cols-2">
            <div>
              <p className="text-muted-foreground text-sm">Utakmica</p>
              <p>
                {match.status === "PLAYED" && !match.isArchived
                  ? "Odigrana i aktivna"
                  : "Nije spremna za izvještaj"}
              </p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Sastav</p>
              <p>
                Početni sastav:{" "}
                {lineup.data?.entries.filter(
                  (entry) => entry.role === "STARTER",
                ).length ?? 0}
              </p>
              <p>
                Zamjene:{" "}
                {lineup.data?.entries.filter(
                  (entry) => entry.role === "SUBSTITUTE",
                ).length ?? 0}
              </p>
              <p>Nastupi: {lineup.data?.appearances.length ?? 0}</p>
              <p>
                Kapiten: {lineup.data?.captain ? "određen" : "nije određen"}
              </p>
              <p>Izmjene: {lineup.data?.substitutions.length ?? 0}</p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Statistika</p>
              <p>
                Nivo praćenja:{" "}
                {statistics.data?.appliedTrackingLevel ?? "nije dostupan"}
              </p>
              <p>
                Potpuni igrači:{" "}
                {statistics.data?.playerStatistics.filter(
                  (row) => row.isComplete,
                ).length ?? 0}{" "}
                / {statistics.data?.appearances.length ?? 0}
              </p>
              <p>
                Golmani: {statistics.data?.goalkeeperStatistics.length ?? 0}
              </p>
              <p>
                Potpuni golmani:{" "}
                {statistics.data?.goalkeeperStatistics.filter(
                  (row) => row.isComplete,
                ).length ?? 0}
              </p>
              <p>
                Stanje:{" "}
                {statistics.data?.isComplete ? "Spremno" : "Nedostaju podaci"}
              </p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Tok izvještaja</p>
              <p>{reportStatusLabels[data.status]}</p>
              <p>
                {data.submitted
                  ? "Poslan na pregled"
                  : "Nije poslan na pregled"}
              </p>
              <p>{data.verified ? "Verificiran" : "Nije verificiran"}</p>
            </div>
          </div>
        )}
      </section>
      {data.lastCorrection ? (
        <Alert>
          <AlertTitle>Zahtjev za korekciju</AlertTitle>
          <AlertDescription>
            {data.lastCorrection.displayName ?? "Nepoznat korisnik"} ·{" "}
            {data.lastCorrection.reason} ·{" "}
            {formatUtcDateTime(data.lastCorrection.atUtc)}
          </AlertDescription>
        </Alert>
      ) : null}
      <section className="border-border bg-card rounded-xl border p-5">
        <h2 className="font-heading text-lg">Podaci o toku izvještaja</h2>
        <dl className="mt-4 grid gap-3 sm:grid-cols-2">
          <div>
            <dt className="text-muted-foreground text-sm">Kreirano</dt>
            <dd>
              {data.createdByDisplayName ?? "Nepoznat korisnik"} ·{" "}
              {formatUtcDateTime(data.createdAtUtc)}
            </dd>
          </div>
          {data.submitted ? (
            <div>
              <dt className="text-muted-foreground text-sm">
                Poslano na pregled
              </dt>
              <dd>
                {data.submitted.displayName ?? "Nepoznat korisnik"} ·{" "}
                {formatUtcDateTime(data.submitted.atUtc)}
              </dd>
            </div>
          ) : null}
          {data.verified ? (
            <div>
              <dt className="text-muted-foreground text-sm">Verificirano</dt>
              <dd>
                {data.verified.displayName ?? "Nepoznat korisnik"} ·{" "}
                {formatUtcDateTime(data.verified.atUtc)}
              </dd>
            </div>
          ) : null}
          {data.archived ? (
            <div>
              <dt className="text-muted-foreground text-sm">Arhivirano</dt>
              <dd>
                {data.archived.displayName ?? "Nepoznat korisnik"} ·{" "}
                {formatUtcDateTime(data.archived.atUtc)}
              </dd>
            </div>
          ) : null}
        </dl>
        <p className="text-muted-foreground mt-4 text-sm">
          Ovo nije potpuna historija revizije.
        </p>
      </section>
      <Dialog
        open={confirm !== null}
        onOpenChange={(open) => {
          if (!open && !transition.isPending) setConfirm(null);
        }}
      >
        <DialogContent showCloseButton={!transition.isPending}>
          <DialogHeader>
            <DialogTitle>
              {confirm === "submit"
                ? "Pošalji izvještaj na pregled"
                : confirm === "verify"
                  ? "Verificiraj izvještaj"
                  : "Arhiviraj izvještaj"}
            </DialogTitle>
            <DialogDescription>
              {confirm === "submit"
                ? "Nakon slanja, uobičajeno uređivanje sastava i statistike bit će zaključano."
                : confirm === "verify"
                  ? "Potvrdite da je izvještaj pregledan i verificiran."
                  : "Arhiviranje je završna radnja izvještaja, nije otkazivanje utakmice. Izvještaj ostaje vidljiv samo za čitanje i ne može se vratiti."}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              disabled={transition.isPending}
              variant="outline"
              onClick={() => setConfirm(null)}
            >
              Odustani
            </Button>
            <Button
              disabled={transition.isPending}
              variant={confirm === "archive" ? "destructive" : "default"}
              onClick={() => confirm && transition.mutate(confirm)}
            >
              {transition.isPending ? "Obrađivanje…" : "Potvrdi"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog
        open={correctionOpen}
        onOpenChange={(open) => {
          if (!open && !correction.isPending) setCorrectionOpen(false);
        }}
      >
        <DialogContent showCloseButton={!correction.isPending}>
          <DialogHeader>
            <DialogTitle>Zatraži korekciju</DialogTitle>
            <DialogDescription>
              Navedite jasan razlog koji će biti prikazan osoblju za unos
              podataka.
            </DialogDescription>
          </DialogHeader>
          <form
            onSubmit={form.handleSubmit((values) => correction.mutate(values))}
          >
            <FieldGroup>
              <Field data-invalid={!!form.formState.errors.reason}>
                <FieldLabel htmlFor="correction-reason">
                  Razlog korekcije
                </FieldLabel>
                <Textarea
                  id="correction-reason"
                  aria-invalid={!!form.formState.errors.reason}
                  disabled={correction.isPending}
                  {...form.register("reason")}
                />
                <FieldDescription>
                  {form.formState.errors.reason?.message}
                </FieldDescription>
              </Field>
            </FieldGroup>
            <DialogFooter>
              <Button
                type="button"
                disabled={correction.isPending}
                variant="outline"
                onClick={() => setCorrectionOpen(false)}
              >
                Odustani
              </Button>
              <Button disabled={correction.isPending} type="submit">
                {correction.isPending ? "Slanje…" : "Pošalji zahtjev"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
