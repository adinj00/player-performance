import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowDown, ArrowUp, Pencil, Plus, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";

import { FormErrorSummary } from "@/components/common/form-error-summary";
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
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
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
import { useSession } from "@/features/auth";
import { matchesApi } from "@/features/matches/api/matches-api";
import type {
  EligibleLineupPlayer,
  MatchLineupPlayer,
  MatchLineupResponse,
  MatchResponse,
  MatchStatus,
  SaveMatchLineupRequest,
} from "@/features/matches/types/match";
import { validateOrderedSubstitutions } from "@/features/matches/utils/lineup";
import { isApiError } from "@/lib/api/api-client";

const rowSchema = z.object({
  playerId: z.string().min(1),
  role: z.enum(["STARTER", "SUBSTITUTE"]),
});
const formSchema = z.object({
  formation: z.string(),
  captainPlayerId: z.string(),
  entries: z.array(rowSchema),
  substitutions: z.array(
    z.object({
      playerOutId: z.string(),
      playerInId: z.string(),
      minute: z.string(),
      stoppageTimeMinute: z.string(),
    }),
  ),
  appearances: z.array(
    z.object({ playerId: z.string(), minutesPlayed: z.string() }),
  ),
});
type LineupForm = z.infer<typeof formSchema>;

function playerName(player: MatchLineupPlayer) {
  return player.preferredName || `${player.firstName} ${player.lastName}`;
}

function isNonNegativeInteger(value: string) {
  return /^\d+$/.test(value);
}

function emptyForm(lineup: MatchLineupResponse): LineupForm {
  return {
    formation: lineup.formation ?? "",
    captainPlayerId: lineup.captain?.id ?? "",
    entries: lineup.entries.map((entry) => ({
      playerId: entry.player.id,
      role: entry.role,
    })),
    substitutions: lineup.substitutions.map((item) => ({
      playerOutId: item.playerOutId,
      playerInId: item.playerInId,
      minute: String(item.minute),
      stoppageTimeMinute:
        item.stoppageTimeMinute === null ? "" : String(item.stoppageTimeMinute),
    })),
    appearances: lineup.appearances.map((item) => ({
      playerId: item.playerId,
      minutesPlayed: String(item.minutesPlayed),
    })),
  };
}

function canEdit(
  status: MatchStatus,
  isArchived: boolean,
  reportStatus?: string,
  role?: string | null,
) {
  return (
    !isArchived &&
    status !== "CANCELLED" &&
    !["READY_FOR_REVIEW", "VERIFIED", "ARCHIVED"].includes(
      reportStatus ?? "",
    ) &&
    (role === "ADMIN" || role === "DATA_OPERATOR")
  );
}

export function LineupTab({ match }: { match: MatchResponse }) {
  const { user } = useSession();
  const lineup = useQuery({
    queryKey: ["matches", match.id, "lineup"],
    queryFn: () => matchesApi.getLineup(match.id),
  });
  const report = useQuery({
    queryKey: ["matches", match.id, "report"],
    queryFn: () => matchesApi.getReport(match.id),
    retry: false,
  });
  const editable = canEdit(
    match.status,
    match.isArchived,
    report.data?.status,
    user?.primaryRole,
  );

  if (lineup.isLoading) return <Skeleton className="h-72 w-full" />;
  if (lineup.isError || !lineup.data)
    return <FormErrorSummary errors="Sastav nije moguće učitati." />;

  return (
    <LineupReadMode
      lineup={lineup.data}
      editable={editable}
      locked={Boolean(
        match.isArchived ||
        match.status === "CANCELLED" ||
        ["READY_FOR_REVIEW", "VERIFIED", "ARCHIVED"].includes(
          report.data?.status ?? "",
        ),
      )}
    />
  );
}

function LineupReadMode({
  lineup,
  editable,
  locked,
}: {
  lineup: MatchLineupResponse;
  editable: boolean;
  locked: boolean;
}) {
  const [open, setOpen] = useState(false);
  const playerById = new Map(
    lineup.entries.map((entry) => [entry.player.id, entry.player]),
  );
  const starters = lineup.entries.filter((entry) => entry.role === "STARTER");
  const substitutes = lineup.entries.filter(
    (entry) => entry.role === "SUBSTITUTE",
  );
  const preliminary =
    lineup.matchStatus === "SCHEDULED" || lineup.matchStatus === "POSTPONED";

  return (
    <section className="flex flex-col gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <h2 className="font-heading text-xl">Sastav</h2>
          {preliminary ? (
            <Badge variant="secondary">Preliminarni sastav</Badge>
          ) : null}
          {locked ? <Badge variant="outline">Sastav je zaključan</Badge> : null}
        </div>
        {editable ? (
          <Button onClick={() => setOpen(true)}>
            <Pencil data-icon="inline-start" />
            Uredi sastav
          </Button>
        ) : null}
      </div>
      {lineup.entries.length === 0 && !lineup.formation ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>Nema unesenog sastava</EmptyTitle>
            <EmptyDescription>
              Dodajte formaciju, početni sastav i klupu kada su podaci dostupni.
            </EmptyDescription>
          </EmptyHeader>
        </Empty>
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          <LineupList
            title="Početni sastav"
            entries={starters}
            captainId={lineup.captain?.id}
          />
          <LineupList title="Klupa" entries={substitutes} />
          <dl className="rounded-xl border p-4">
            <dt className="text-muted-foreground text-sm">Formacija</dt>
            <dd className="mt-1 font-medium">
              {lineup.formation || "Nije unesena"}
            </dd>
            <dt className="text-muted-foreground mt-4 text-sm">Kapiten</dt>
            <dd className="mt-1 font-medium">
              {lineup.captain ? playerName(lineup.captain) : "Nije određen"}
            </dd>
          </dl>
          {lineup.matchStatus === "PLAYED" ? (
            <ParticipationReadMode lineup={lineup} playerById={playerById} />
          ) : null}
        </div>
      )}
      {open ? (
        <LineupEditor lineup={lineup} onClose={() => setOpen(false)} />
      ) : null}
    </section>
  );
}

function LineupList({
  title,
  entries,
  captainId,
}: {
  title: string;
  entries: MatchLineupResponse["entries"];
  captainId?: string;
}) {
  return (
    <section className="rounded-xl border p-4">
      <h3 className="font-medium">
        {title}{" "}
        <span className="text-muted-foreground font-normal">
          ({entries.length})
        </span>
      </h3>
      <ol className="mt-3 flex flex-col gap-2">
        {entries.length ? (
          entries.map((entry) => (
            <li
              className="flex items-center justify-between gap-2"
              key={entry.id}
            >
              <span>{playerName(entry.player)}</span>
              {entry.player.id === captainId ? (
                <Badge variant="outline">Kapiten</Badge>
              ) : null}
            </li>
          ))
        ) : (
          <li className="text-muted-foreground text-sm">Nema igrača.</li>
        )}
      </ol>
    </section>
  );
}

function ParticipationReadMode({
  lineup,
  playerById,
}: {
  lineup: MatchLineupResponse;
  playerById: Map<string, MatchLineupPlayer>;
}) {
  return (
    <section className="rounded-xl border p-4 lg:col-span-2">
      <h3 className="font-medium">Nastupi i izmjene</h3>
      <div className="mt-3 grid gap-5 lg:grid-cols-2">
        <div>
          <p className="text-muted-foreground text-sm">Minute</p>
          <ul className="mt-2 flex flex-col gap-1">
            {lineup.appearances.map((appearance) => (
              <li className="flex justify-between gap-2" key={appearance.id}>
                <span>
                  {playerById.get(appearance.playerId)
                    ? playerName(playerById.get(appearance.playerId)!)
                    : "Igrač"}
                </span>
                <span className="font-mono">{appearance.minutesPlayed}</span>
              </li>
            ))}
          </ul>
        </div>
        <div>
          <p className="text-muted-foreground text-sm">Izmjene</p>
          <ol className="mt-2 flex flex-col gap-1">
            {lineup.substitutions.map((substitution) => (
              <li key={substitution.id}>
                {substitution.sequence}.{" "}
                {playerName(playerById.get(substitution.playerOutId)!)} izašao,{" "}
                {playerName(playerById.get(substitution.playerInId)!)} ušao (
                {substitution.minute}
                {substitution.stoppageTimeMinute !== null
                  ? `+${substitution.stoppageTimeMinute}`
                  : "}'"}
                )
              </li>
            ))}
          </ol>
        </div>
      </div>
    </section>
  );
}

function LineupEditor({
  lineup,
  onClose,
}: {
  lineup: MatchLineupResponse;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const candidates = useQuery({
    queryKey: ["matches", lineup.matchId, "lineup", "eligible-players"],
    queryFn: () => matchesApi.getEligibleLineupPlayers(lineup.matchId),
  });
  const form = useForm<LineupForm>({
    resolver: zodResolver(formSchema),
    defaultValues: emptyForm(lineup),
  });
  const entries = useFieldArray({ control: form.control, name: "entries" });
  const substitutions = useFieldArray({
    control: form.control,
    name: "substitutions",
  });
  const [discardOpen, setDiscardOpen] = useState(false);
  const [pendingRemoval, setPendingRemoval] = useState<number | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const values = useWatch({ control: form.control }) as LineupForm;
  const playerMap = useMemo(
    () =>
      new Map<string, MatchLineupPlayer>([
        ...lineup.entries.map(
          (entry) => [entry.player.id, entry.player] as const,
        ),
        ...(candidates.data ?? []).map(
          (player) => [player.id, player] as const,
        ),
      ]),
    [candidates.data, lineup.entries],
  );
  const starters = values.entries.filter((entry) => entry.role === "STARTER");
  const selectedIds = new Set(values.entries.map((entry) => entry.playerId));
  const appearances = useMemo(() => {
    if (lineup.matchStatus !== "PLAYED") return [];
    const ids = [
      ...starters.map((entry) => entry.playerId),
      ...values.substitutions.map((entry) => entry.playerInId).filter(Boolean),
    ];
    return [...new Set(ids)];
  }, [lineup.matchStatus, starters, values.substitutions]);

  useEffect(() => {
    const previous = new Map(
      values.appearances.map((item) => [item.playerId, item.minutesPlayed]),
    );
    const next = appearances.map((playerId) => ({
      playerId,
      minutesPlayed: previous.get(playerId) ?? "",
    }));
    if (JSON.stringify(next) !== JSON.stringify(values.appearances))
      form.setValue("appearances", next, { shouldDirty: true });
  }, [appearances, form, values.appearances]);

  const save = useMutation({
    mutationFn: (request: SaveMatchLineupRequest) =>
      matchesApi.saveLineup(lineup.matchId, request),
    onSuccess: async () => {
      toast.success("Sastav je sačuvan.");
      await queryClient.invalidateQueries({
        queryKey: ["matches", lineup.matchId, "lineup"],
      });
      onClose();
    },
    onError: async (error) => {
      setSubmitError(
        isApiError(error) ? error.message : "Sastav nije moguće sačuvati.",
      );
      if (isApiError(error) && [403, 409].includes(error.status))
        await queryClient.invalidateQueries({
          queryKey: ["matches", lineup.matchId],
        });
    },
  });

  function addPlayer(playerId: string, role: "STARTER" | "SUBSTITUTE") {
    if (!selectedIds.has(playerId)) entries.append({ playerId, role });
  }
  function removePlayer(index: number) {
    const id = values.entries[index].playerId;
    const hasDependentData =
      values.captainPlayerId === id ||
      values.substitutions.some(
        (item) => item.playerInId === id || item.playerOutId === id,
      ) ||
      values.appearances.some(
        (item) => item.playerId === id && item.minutesPlayed !== "",
      );
    if (hasDependentData) {
      setPendingRemoval(index);
      return;
    }
    removePlayerAndDependencies(index);
  }
  function removePlayerAndDependencies(index: number) {
    const id = form.getValues(`entries.${index}.playerId`);
    const currentSubstitutions = form.getValues("substitutions");
    const currentAppearances = form.getValues("appearances");
    entries.remove(index);
    form.setValue(
      "substitutions",
      currentSubstitutions.filter(
        (item) => item.playerInId !== id && item.playerOutId !== id,
      ),
      { shouldDirty: true },
    );
    form.setValue(
      "appearances",
      currentAppearances.filter((item) => item.playerId !== id),
      { shouldDirty: true },
    );
    if (form.getValues("captainPlayerId") === id)
      form.setValue("captainPlayerId", "", { shouldDirty: true });
  }
  function moveEntry(index: number, direction: -1 | 1) {
    const target = index + direction;
    if (target >= 0 && target < entries.fields.length)
      entries.move(index, target);
  }
  function submit(values: LineupForm) {
    setSubmitError(null);
    const ids = values.entries.map((entry) => entry.playerId);
    if (new Set(ids).size !== ids.length)
      return setSubmitError("Igrač može biti dodan samo jednom.");
    if (
      values.captainPlayerId &&
      !values.entries.some(
        (entry) =>
          entry.role === "STARTER" && entry.playerId === values.captainPlayerId,
      )
    )
      return setSubmitError("Kapiten mora biti u početnom sastavu.");
    if (
      lineup.matchStatus === "PLAYED" &&
      starters.length &&
      !values.captainPlayerId
    )
      return setSubmitError(
        "Za odigranu utakmicu kapiten je obavezan kada postoji početni sastav.",
      );
    if (
      lineup.matchStatus === "PLAYED" &&
      (values.substitutions.some(
        (item) =>
          !item.playerOutId ||
          !item.playerInId ||
          item.playerOutId === item.playerInId ||
          !isNonNegativeInteger(item.minute) ||
          (item.stoppageTimeMinute &&
            !isNonNegativeInteger(item.stoppageTimeMinute)),
      ) ||
        values.appearances.some(
          (item) => !isNonNegativeInteger(item.minutesPlayed),
        ))
    )
      return setSubmitError(
        "Provjerite izmjene i minute. Sve vrijednosti moraju biti nenegativni cijeli brojevi.",
      );
    const substitutionError = validateOrderedSubstitutions(
      values.entries,
      values.substitutions,
    );
    if (lineup.matchStatus === "PLAYED" && substitutionError) {
      return setSubmitError(substitutionError);
    }
    save.mutate({
      formation: values.formation.trim() || null,
      captainPlayerId: values.captainPlayerId || null,
      entries: values.entries.map(({ playerId, role }) => ({ playerId, role })),
      appearances:
        lineup.matchStatus === "PLAYED"
          ? values.appearances.map((item) => ({
              playerId: item.playerId,
              minutesPlayed: Number(item.minutesPlayed),
            }))
          : [],
      substitutions:
        lineup.matchStatus === "PLAYED"
          ? values.substitutions.map((item, index) => ({
              playerOutId: item.playerOutId,
              playerInId: item.playerInId,
              minute: Number(item.minute),
              stoppageTimeMinute: item.stoppageTimeMinute
                ? Number(item.stoppageTimeMinute)
                : null,
              sequence: index + 1,
            }))
          : [],
    });
  }

  return (
    <>
      <Dialog
        open
        onOpenChange={(open) => {
          if (!open) {
            if (form.formState.isDirty) {
              setDiscardOpen(true);
            } else {
              onClose();
            }
          }
        }}
      >
        <DialogContent className="flex max-h-[calc(100dvh-2rem)] flex-col gap-0 overflow-hidden p-0 sm:max-w-4xl">
          <DialogHeader className="shrink-0 border-b px-4 py-4 pr-12 sm:px-6">
            <DialogTitle>Uredi sastav</DialogTitle>
            <DialogDescription>
              Promjene se čuvaju jednom atomskom radnjom.
            </DialogDescription>
          </DialogHeader>
          <form
            className="flex min-h-0 flex-1 flex-col overflow-hidden"
            onSubmit={form.handleSubmit(submit)}
          >
            <div className="no-scrollbar min-h-0 flex-1 overflow-y-auto px-4 py-4 sm:px-6">
              <FieldGroup>
                <FormErrorSummary errors={submitError} />
                <Field>
                  <FieldLabel htmlFor="formation">Formacija</FieldLabel>
                  <Input
                    id="formation"
                    placeholder="npr. 4-3-3"
                    {...form.register("formation")}
                  />
                </Field>
                <LineupEditorList
                  title="Početni sastav"
                  role="STARTER"
                  fields={entries.fields}
                  values={values.entries}
                  playerMap={playerMap}
                  candidates={candidates.data ?? []}
                  selectedIds={selectedIds}
                  onAdd={addPlayer}
                  onRemove={removePlayer}
                  onMove={moveEntry}
                  onRoleChange={(index, role) =>
                    form.setValue(`entries.${index}.role`, role, {
                      shouldDirty: true,
                    })
                  }
                />
                <LineupEditorList
                  title="Klupa"
                  role="SUBSTITUTE"
                  fields={entries.fields}
                  values={values.entries}
                  playerMap={playerMap}
                  candidates={candidates.data ?? []}
                  selectedIds={selectedIds}
                  onAdd={addPlayer}
                  onRemove={removePlayer}
                  onMove={moveEntry}
                  onRoleChange={(index, role) =>
                    form.setValue(`entries.${index}.role`, role, {
                      shouldDirty: true,
                    })
                  }
                />
                <Field>
                  <FieldLabel>Kapiten</FieldLabel>
                  <Select
                    value={values.captainPlayerId || null}
                    onValueChange={(value) =>
                      form.setValue("captainPlayerId", value ?? "", {
                        shouldDirty: true,
                      })
                    }
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Odaberite kapitena">
                        {values.captainPlayerId &&
                        playerMap.get(values.captainPlayerId)
                          ? playerName(playerMap.get(values.captainPlayerId)!)
                          : "Odaberite kapitena"}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {starters.map((entry) => (
                          <SelectItem
                            key={entry.playerId}
                            value={entry.playerId}
                          >
                            {playerMap.get(entry.playerId)
                              ? playerName(playerMap.get(entry.playerId)!)
                              : "Igrač"}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                </Field>
                {lineup.matchStatus === "PLAYED" ? (
                  <PlayedFields
                    form={form}
                    substitutions={substitutions}
                    values={values}
                    playerMap={playerMap}
                    starters={starters}
                    appearances={appearances}
                  />
                ) : null}
              </FieldGroup>
            </div>
            <DialogFooter className="mx-0 mb-0 shrink-0 rounded-none px-4 py-3 sm:px-6">
              <Button
                type="button"
                variant="outline"
                onClick={() =>
                  form.formState.isDirty ? setDiscardOpen(true) : onClose()
                }
              >
                Odustani
              </Button>
              <Button disabled={save.isPending} type="submit">
                Sačuvaj sastav
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
      <Dialog
        open={pendingRemoval !== null}
        onOpenChange={(open) => !open && setPendingRemoval(null)}
      >
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>Ukloniti igrača?</DialogTitle>
            <DialogDescription>
              Uklanjanjem igrača izgubit ćete povezane podatke o kapitenu,
              izmjenama ili minutama.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPendingRemoval(null)}>
              Odustani
            </Button>
            <Button
              variant="destructive"
              onClick={() => {
                if (pendingRemoval !== null) {
                  removePlayerAndDependencies(pendingRemoval);
                }
                setPendingRemoval(null);
              }}
            >
              Ukloni igrača
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <Dialog open={discardOpen} onOpenChange={setDiscardOpen}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>Odbaciti promjene?</DialogTitle>
            <DialogDescription>
              Nesačuvane promjene sastava bit će izgubljene.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDiscardOpen(false)}>
              Nastavi uređivati
            </Button>
            <Button variant="destructive" onClick={onClose}>
              Odbaci promjene
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

function LineupEditorList({
  title,
  role,
  fields,
  values,
  playerMap,
  candidates,
  selectedIds,
  onAdd,
  onRemove,
  onMove,
  onRoleChange,
}: {
  title: string;
  role: "STARTER" | "SUBSTITUTE";
  fields: Array<{ id: string }>;
  values: LineupForm["entries"];
  playerMap: Map<string, MatchLineupPlayer>;
  candidates: EligibleLineupPlayer[];
  selectedIds: Set<string>;
  onAdd: (id: string, role: "STARTER" | "SUBSTITUTE") => void;
  onRemove: (index: number) => void;
  onMove: (index: number, direction: -1 | 1) => void;
  onRoleChange: (index: number, role: "STARTER" | "SUBSTITUTE") => void;
}) {
  const filtered = fields
    .map((field, index) => ({ field, index, value: values[index] }))
    .filter((item) => item.value?.role === role);
  return (
    <section className="rounded-xl border p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="font-medium">
          {title}{" "}
          <span className="text-muted-foreground font-normal">
            ({filtered.length})
          </span>
        </h3>
        <Select
          onValueChange={(value) => {
            if (typeof value === "string") onAdd(value, role);
          }}
        >
          <SelectTrigger size="sm">
            <Plus data-icon="inline-start" />
            <SelectValue placeholder="Dodaj igrača" />
          </SelectTrigger>
          <SelectContent>
            <SelectGroup>
              {candidates
                .filter((player) => !selectedIds.has(player.id))
                .map((player) => (
                  <SelectItem key={player.id} value={player.id}>
                    {playerName(player)}
                  </SelectItem>
                ))}
            </SelectGroup>
          </SelectContent>
        </Select>
      </div>
      <ol className="mt-3 flex flex-col gap-2">
        {filtered.map(({ field, index, value }) => (
          <li
            className="flex items-center justify-between gap-2"
            key={field.id}
          >
            <span>
              {playerMap.get(value.playerId)
                ? playerName(playerMap.get(value.playerId)!)
                : "Historijski igrač"}
            </span>
            <div className="flex items-center gap-1">
              <Button
                aria-label="Premjesti gore"
                size="icon-xs"
                type="button"
                variant="ghost"
                onClick={() => onMove(index, -1)}
              >
                <ArrowUp />
              </Button>
              <Button
                aria-label="Premjesti dolje"
                size="icon-xs"
                type="button"
                variant="ghost"
                onClick={() => onMove(index, 1)}
              >
                <ArrowDown />
              </Button>
              <Button
                size="xs"
                type="button"
                variant="outline"
                onClick={() =>
                  onRoleChange(
                    index,
                    role === "STARTER" ? "SUBSTITUTE" : "STARTER",
                  )
                }
              >
                {role === "STARTER" ? "Na klupu" : "U sastav"}
              </Button>
              <Button
                aria-label="Ukloni igrača"
                size="icon-xs"
                type="button"
                variant="destructive"
                onClick={() => onRemove(index)}
              >
                <Trash2 />
              </Button>
            </div>
          </li>
        ))}
      </ol>
    </section>
  );
}

function PlayedFields({
  form,
  substitutions,
  values,
  playerMap,
  starters,
  appearances,
}: {
  form: ReturnType<typeof useForm<LineupForm>>;
  substitutions: ReturnType<typeof useFieldArray<LineupForm, "substitutions">>;
  values: LineupForm;
  playerMap: Map<string, MatchLineupPlayer>;
  starters: LineupForm["entries"];
  appearances: string[];
}) {
  const lineupIds = values.entries.map((entry) => entry.playerId);
  return (
    <>
      <section className="rounded-xl border p-4">
        <div className="flex items-center justify-between gap-2">
          <h3 className="font-medium">Izmjene</h3>
          <Button
            size="sm"
            type="button"
            variant="outline"
            onClick={() =>
              substitutions.append({
                playerOutId: "",
                playerInId: "",
                minute: "",
                stoppageTimeMinute: "",
              })
            }
          >
            <Plus data-icon="inline-start" />
            Dodaj izmjenu
          </Button>
        </div>
        <div className="mt-3 flex flex-col gap-3">
          {substitutions.fields.map((field, index) => (
            <div
              className="grid gap-2 rounded-lg border p-3 md:grid-cols-5"
              key={field.id}
            >
              <Field>
                <FieldLabel>Izašao</FieldLabel>
                <PlayerSelect
                  ids={lineupIds}
                  playerMap={playerMap}
                  value={values.substitutions[index]?.playerOutId}
                  onChange={(value) =>
                    form.setValue(`substitutions.${index}.playerOutId`, value, {
                      shouldDirty: true,
                    })
                  }
                />
              </Field>
              <Field>
                <FieldLabel>Ušao</FieldLabel>
                <PlayerSelect
                  ids={lineupIds}
                  playerMap={playerMap}
                  value={values.substitutions[index]?.playerInId}
                  onChange={(value) =>
                    form.setValue(`substitutions.${index}.playerInId`, value, {
                      shouldDirty: true,
                    })
                  }
                />
              </Field>
              <Field>
                <FieldLabel>Minuta</FieldLabel>
                <Input
                  inputMode="numeric"
                  {...form.register(`substitutions.${index}.minute`)}
                />
              </Field>
              <Field>
                <FieldLabel>Nadoknada</FieldLabel>
                <Input
                  inputMode="numeric"
                  {...form.register(
                    `substitutions.${index}.stoppageTimeMinute`,
                  )}
                />
              </Field>
              <div className="flex items-end gap-1">
                <Button
                  aria-label={`Pomjeri izmjenu ${index + 1} gore`}
                  disabled={index === 0}
                  size="icon-sm"
                  type="button"
                  variant="ghost"
                  onClick={() => substitutions.move(index, index - 1)}
                >
                  <ArrowUp />
                </Button>
                <Button
                  aria-label={`Pomjeri izmjenu ${index + 1} dolje`}
                  disabled={index === substitutions.fields.length - 1}
                  size="icon-sm"
                  type="button"
                  variant="ghost"
                  onClick={() => substitutions.move(index, index + 1)}
                >
                  <ArrowDown />
                </Button>
                <Button
                  aria-label="Ukloni izmjenu"
                  size="icon-sm"
                  type="button"
                  variant="destructive"
                  onClick={() => substitutions.remove(index)}
                >
                  <Trash2 />
                </Button>
              </div>
            </div>
          ))}
        </div>
      </section>
      <section className="rounded-xl border p-4">
        <h3 className="font-medium">Nastupi i minute</h3>
        <p className="text-muted-foreground mt-1 text-sm">
          Minute unosite ručno za početni sastav i igrače koji su ušli u igru.
        </p>
        <div className="mt-3 grid gap-3 md:grid-cols-2">
          {appearances.map((playerId) => {
            const index = values.appearances.findIndex(
              (item) => item.playerId === playerId,
            );
            return (
              <Field key={playerId}>
                <FieldLabel htmlFor={`minutes-${playerId}`}>
                  {playerMap.get(playerId)
                    ? playerName(playerMap.get(playerId)!)
                    : "Igrač"}
                  {starters.some((entry) => entry.playerId === playerId)
                    ? " (starter)"
                    : " (zamjena)"}
                </FieldLabel>
                <Input
                  id={`minutes-${playerId}`}
                  inputMode="numeric"
                  {...form.register(`appearances.${index}.minutesPlayed`)}
                />
              </Field>
            );
          })}
        </div>
      </section>
    </>
  );
}

function PlayerSelect({
  ids,
  playerMap,
  value,
  onChange,
}: {
  ids: string[];
  playerMap: Map<string, MatchLineupPlayer>;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <Select
      value={value || null}
      onValueChange={(next) => onChange(next ?? "")}
    >
      <SelectTrigger className="w-full">
        <SelectValue placeholder="Odaberite igrača">
          {value && playerMap.get(value)
            ? playerName(playerMap.get(value)!)
            : "Odaberite igrača"}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          {ids.map((id) => (
            <SelectItem key={id} value={id}>
              {playerMap.get(id) ? playerName(playerMap.get(id)!) : "Igrač"}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
