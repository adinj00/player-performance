import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Copy, MoreHorizontal, Plus } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Link, useNavigate } from "react-router-dom";
import { parseAsInteger, parseAsString, useQueryStates } from "nuqs";
import { routePaths } from "@/app/route-paths";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { FilterSelect } from "@/components/common/filter-select";
import { LoadingState } from "@/components/common/loading-state";
import { PageHeader } from "@/components/common/page-header";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
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
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Label } from "@/components/ui/label";
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
import { useSession, sessionQueryKey } from "@/features/auth/hooks/use-session";
import * as api from "@/features/users/api";
import {
  permissionLabels,
  roleLabels,
  scopeLabels,
  statusLabels,
  type CreateStaffInvitationRequest,
  type StaffAccessRequest,
  type StaffRole,
  type StaffUserResponse,
  type TeamScopeType,
} from "@/features/users/types";
import { isApiError } from "@/lib/api/api-client";
import {
  AuditError,
  AuditFilterBar,
  AuditHistory,
  AuditLoading,
  AuditPagination,
  getStaffUserAudit,
} from "@/features/audit";

const staffAuditLabels = {
  STAFF_INVITATION_CREATED: "Poziv je kreiran",
  STAFF_INVITATION_REISSUED: "Poziv je ponovo izdat",
  STAFF_INVITATION_ACCEPTED: "Poziv je prihvaćen",
  STAFF_PROFILE_UPDATED: "Profil je ažuriran",
  STAFF_ACCESS_REPLACED: "Pristup je zamijenjen",
  STAFF_USER_DISABLED: "Korisnik je onemogućen",
  STAFF_USER_REACTIVATED: "Korisnik je reaktiviran",
};
const staffAuditFields = {
  displayName: "Ime",
  status: "Status",
  primaryRole: "Uloga",
  canVerifyReports: "Može verifikovati izvještaje",
  canImportData: "Može importovati podatke",
  canViewMedicalDetails: "Može pregledati medicinske detalje",
  teamScopeType: "Obim timova",
  selectedTeamIds: "Odabrane selekcije",
};

function StaffAuditSheet({
  staff,
  onClose,
  onForbidden,
  teams,
}: {
  staff: StaffUserResponse;
  onClose: () => void;
  onForbidden: () => void;
  teams: Awaited<ReturnType<typeof api.listTeams>>;
}) {
  const [filters, setFilters] = useQueryStates({
    auditAction: parseAsString,
    auditFrom: parseAsString,
    auditTo: parseAsString,
    auditPage: parseAsInteger.withDefault(1),
  });
  const audit = useQuery({
    queryKey: ["staff-user-audit", staff.id, filters],
    queryFn: () =>
      getStaffUserAudit(staff.id, {
        action: filters.auditAction,
        dateFrom: filters.auditFrom,
        dateTo: filters.auditTo,
        page: filters.auditPage,
      }),
    retry: false,
  });
  useEffect(() => {
    if (audit.isError && isApiError(audit.error) && audit.error.status === 403)
      onForbidden();
  }, [audit.error, audit.isError, onForbidden]);
  return (
    <Sheet open onOpenChange={(open) => !open && onClose()}>
      <SheetContent className="w-full overflow-x-hidden data-[side=right]:sm:max-w-xl">
        <SheetHeader>
          <SheetTitle>Historija promjena korisnika</SheetTitle>
          <SheetDescription>
            Trenutno stanje: {staff.displayName} · {statusLabels[staff.status]}
          </SheetDescription>
        </SheetHeader>
        <div className="flex min-w-0 flex-col gap-4 overflow-y-auto px-4 pb-4">
          <AuditFilterBar
            actions={staffAuditLabels}
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
            compact
          />
          {audit.isLoading ? (
            <AuditLoading />
          ) : audit.isError ? (
            <AuditError retry={() => void audit.refetch()} />
          ) : (
            <>
              <AuditHistory
                data={audit.data}
                labels={staffAuditLabels}
                fields={staffAuditFields}
                expectedEntityType="STAFF_USER"
                formatValue={(key, value) =>
                  key === "selectedTeamIds" && Array.isArray(value)
                    ? value
                        .map(
                          (id) =>
                            teams.find((team) => team.id === id)?.name ??
                            `Nepoznata selekcija (${String(id)})`,
                        )
                        .join(", ") || "Nije postavljeno"
                    : undefined
                }
              />
              <AuditPagination
                data={audit.data!}
                onPage={(page) => void setFilters({ auditPage: page })}
              />
            </>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}

const roles = Object.keys(roleLabels) as StaffRole[];
const schema = z
  .object({
    displayName: z.string().trim().min(1, "Unesite ime."),
    email: z.string().trim().email("Unesite ispravnu e-mail adresu."),
    primaryRole: z.enum([
      "ADMIN",
      "DATA_OPERATOR",
      "ANALYST",
      "COACH",
      "MEDICAL_STAFF",
      "VIEWER",
    ]),
    canVerifyReports: z.boolean(),
    canImportData: z.boolean(),
    canViewMedicalDetails: z.boolean(),
    teamScopeType: z.enum(["ALL_TEAMS", "SELECTED_TEAMS"]),
    selectedTeamIds: z.array(z.string()),
  })
  .superRefine((v, c) => {
    if (
      v.primaryRole !== "ADMIN" &&
      v.teamScopeType === "SELECTED_TEAMS" &&
      !v.selectedTeamIds.length
    )
      c.addIssue({
        code: "custom",
        path: ["selectedTeamIds"],
        message: "Odaberite najmanje jednu selekciju.",
      });
  });
type Values = z.infer<typeof schema>;
function message(error: unknown) {
  if (isApiError(error)) {
    if (error.status === 409)
      return "Promjena nije moguća zbog trenutnog stanja računa ili posljednjeg aktivnog administratora.";
    if (error.status === 403) return "Nemate ovlaštenje za ovu radnju.";
    if (error.status === 404) return "Korisnik više nije dostupan.";
  }
  return "Radnja nije uspjela. Pokušajte ponovo.";
}

function Modal({
  title,
  description,
  children,
  onClose,
}: {
  title: string;
  description: string;
  children: React.ReactNode;
  onClose: () => void;
}) {
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        {children}
      </DialogContent>
    </Dialog>
  );
}

function AccessFields({
  form,
  teams,
}: {
  form: ReturnType<typeof useForm<Values>>;
  teams: Awaited<ReturnType<typeof api.listTeams>>;
}) {
  const role = form.watch("primaryRole");
  const scope = form.watch("teamScopeType");
  const admin = role === "ADMIN";
  const selected = form.watch("selectedTeamIds");
  function selectRole(value: StaffRole) {
    form.setValue("primaryRole", value);
    if (value === "ADMIN") {
      form.setValue("teamScopeType", "ALL_TEAMS");
      form.setValue("selectedTeamIds", []);
      form.setValue("canVerifyReports", true);
      form.setValue("canImportData", true);
      form.setValue("canViewMedicalDetails", true);
    }
  }
  return (
    <FieldSet className="mt-7">
      <FieldGroup className="gap-6">
        <Field>
          <FieldLabel>Uloga</FieldLabel>
          <Select
            value={role}
            onValueChange={(value) => selectRole(value as StaffRole)}
          >
            <SelectTrigger className="w-full">
              <SelectValue>{roleLabels[role]}</SelectValue>
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                {roles.map((r) => (
                  <SelectItem key={r} value={r}>
                    {roleLabels[r]}
                  </SelectItem>
                ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </Field>
        <FieldDescription>
          {admin
            ? "Administrator ima sve dozvole i pristup svim timovima."
            : "Odaberite eksplicitne dozvole i pristup timovima."}
        </FieldDescription>
        {(Object.keys(permissionLabels) as (keyof Values)[])
          .filter((k) => k.startsWith("can"))
          .map((key) => (
            <Label key={key} className="gap-2">
              <Checkbox
                disabled={admin}
                checked={Boolean(form.watch(key))}
                onCheckedChange={(checked) =>
                  form.setValue(key, checked === true)
                }
              />
              {permissionLabels[key as keyof typeof permissionLabels]}
            </Label>
          ))}
        <Field>
          <FieldLabel>Obim timova</FieldLabel>
          <Select
            disabled={admin}
            value={scope}
            onValueChange={(value) => {
              form.setValue("teamScopeType", value as TeamScopeType);
              if (value === "ALL_TEAMS") form.setValue("selectedTeamIds", []);
            }}
          >
            <SelectTrigger className="w-full">
              <SelectValue>{scopeLabels[scope]}</SelectValue>
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectItem value="ALL_TEAMS">Svi timovi</SelectItem>
                <SelectItem value="SELECTED_TEAMS">
                  Odabrane selekcije
                </SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </Field>
        {!admin && scope === "SELECTED_TEAMS" ? (
          <div className="flex flex-col gap-2">
            <span className="text-sm font-medium">Selekcije</span>
            {teams
              .filter((t) => t.status !== "ARCHIVED")
              .map((t) => (
                <Label key={t.id} className="gap-2">
                  <Checkbox
                    checked={selected.includes(t.id)}
                    onCheckedChange={(checked) =>
                      form.setValue(
                        "selectedTeamIds",
                        checked === true
                          ? [...selected, t.id]
                          : selected.filter((id) => id !== t.id),
                      )
                    }
                  />
                  {t.name}
                  {t.status === "INACTIVE" ? " (neaktivan)" : ""}
                </Label>
              ))}
            {form.formState.errors.selectedTeamIds ? (
              <p className="text-destructive text-sm">
                {form.formState.errors.selectedTeamIds.message}
              </p>
            ) : null}
          </div>
        ) : null}
      </FieldGroup>
    </FieldSet>
  );
}
export function UsersPage() {
  const { user, isLoading: sessionLoading, refetchSession } = useSession();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [filters, setFilters] = useState({
    q: "",
    role: null as string | null,
    status: null as string | null,
    scope: null as string | null,
    team: null as string | null,
  });
  const [auditState, setAuditState] = useQueryStates({
    auditUser: parseAsString,
    auditAction: parseAsString,
    auditFrom: parseAsString,
    auditTo: parseAsString,
    auditPage: parseAsInteger.withDefault(1),
  });
  const updateFilters = (updates: Partial<typeof filters>) => {
    setFilters((current) => ({ ...current, ...updates }));
  };

  const staff = useQuery({
    queryKey: ["users", "list", filters],
    queryFn: () =>
      api.listStaff({
        q: filters.q || undefined,
        role: (filters.role as StaffRole | null) || undefined,
        status:
          (filters.status as
            import("@/features/users/types").StaffAccountStatus | null) ||
          undefined,
        scope: (filters.scope as TeamScopeType | null) || undefined,
        team: filters.team || undefined,
      }),
    enabled: user?.primaryRole === "ADMIN",
    retry: false,
  });
  const teams = useQuery({
    queryKey: ["users", "teams"],
    queryFn: api.listTeams,
    enabled: user?.primaryRole === "ADMIN",
    retry: false,
  });
  const [invite, setInvite] = useState(false);
  const [credential, setCredential] = useState<{
    email: string;
    token: string;
  } | null>(null);
  const [isSetupLinkCopied, setIsSetupLinkCopied] = useState(false);
  const [editing, setEditing] = useState<StaffUserResponse | null>(null);
  const [action, setAction] = useState<{
    kind: "disable" | "reactivate" | "reissue";
    staff: StaffUserResponse;
  } | null>(null);
  const auditStaff =
    staff.data?.find((candidate) => candidate.id === auditState.auditUser) ??
    null;

  useEffect(() => {
    if (!isSetupLinkCopied) {
      return;
    }

    const timeoutId = window.setTimeout(
      () => setIsSetupLinkCopied(false),
      2000,
    );
    return () => window.clearTimeout(timeoutId);
  }, [isSetupLinkCopied]);
  const invalidate = async () => {
    await Promise.all([
      qc.invalidateQueries({ queryKey: ["users"] }),
      qc.invalidateQueries({ queryKey: ["staff-user-audit"] }),
    ]);
  };
  const create = useMutation({
    mutationFn: api.createInvitation,
    retry: false,
    onSuccess: (r) => {
      setInvite(false);
      setCredential({ email: r.user.email, token: r.setupToken });
      void invalidate();
    },
  });
  const lifecycle = useMutation({
    mutationFn: async (a: { kind: string; staff: StaffUserResponse }) =>
      a.kind === "disable"
        ? api.disableStaff(a.staff.id)
        : a.kind === "reactivate"
          ? api.reactivateStaff(a.staff.id)
          : api.reissueInvitation(a.staff.id),
    retry: false,
    onSuccess: async (r, a) => {
      setAction(null);
      await invalidate();
      if (a.kind === "reissue" && "setupToken" in r)
        setCredential({ email: r.user.email, token: r.setupToken });
      if (a.kind === "disable" && a.staff.id === user?.id) {
        qc.setQueryData(sessionQueryKey, {
          isAuthenticated: false,
          user: null,
        });
        navigate(routePaths.signIn, { replace: true });
      }
    },
  });
  if (sessionLoading) return <LoadingState label="Provjera pristupa..." />;
  if (user?.primaryRole !== "ADMIN")
    return (
      <ErrorState
        title="Pristup nije dozvoljen"
        description="Ovu stranicu mogu otvoriti samo administratori."
        action={
          <Button
            nativeButton={false}
            render={<Link to={routePaths.dashboard} />}
          >
            Na kontrolnu ploču
          </Button>
        }
      />
    );
  const link = credential
    ? `${window.location.origin}${routePaths.acceptInvitation}?email=${encodeURIComponent(credential.email)}&token=${encodeURIComponent(credential.token)}`
    : "";
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Korisnici i uloge"
        description="Upravljanje pozivima, ulogama, dozvolama i pristupom selekcijama."
        actions={
          <Button onClick={() => setInvite(true)}>
            <Plus data-icon="inline-start" />
            Pozovi korisnika
          </Button>
        }
      />
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 md:flex md:flex-wrap">
        <Input
          aria-label="Pretraži korisnike"
          value={filters.q}
          onChange={(e) => updateFilters({ q: e.target.value })}
          placeholder="Pretraži ime ili e-mail"
          className="w-full md:w-64"
        />
        <FilterSelect
          label="Uloge"
          emptyLabel="Sve uloge"
          value={filters.role}
          options={roleLabels}
          className="w-full md:w-40"
          onChange={(role) => updateFilters({ role })}
        />
        <FilterSelect
          label="Status"
          emptyLabel="Svi statusi"
          value={filters.status}
          options={statusLabels}
          className="w-full md:w-40"
          onChange={(status) => updateFilters({ status })}
        />
        <FilterSelect
          label="Pristup timovima"
          emptyLabel="Svi pristupi"
          value={filters.scope}
          options={scopeLabels}
          className="w-full md:w-48"
          onChange={(scope) => updateFilters({ scope })}
        />
      </section>
      {staff.isLoading ? (
        <LoadingState />
      ) : staff.isError ? (
        <ErrorState
          description="Korisnike nije moguće učitati."
          action={
            <Button onClick={() => void staff.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : staff.data?.length === 0 ? (
        <EmptyState
          action={
            Object.values(filters).some(Boolean) ? (
              <Button
                variant="outline"
                onClick={() => {
                  setFilters({
                    q: "",
                    role: null,
                    status: null,
                    scope: null,
                    team: null,
                  });
                }}
              >
                Poništi filtere
              </Button>
            ) : null
          }
          title={
            Object.values(filters).some(Boolean)
              ? "Nema rezultata za odabrane filtere"
              : "Nema korisnika"
          }
          description="Kreirajte poziv za prvog člana osoblja."
        />
      ) : (
        <div className="border-border overflow-hidden rounded-xl border">
          <Table>
            <TableHeader className="bg-muted">
              <TableRow>
                <TableHead>Korisnik</TableHead>
                <TableHead>Uloga</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Pristup timovima</TableHead>
                <TableHead>Dozvole</TableHead>
                <TableHead>
                  <span className="sr-only">Radnje</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {staff.data?.map((s) => (
                <TableRow key={s.id}>
                  <TableCell>
                    <div className="font-medium">{s.displayName}</div>
                    <div className="text-muted-foreground">{s.email}</div>
                  </TableCell>
                  <TableCell>{roleLabels[s.primaryRole]}</TableCell>
                  <TableCell>{statusLabels[s.status]}</TableCell>
                  <TableCell>{scopeLabels[s.teamScope.type]}</TableCell>
                  <TableCell>
                    {s.primaryRole === "ADMIN"
                      ? "Sve dozvole"
                      : Object.entries(s.permissions)
                          .filter(([, v]) => v)
                          .map(
                            ([k]) =>
                              permissionLabels[
                                k as keyof typeof permissionLabels
                              ],
                          )
                          .join(", ") || "Nema"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger
                        render={
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            aria-label={`Radnje za ${s.displayName}`}
                          />
                        }
                      >
                        <MoreHorizontal />
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuGroup>
                          <DropdownMenuItem onClick={() => setEditing(s)}>
                            Uredi
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() =>
                              void setAuditState({
                                auditUser: s.id,
                                auditAction: null,
                                auditFrom: null,
                                auditTo: null,
                                auditPage: 1,
                              })
                            }
                          >
                            Historija promjena
                          </DropdownMenuItem>
                          {s.status === "INVITED" ? (
                            <DropdownMenuItem
                              onClick={() =>
                                setAction({ kind: "reissue", staff: s })
                              }
                            >
                              Ponovo izdaj poziv
                            </DropdownMenuItem>
                          ) : null}
                          {s.status === "DISABLED" ? (
                            <DropdownMenuItem
                              onClick={() =>
                                setAction({ kind: "reactivate", staff: s })
                              }
                            >
                              Reaktiviraj
                            </DropdownMenuItem>
                          ) : (
                            <DropdownMenuItem
                              variant="destructive"
                              onClick={() =>
                                setAction({ kind: "disable", staff: s })
                              }
                            >
                              Onemogući
                            </DropdownMenuItem>
                          )}
                        </DropdownMenuGroup>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      {invite ? (
        <InviteModal
          teams={teams.data ?? []}
          pending={create.isPending}
          error={create.error ? message(create.error) : null}
          onClose={() => setInvite(false)}
          onSubmit={(v) => create.mutate(v)}
        />
      ) : null}
      {auditStaff ? (
        <StaffAuditSheet
          staff={auditStaff}
          onClose={() =>
            void setAuditState({
              auditUser: null,
              auditAction: null,
              auditFrom: null,
              auditTo: null,
              auditPage: 1,
            })
          }
          onForbidden={() => {
            void setAuditState({
              auditUser: null,
              auditAction: null,
              auditFrom: null,
              auditTo: null,
              auditPage: 1,
            });
            void refetchSession();
          }}
          teams={teams.data ?? []}
        />
      ) : null}
      {editing ? (
        <EditModal
          staff={editing}
          teams={teams.data ?? []}
          onClose={() => setEditing(null)}
          onSaved={async () => {
            setEditing(null);
            await invalidate();
            if (editing.id === user.id) {
              await qc.invalidateQueries({ queryKey: sessionQueryKey });
              navigate(routePaths.users, { replace: true });
            }
          }}
        />
      ) : null}
      {action ? (
        <Modal
          title={
            action.kind === "reissue"
              ? "Ponovo izdajte poziv"
              : "Potvrdite promjenu statusa"
          }
          description={
            action.kind === "reissue"
              ? `Prethodni setup link za ${action.staff.email} prestat će raditi.`
              : `${action.kind === "disable" ? "Onemogućit ćete" : "Reaktivirat ćete"} račun ${action.staff.email}.`
          }
          onClose={() => setAction(null)}
        >
          <div className="mt-5 flex gap-2">
            <Button
              disabled={lifecycle.isPending}
              onClick={() => lifecycle.mutate(action)}
            >
              {lifecycle.isPending ? "Obrada..." : "Potvrdi"}
            </Button>
            {lifecycle.error ? (
              <p className="text-destructive text-sm">
                {message(lifecycle.error)}
              </p>
            ) : null}
          </div>
        </Modal>
      ) : null}
      {credential ? (
        <Modal
          title="Jednokratni setup link"
          description="Link je prikazan jednom. Pošaljite ga samo kroz pouzdan klupski kanal."
          onClose={() => {
            setCredential(null);
            setIsSetupLinkCopied(false);
          }}
        >
          <Input
            readOnly
            value={link}
            aria-label="Setup link"
            className="mt-5"
          />
          <Button
            className="mt-3"
            onClick={async () => {
              try {
                await navigator.clipboard.writeText(link);
                setIsSetupLinkCopied(true);
              } catch {
                // The selectable read-only field remains available as a safe fallback.
              }
            }}
          >
            {isSetupLinkCopied ? (
              <Check data-icon="inline-start" />
            ) : (
              <Copy data-icon="inline-start" />
            )}
            {isSetupLinkCopied ? "Kopirano" : "Kopiraj setup link"}
          </Button>
        </Modal>
      ) : null}
    </div>
  );
}

function InviteModal({
  teams,
  pending,
  error,
  onClose,
  onSubmit,
}: {
  teams: Awaited<ReturnType<typeof api.listTeams>>;
  pending: boolean;
  error: string | null;
  onClose: () => void;
  onSubmit: (v: CreateStaffInvitationRequest) => void;
}) {
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      displayName: "",
      email: "",
      primaryRole: "VIEWER",
      canVerifyReports: false,
      canImportData: false,
      canViewMedicalDetails: false,
      teamScopeType: "ALL_TEAMS",
      selectedTeamIds: [],
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Pozovi korisnika</DialogTitle>
          <DialogDescription>
            Administrator ne postavlja lozinku; korisnik je postavlja putem
            jednokratnog linka.
          </DialogDescription>
        </DialogHeader>
        <div className="no-scrollbar -mx-4 max-h-[50vh] overflow-y-auto px-4">
          <form
            id="invite-staff-form"
            className="py-1"
            onSubmit={form.handleSubmit((v) =>
              onSubmit({
                ...v,
                selectedTeamIds:
                  v.primaryRole === "ADMIN" || v.teamScopeType === "ALL_TEAMS"
                    ? []
                    : v.selectedTeamIds,
              }),
            )}
          >
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor="invite-display-name">Ime</FieldLabel>
                <Input
                  id="invite-display-name"
                  {...form.register("displayName")}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="invite-email">E-mail</FieldLabel>
                <Input
                  id="invite-email"
                  type="email"
                  {...form.register("email")}
                />
              </Field>
              <AccessFields form={form} teams={teams} />
              {error ? (
                <p className="text-destructive mt-3 text-sm">{error}</p>
              ) : null}
            </FieldGroup>
          </form>
        </div>
        <DialogFooter>
          <Button form="invite-staff-form" type="submit" disabled={pending}>
            {pending ? "Slanje..." : "Kreiraj poziv"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function EditModal({
  staff,
  teams,
  onClose,
  onSaved,
}: {
  staff: StaffUserResponse;
  teams: Awaited<ReturnType<typeof api.listTeams>>;
  onClose: () => void;
  onSaved: () => Promise<void>;
}) {
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      displayName: staff.displayName,
      email: staff.email,
      primaryRole: staff.primaryRole,
      ...staff.permissions,
      teamScopeType: staff.teamScope.type,
      selectedTeamIds: staff.teamScope.selectedTeamIds,
    },
  });
  const save = useMutation({
    mutationFn: async (v: Values) => {
      await api.updateProfile(staff.id, v.displayName);
      await api.replaceAccess(staff.id, {
        primaryRole: v.primaryRole,
        canVerifyReports: v.primaryRole === "ADMIN" ? true : v.canVerifyReports,
        canImportData: v.primaryRole === "ADMIN" ? true : v.canImportData,
        canViewMedicalDetails:
          v.primaryRole === "ADMIN" ? true : v.canViewMedicalDetails,
        teamScopeType:
          v.primaryRole === "ADMIN" ? "ALL_TEAMS" : v.teamScopeType,
        selectedTeamIds:
          v.primaryRole === "ADMIN" || v.teamScopeType === "ALL_TEAMS"
            ? []
            : v.selectedTeamIds,
      } as StaffAccessRequest);
    },
    retry: false,
    onSuccess: () => void onSaved(),
  });
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Uredi ime i pristup</DialogTitle>
          <DialogDescription>
            E-mail adresu nije moguće mijenjati. Pristup se zamjenjuje
            kompletnom konfiguracijom.
          </DialogDescription>
        </DialogHeader>
        <div className="no-scrollbar -mx-4 max-h-[50vh] overflow-y-auto px-4">
          <form
            id="edit-staff-form"
            className="py-1"
            onSubmit={form.handleSubmit((v) => save.mutate(v))}
          >
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor="edit-display-name">Ime</FieldLabel>
                <Input
                  id="edit-display-name"
                  {...form.register("displayName")}
                />
              </Field>
              <Field>
                <FieldLabel>E-mail</FieldLabel>
                <FieldDescription>{staff.email}</FieldDescription>
              </Field>
              <AccessFields form={form} teams={teams} />
              {save.error ? (
                <p className="text-destructive mt-3 text-sm">
                  {message(save.error)}
                </p>
              ) : null}
            </FieldGroup>
          </form>
        </div>
        <DialogFooter>
          <Button
            form="edit-staff-form"
            type="submit"
            disabled={save.isPending}
          >
            {save.isPending ? "Spremanje..." : "Sačuvaj promjene"}
          </Button>
          <DialogClose render={<Button variant="outline" />}>
            Odustani
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
