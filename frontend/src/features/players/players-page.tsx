import { zodResolver } from "@hookform/resolvers/zod";
import {
  useMutation,
  useQuery,
  useQueries,
  useQueryClient,
} from "@tanstack/react-query";
import { MoreHorizontal, Plus } from "lucide-react";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { EmptyState } from "@/components/common/empty-state";
import { DatePicker } from "@/components/common/date-picker";
import { routePaths } from "@/app/route-paths";
import { ErrorState } from "@/components/common/error-state";
import { FilterSelect } from "@/components/common/filter-select";
import { LoadingState } from "@/components/common/loading-state";
import { PageHeader } from "@/components/common/page-header";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
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
import { useSession } from "@/features/auth/hooks/use-session";
import { MediaLinksSection } from "@/features/media";
import { WorkloadTable } from "@/features/training/workloads";
import { medicalApi, type AvailabilityStatus } from "@/features/medical/api";
import { settingsApi } from "@/features/settings/api";
import { isApiError } from "@/lib/api/api-client";
import { formatDate } from "@/lib/date-format";
import { playersApi } from "./api";
import {
  assignmentTimingLabels,
  playerStatusLabels,
  type Player,
  type PlayerAssignment,
  type PlayerStatus,
} from "./types";

const schema = z.object({
  firstName: z.string().trim().min(1, "Unesite ime.").max(100),
  lastName: z.string().trim().min(1, "Unesite prezime.").max(100),
  preferredName: z.string().trim().max(100).optional(),
  dateOfBirth: z.string().optional(),
});
type Values = z.infer<typeof schema>;
const keys = {
  list: (x: unknown) => ["players", "list", x] as const,
  detail: (id: string) => ["players", "detail", id] as const,
  assignments: (id: string) => ["players", "assignments", id] as const,
};
function message(e: unknown) {
  if (isApiError(e)) {
    if (e.status === 403) return "Nemate ovlaštenje za ovu radnju.";
    if (e.status === 404) return "Igrač nije dostupan.";
    if (e.code === "player_has_current_assignments")
      return "Trenutne pripadnosti selekcijama morate prvo završiti.";
    if (e.code === "player_assignment_overlap")
      return "Pripadnost se preklapa s postojećom pripadnošću ove selekcije.";
    return e.detail ?? "Radnja nije moguća.";
  }
  return "Radnja nije uspjela. Pokušajte ponovo.";
}
function Status({ status }: { status: PlayerStatus }) {
  return (
    <Badge variant={status === "ARCHIVED" ? "destructive" : "secondary"}>
      {playerStatusLabels[status]}
    </Badge>
  );
}
function availabilityLabel(status: AvailabilityStatus | undefined) {
  return (
    {
      AVAILABLE: "Dostupan",
      LIMITED: "Ograničeno dostupan",
      UNAVAILABLE: "Nedostupan",
      REHAB: "Rehabilitacija",
      UNKNOWN: "Nepoznato",
    } as const
  )[status ?? "UNKNOWN"];
}
function Memberships({ items }: { items: { teamName: string }[] }) {
  return items.length ? (
    <div className="flex flex-wrap gap-1">
      {items.map((x, i) => (
        <Badge key={`${x.teamName}-${i}`} variant="secondary">
          {x.teamName}
        </Badge>
      ))}
    </div>
  ) : (
    <span className="text-muted-foreground">Nema trenutne selekcije</span>
  );
}
function refresh(qc: ReturnType<typeof useQueryClient>, id?: string) {
  void qc.invalidateQueries({ queryKey: ["players", "list"] });
  if (id) {
    void qc.invalidateQueries({ queryKey: keys.detail(id) });
    void qc.invalidateQueries({ queryKey: keys.assignments(id) });
  }
}

export function PlayersPage() {
  const { user } = useSession();
  const admin = user?.primaryRole === "ADMIN";
  const nav = useNavigate();
  const qc = useQueryClient();
  const [filters, setFilters] = useState({
    search: "",
    status: undefined as PlayerStatus | undefined,
    teamId: undefined as string | undefined,
    page: 1,
    pageSize: 25,
  });
  const [create, setCreate] = useState(false);
  const [edit, setEdit] = useState<Player | null>(null);
  const [life, setLife] = useState<{ p: Player; a: Action } | null>(null);
  const data = useQuery({
    queryKey: keys.list(filters),
    queryFn: () => playersApi.list(filters),
    retry: false,
  });
  const teams = useQuery({
    queryKey: ["settings", "teams", false],
    queryFn: () => settingsApi.listTeams(false),
    retry: false,
  });
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Igrači"
        description="Pregled klupskih igrača i njihovog razvoja kroz selekcije."
        actions={
          admin ? (
            <Button onClick={() => setCreate(true)}>
              <Plus data-icon="inline-start" />
              Novi igrač
            </Button>
          ) : null
        }
      />
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 md:flex md:flex-wrap">
        <Input
          className="w-full md:w-64"
          aria-label="Pretraži igrače"
          value={filters.search}
          onChange={(e) =>
            setFilters((current) => ({
              ...current,
              search: e.target.value,
              page: 1,
            }))
          }
          placeholder="Pretraži ime igrača"
        />
        <FilterSelect
          label="Status"
          emptyLabel="Sve"
          value={filters.status}
          options={playerStatusLabels}
          className="w-full md:w-40"
          onChange={(v) =>
            setFilters((current) => ({
              ...current,
              status: (v as PlayerStatus | null) ?? undefined,
              page: 1,
            }))
          }
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={filters.teamId}
          options={Object.fromEntries(
            (teams.data ?? []).map((team) => [team.id, team.name]),
          )}
          className="w-full md:w-48"
          onChange={(value) =>
            setFilters((current) => ({
              ...current,
              teamId: value ?? undefined,
              page: 1,
            }))
          }
        />
      </section>
      {data.isLoading ? (
        <LoadingState />
      ) : data.isError ? (
        <ErrorState
          description="Igrače nije moguće učitati."
          action={
            <Button onClick={() => void data.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : data.data?.items.length === 0 ? (
        <EmptyState
          title={
            filters.search || filters.status || filters.teamId
              ? "Nema rezultata za odabrane filtere"
              : "Nema igrača"
          }
          description={
            filters.search || filters.status || filters.teamId
              ? "Promijenite ili poništite filtere."
              : "Dodajte prvog igrača u klupski registar."
          }
          action={
            filters.search || filters.status || filters.teamId ? (
              <Button
                variant="outline"
                onClick={() => {
                  setFilters({
                    search: "",
                    status: undefined,
                    teamId: undefined,
                    page: 1,
                    pageSize: 25,
                  });
                }}
              >
                Poništi filtere
              </Button>
            ) : null
          }
        />
      ) : (
        <>
          <div className="border-border overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader className="bg-muted">
                <TableRow>
                  <TableHead>Igrač</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Trenutne selekcije</TableHead>
                  <TableHead>Datum rođenja</TableHead>
                  {admin && (
                    <TableHead>
                      <span className="sr-only">Radnje</span>
                    </TableHead>
                  )}
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.data!.items.map((p) => (
                  <TableRow
                    key={p.id}
                    className="cursor-pointer"
                    onClick={() => nav(`/players/${p.id}`)}
                  >
                    <TableCell className="font-medium">
                      {p.displayName}
                    </TableCell>
                    <TableCell>
                      <Status status={p.status} />
                    </TableCell>
                    <TableCell>
                      <Memberships items={p.currentAssignments} />
                    </TableCell>
                    <TableCell>
                      {p.dateOfBirth
                        ? formatDate(p.dateOfBirth)
                        : "Nije uneseno"}
                    </TableCell>
                    {admin && (
                      <TableCell onClick={(e) => e.stopPropagation()}>
                        <Menu
                          player={p}
                          edit={() => setEdit(p)}
                          lifecycle={(a) => setLife({ p, a })}
                        />
                      </TableCell>
                    )}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between">
            <p className="text-muted-foreground text-sm">
              Ukupno: {data.data!.totalCount}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                disabled={filters.page <= 1}
                onClick={() =>
                  setFilters((current) => ({
                    ...current,
                    page: current.page - 1,
                  }))
                }
              >
                Prethodna
              </Button>
              <Button
                variant="outline"
                disabled={filters.page >= data.data!.totalPages}
                onClick={() =>
                  setFilters((current) => ({
                    ...current,
                    page: current.page + 1,
                  }))
                }
              >
                Sljedeća
              </Button>
            </div>
          </div>
        </>
      )}
      {create && (
        <ProfileDialog
          title="Novi igrač"
          close={() => setCreate(false)}
          saved={() => {
            setCreate(false);
            refresh(qc);
          }}
        />
      )}
      {edit && (
        <ProfileDialog
          player={edit}
          title="Uredi igrača"
          close={() => setEdit(null)}
          saved={() => {
            setEdit(null);
            refresh(qc, edit.id);
          }}
        />
      )}
      {life && (
        <Lifecycle
          value={life}
          close={() => setLife(null)}
          saved={() => {
            refresh(qc, life.p.id);
            setLife(null);
          }}
        />
      )}
    </div>
  );
}
type Action = "activate" | "deactivate" | "archive" | "restore";
function Menu({
  player,
  edit,
  lifecycle,
}: {
  player: Player;
  edit: () => void;
  lifecycle: (a: Action) => void;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button
            variant="ghost"
            size="icon-sm"
            aria-label={`Radnje za ${player.displayName}`}
          />
        }
      >
        <MoreHorizontal />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuGroup>
          {player.status !== "ARCHIVED" && (
            <DropdownMenuItem onClick={edit}>Uredi</DropdownMenuItem>
          )}
          {player.status === "ACTIVE" && (
            <DropdownMenuItem onClick={() => lifecycle("deactivate")}>
              Deaktiviraj
            </DropdownMenuItem>
          )}
          {player.status === "INACTIVE" && (
            <DropdownMenuItem onClick={() => lifecycle("activate")}>
              Aktiviraj
            </DropdownMenuItem>
          )}
          {player.status === "ARCHIVED" ? (
            <DropdownMenuItem onClick={() => lifecycle("restore")}>
              Vrati iz arhive
            </DropdownMenuItem>
          ) : (
            <DropdownMenuItem
              variant="destructive"
              onClick={() => lifecycle("archive")}
            >
              Arhiviraj
            </DropdownMenuItem>
          )}
        </DropdownMenuGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
function ProfileDialog({
  player,
  title,
  close,
  saved,
}: {
  player?: Player;
  title: string;
  close: () => void;
  saved: () => void;
}) {
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      firstName: player?.firstName ?? "",
      lastName: player?.lastName ?? "",
      preferredName: player?.preferredName ?? "",
      dateOfBirth: player?.dateOfBirth ?? "",
    },
  });
  const mutation = useMutation({
    mutationFn: (v: Values) => {
      const body = {
        ...v,
        preferredName: v.preferredName || null,
        dateOfBirth: v.dateOfBirth || null,
      };
      return player
        ? playersApi.update(player.id, body)
        : playersApi.create(body);
    },
    retry: false,
    onSuccess: saved,
  });
  return (
    <Dialog open onOpenChange={(o) => !o && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            Pripadnosti selekcijama vode se zasebno.
          </DialogDescription>
        </DialogHeader>
        <form
          id="player-form"
          onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        >
          <FieldGroup>
            <Text form={form} name="firstName" label="Ime" />
            <Text form={form} name="lastName" label="Prezime" />
            <Text form={form} name="preferredName" label="Preferirano ime" />
            <Text
              form={form}
              name="dateOfBirth"
              label="Datum rođenja"
              type="date"
            />
            {mutation.error && (
              <FieldError>{message(mutation.error)}</FieldError>
            )}
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button
            form="player-form"
            type="submit"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Spremanje..." : "Sačuvaj"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
function Text({
  form,
  name,
  label,
  type = "text",
}: {
  form: ReturnType<typeof useForm<Values>>;
  name: keyof Values;
  label: string;
  type?: string;
}) {
  const e = form.formState.errors[name]?.message;
  return (
    <Field data-invalid={Boolean(e)}>
      <FieldLabel htmlFor={name}>{label}</FieldLabel>
      {type === "date" ? (
        <DatePicker
          id={name}
          value={form.watch(name) as string}
          onChange={(value) => form.setValue(name, value)}
          aria-invalid={Boolean(e)}
        />
      ) : (
        <Input
          id={name}
          type={type}
          aria-invalid={Boolean(e)}
          {...form.register(name)}
        />
      )}
      {e && <FieldError>{e}</FieldError>}
    </Field>
  );
}
function Lifecycle({
  value,
  close,
  saved,
}: {
  value: { p: Player; a: Action };
  close: () => void;
  saved: () => void;
}) {
  const mutation = useMutation({
    mutationFn: () => playersApi.lifecycle(value.p.id, value.a),
    retry: false,
    onSuccess: saved,
  });
  const names: Record<Action, string> = {
    activate: "Aktiviraj",
    deactivate: "Deaktiviraj",
    archive: "Arhiviraj",
    restore: "Vrati iz arhive",
  };
  return (
    <Dialog open onOpenChange={(o) => !o && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{names[value.a]} igrača</DialogTitle>
          <DialogDescription>
            Potvrdite promjenu statusa za igrača {value.p.displayName}.
          </DialogDescription>
        </DialogHeader>
        {mutation.error && <FieldError>{message(mutation.error)}</FieldError>}
        <DialogFooter>
          <Button
            variant={value.a === "archive" ? "destructive" : "default"}
            disabled={mutation.isPending}
            onClick={() => mutation.mutate()}
          >
            {mutation.isPending ? "Obrada..." : "Potvrdi"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function PlayerDetailPage() {
  const { playerId = "" } = useParams();
  const { user } = useSession();
  const admin = user?.primaryRole === "ADMIN";
  const canViewMedicalDetails =
    admin || user?.permissions.canViewMedicalDetails === true;
  const canRecordInjury = admin || user?.primaryRole === "MEDICAL_STAFF";
  const nav = useNavigate();
  const qc = useQueryClient();
  const [add, setAdd] = useState(false);
  const [end, setEnd] = useState<PlayerAssignment | null>(null);
  const [edit, setEdit] = useState<Player | null>(null);
  const [life, setLife] = useState<{ p: Player; a: Action } | null>(null);
  const player = useQuery({
    queryKey: keys.detail(playerId),
    queryFn: () => playersApi.get(playerId),
    retry: false,
  });
  const history = useQuery({
    queryKey: keys.assignments(playerId),
    queryFn: () => playersApi.assignments(playerId),
    retry: false,
  });
  const physical = useQuery({
    queryKey: ["players", "physical-workloads", playerId],
    queryFn: () => playersApi.physicalWorkloads(playerId),
    retry: false,
  });
  const availability = useQueries({
    queries: (player.data?.currentAssignments ?? []).map((assignment) => ({
      queryKey: ["availability", "player-history", playerId, assignment.teamId],
      queryFn: () => medicalApi.playerAvailability(playerId, assignment.teamId),
      retry: false,
    })),
  });
  if (player.isLoading) return <LoadingState />;
  if (player.isError || !player.data)
    return (
      <ErrorState
        title="Igrač nije dostupan"
        description="Igrač ne postoji ili nemate pristup njegovim podacima."
        action={
          <Button onClick={() => nav("/players")}>Nazad na igrače</Button>
        }
      />
    );
  const p = player.data;
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={p.displayName}
        description="Profil igrača i historija pripadnosti selekcijama."
        actions={
          admin ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setAdd(true)}>
                Dodaj pripadnost
              </Button>
              <Menu
                player={p}
                edit={() => setEdit(p)}
                lifecycle={(a) => setLife({ p, a })}
              />
            </div>
          ) : null
        }
      />
      <section className="border-border bg-card flex flex-col gap-4 rounded-xl border p-5">
        <div className="flex flex-wrap items-center gap-2">
          <Status status={p.status} />
          <Memberships items={p.currentAssignments} />
        </div>
        <dl className="grid gap-4 sm:grid-cols-2">
          <Info label="Puno ime" value={`${p.firstName} ${p.lastName}`} />
          <Info
            label="Preferirano ime"
            value={p.preferredName ?? "Nije uneseno"}
          />
          <Info
            label="Datum rođenja"
            value={p.dateOfBirth ? formatDate(p.dateOfBirth) : "Nije uneseno"}
          />
          <Info label="Status" value={playerStatusLabels[p.status]} />
        </dl>
      </section>
      <section className="flex flex-col gap-3">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 className="font-heading text-xl">Dostupnost</h2>
          {canViewMedicalDetails && p.currentAssignments.length ? (
            <div className="flex flex-wrap gap-2">
              {p.currentAssignments.map((assignment) => (
                <Button
                  key={assignment.teamId}
                  size="sm"
                  variant="outline"
                  onClick={() =>
                    nav(
                      `${routePaths.medical}?teamId=${assignment.teamId}&medicalView=injuries&injuryPlayerId=${playerId}`,
                    )
                  }
                >
                  Povrede: {assignment.teamName}
                </Button>
              ))}
              {canRecordInjury
                ? p.currentAssignments.map((assignment) => (
                    <Button
                      key={`create-injury-${assignment.teamId}`}
                      size="sm"
                      onClick={() =>
                        nav(
                          `${routePaths.medical}?teamId=${assignment.teamId}&medicalView=injuries&injuryPlayerId=${playerId}&injuryCreatePlayerId=${playerId}`,
                        )
                      }
                    >
                      Evidentiraj: {assignment.teamName}
                    </Button>
                  ))
                : null}
            </div>
          ) : null}
        </div>
        <div className="border-border overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader className="bg-muted">
              <TableRow>
                <TableHead>Selekcija</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Datum važenja</TableHead>
                <TableHead>Očekivani povratak</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {p.currentAssignments.map((assignment, index) => {
                const record = availability[index]?.data?.items[0];
                return (
                  <TableRow key={assignment.teamId}>
                    <TableCell>{assignment.teamName}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">
                        {availabilityLabel(record?.status)}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {record?.effectiveOn
                        ? formatDate(record.effectiveOn)
                        : "Nije uneseno"}
                    </TableCell>
                    <TableCell>
                      {record &&
                      ["LIMITED", "UNAVAILABLE", "REHAB"].includes(
                        record.status,
                      )
                        ? record.expectedReturnOn
                          ? formatDate(record.expectedReturnOn)
                          : "Nije uneseno"
                        : "—"}
                    </TableCell>
                  </TableRow>
                );
              })}
              {!p.currentAssignments.length ? (
                <TableRow>
                  <TableCell colSpan={4} className="text-muted-foreground">
                    Nema trenutne selekcije.
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </div>
      </section>
      <section className="flex flex-col gap-3">
        <h2 className="font-heading text-xl">
          Historija pripadnosti selekcijama
        </h2>
        {history.isLoading ? (
          <LoadingState label="Učitavanje pripadnosti..." />
        ) : history.isError ? (
          <ErrorState
            description="Historiju pripadnosti nije moguće učitati."
            action={
              <Button onClick={() => void history.refetch()}>
                Pokušaj ponovo
              </Button>
            }
          />
        ) : history.data?.length === 0 ? (
          <EmptyState
            title="Nema evidentiranih pripadnosti"
            description="Pripadnosti selekcijama bit će prikazane ovdje."
          />
        ) : (
          <div className="border-border overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader className="bg-muted">
                <TableRow>
                  <TableHead>Selekcija</TableHead>
                  <TableHead>Početak</TableHead>
                  <TableHead>Završetak</TableHead>
                  <TableHead>Stanje</TableHead>
                  {admin && <TableHead />}
                </TableRow>
              </TableHeader>
              <TableBody>
                {history.data?.map((a) => (
                  <TableRow key={a.id}>
                    <TableCell className="font-medium">{a.teamName}</TableCell>
                    <TableCell>{formatDate(a.startDate)}</TableCell>
                    <TableCell>
                      {a.endDate ? formatDate(a.endDate) : "U toku"}
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">
                        {assignmentTimingLabels[a.timingState]}
                      </Badge>
                    </TableCell>
                    {admin && (
                      <TableCell>
                        {!a.endDate && (
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => setEnd(a)}
                          >
                            Završi
                          </Button>
                        )}
                      </TableCell>
                    )}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </section>
      <section className="flex flex-col gap-3">
        <h2 className="font-heading text-xl">Fizički podaci</h2>
        {physical.isLoading ? (
          <LoadingState label="Učitavanje fizičkih podataka..." />
        ) : physical.isError ? (
          <ErrorState
            description="Fizičke podatke nije moguće učitati."
            action={
              <Button onClick={() => void physical.refetch()}>
                Pokušaj ponovo
              </Button>
            }
          />
        ) : (
          <WorkloadTable workloads={physical.data ?? []} />
        )}
      </section>
      <MediaLinksSection targetId={playerId} targetType="PLAYER" />
      {add && (
        <AssignmentDialog
          playerId={playerId}
          close={() => setAdd(false)}
          saved={() => {
            setAdd(false);
            refresh(qc, playerId);
          }}
        />
      )}
      {end && (
        <EndDialog
          assignment={end}
          close={() => setEnd(null)}
          saved={() => {
            setEnd(null);
            refresh(qc, playerId);
          }}
        />
      )}
      {edit && (
        <ProfileDialog
          player={edit}
          title="Uredi igrača"
          close={() => setEdit(null)}
          saved={() => {
            setEdit(null);
            refresh(qc, playerId);
          }}
        />
      )}
      {life && (
        <Lifecycle
          value={life}
          close={() => setLife(null)}
          saved={() => {
            setLife(null);
            refresh(qc, playerId);
          }}
        />
      )}
    </div>
  );
}
function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1">
      <dt className="text-muted-foreground text-sm">{label}</dt>
      <dd>{value}</dd>
    </div>
  );
}
const assignmentSchema = z
  .object({
    teamId: z.string().min(1, "Odaberite selekciju."),
    startDate: z.string().min(1, "Unesite datum početka."),
    endDate: z.string().optional(),
  })
  .refine((x) => !x.endDate || x.endDate >= x.startDate, {
    path: ["endDate"],
    message: "Datum završetka ne može biti prije datuma početka.",
  });
type AssignmentValues = z.infer<typeof assignmentSchema>;
function AssignmentDialog({
  playerId,
  close,
  saved,
}: {
  playerId: string;
  close: () => void;
  saved: () => void;
}) {
  const form = useForm<AssignmentValues>({
    resolver: zodResolver(assignmentSchema),
    defaultValues: { teamId: "", startDate: "", endDate: "" },
  });
  const teams = useQuery({
    queryKey: ["settings", "teams", false],
    queryFn: () => settingsApi.listTeams(false),
    retry: false,
  });
  const mutation = useMutation({
    mutationFn: (v: AssignmentValues) =>
      playersApi.createAssignment(playerId, {
        ...v,
        endDate: v.endDate || null,
      }),
    retry: false,
    onSuccess: saved,
  });
  const teamError = form.formState.errors.teamId?.message;
  const selectedTeamId = useWatch({ control: form.control, name: "teamId" });
  const selectedTeam = teams.data?.find((team) => team.id === selectedTeamId);
  return (
    <Dialog open onOpenChange={(o) => !o && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Dodaj pripadnost selekciji</DialogTitle>
          <DialogDescription>
            Pripadnost drugoj selekciji neće biti automatski završena.
          </DialogDescription>
        </DialogHeader>
        <form
          id="assignment-form"
          onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        >
          <FieldGroup>
            <Field data-invalid={Boolean(teamError)}>
              <FieldLabel>Selekcija</FieldLabel>
              <Select
                value={selectedTeamId ?? ""}
                onValueChange={(v) =>
                  form.setValue("teamId", v ?? "", { shouldValidate: true })
                }
              >
                <SelectTrigger>
                  <SelectValue>
                    {selectedTeam?.name ?? "Odaberite selekciju"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {teams.data
                      ?.filter((t) => t.status === "ACTIVE")
                      .map((t) => (
                        <SelectItem key={t.id} value={t.id}>
                          {t.name}
                        </SelectItem>
                      ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
              {teamError && <FieldError>{teamError}</FieldError>}
            </Field>
            <AssignmentInput
              form={form}
              name="startDate"
              label="Datum početka"
            />
            <AssignmentInput
              form={form}
              name="endDate"
              label="Datum završetka (opcionalno)"
            />
            {mutation.error && (
              <FieldError>{message(mutation.error)}</FieldError>
            )}
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button
            form="assignment-form"
            type="submit"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Spremanje..." : "Dodaj pripadnost"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
function AssignmentInput({
  form,
  name,
  label,
}: {
  form: ReturnType<typeof useForm<AssignmentValues>>;
  name: keyof AssignmentValues;
  label: string;
}) {
  const error = form.formState.errors[name]?.message;
  return (
    <Field data-invalid={Boolean(error)}>
      <FieldLabel htmlFor={name}>{label}</FieldLabel>
      <DatePicker
        id={name}
        value={form.watch(name) as string}
        onChange={(value) =>
          form.setValue(name, value, { shouldValidate: true })
        }
        aria-invalid={Boolean(error)}
      />
      {error && <FieldError>{error}</FieldError>}
    </Field>
  );
}
function EndDialog({
  assignment,
  close,
  saved,
}: {
  assignment: PlayerAssignment;
  close: () => void;
  saved: () => void;
}) {
  const [date, setDate] = useState("");
  const invalid = !date || date < assignment.startDate;
  const mutation = useMutation({
    mutationFn: () =>
      playersApi.endAssignment(assignment.playerId, assignment.id, date),
    retry: false,
    onSuccess: saved,
  });
  return (
    <Dialog open onOpenChange={(o) => !o && close()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Završi pripadnost</DialogTitle>
          <DialogDescription>
            Odredite datum završetka za selekciju {assignment.teamName}.
          </DialogDescription>
        </DialogHeader>
        <Field data-invalid={invalid}>
          <FieldLabel htmlFor="end-date">Datum završetka</FieldLabel>
          <DatePicker
            id="end-date"
            value={date}
            onChange={setDate}
            aria-invalid={invalid}
          />
          {date && date < assignment.startDate && (
            <FieldError>
              Datum završetka ne može biti prije datuma početka.
            </FieldError>
          )}
          {mutation.error && <FieldError>{message(mutation.error)}</FieldError>}
        </Field>
        <DialogFooter>
          <Button
            disabled={invalid || mutation.isPending}
            onClick={() => mutation.mutate()}
          >
            {mutation.isPending ? "Spremanje..." : "Završi pripadnost"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
