import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { DatePicker } from "@/components/common/date-picker";
import { Button } from "@/components/ui/button";
import { invalidateDashboardOverview } from "@/features/dashboard";
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
import { matchesApi } from "@/features/matches/api/matches-api";
import type { SessionUser } from "@/features/auth/types/session";
import type {
  MatchResponse,
  MatchStatus,
} from "@/features/matches/types/match";
import {
  localDateTimeParts,
  locationLabels,
  matchStatusLabels,
  toUtc,
} from "@/features/matches/utils/display";
import { settingsApi } from "@/features/settings/api";
import { isApiError } from "@/lib/api/api-client";

const editSchema = z
  .object({
    seasonId: z.string().min(1, "Odaberite sezonu."),
    competitionId: z.string().min(1, "Odaberite takmičenje."),
    opponentId: z.string().min(1, "Odaberite protivnika."),
    venueId: z.string(),
    date: z.string().min(1, "Odaberite datum."),
    time: z.string().regex(/^\d{2}:\d{2}$/, "Unesite vrijeme."),
    round: z.string().trim().max(80, "Kolo može imati najviše 80 znakova."),
    locationType: z.enum(["HOME", "AWAY", "NEUTRAL"]),
    status: z.enum(["SCHEDULED", "PLAYED", "POSTPONED", "CANCELLED"]),
    teamScore: z.string(),
    opponentScore: z.string(),
  })
  .superRefine((value, context) => {
    if (value.status !== "PLAYED") return;
    for (const field of ["teamScore", "opponentScore"] as const) {
      const score = value[field];
      if (!/^\d+$/.test(score)) {
        context.addIssue({
          code: "custom",
          path: [field],
          message: "Unesite cijeli broj od nule.",
        });
      }
    }
  });

type EditValues = z.infer<typeof editSchema>;

const createSchema = z.object({
  seasonId: z.string().min(1, "Odaberite sezonu."),
  competitionId: z.string().min(1, "Odaberite takmičenje."),
  teamId: z.string().min(1, "Odaberite selekciju."),
  opponentId: z.string().min(1, "Odaberite protivnika."),
  venueId: z.string(),
  date: z.string().min(1, "Odaberite datum."),
  time: z.string().regex(/^\d{2}:\d{2}$/, "Unesite vrijeme."),
  round: z.string().trim().max(80, "Kolo može imati najviše 80 znakova."),
  locationType: z.enum(["HOME", "AWAY", "NEUTRAL"]),
});
type CreateValues = z.infer<typeof createSchema>;

const statusesByCurrentStatus: Record<MatchStatus, MatchStatus[]> = {
  SCHEDULED: ["SCHEDULED", "PLAYED", "POSTPONED", "CANCELLED"],
  POSTPONED: ["POSTPONED", "SCHEDULED", "CANCELLED"],
  PLAYED: ["PLAYED"],
  CANCELLED: ["CANCELLED"],
};

function errorMessage(error: unknown) {
  if (!isApiError(error)) return "Radnja nije uspjela. Pokušajte ponovo.";
  if (error.status === 403) return "Nemate ovlaštenje za ovu radnju.";
  if (error.status === 404) return "Utakmica više nije dostupna.";
  if (error.code === "duplicate_match")
    return "Ista aktivna utakmica već postoji.";
  if (error.code === "report_workflow_locked")
    return "Utakmica se ne može mijenjati dok je izvještaj na pregledu, verificiran ili arhiviran.";
  if (error.code === "match_conflict")
    return "Ova promjena statusa nije dozvoljena.";
  return error.detail ?? "Radnja nije moguća.";
}

function refreshMatches(
  queryClient: ReturnType<typeof useQueryClient>,
  id: string,
) {
  void queryClient.invalidateQueries({ queryKey: ["matches"] });
  void queryClient.invalidateQueries({ queryKey: ["match", id] });
  void invalidateDashboardOverview(queryClient);
}

function SelectField({
  id,
  label,
  value,
  onChange,
  options,
  placeholder,
  error,
}: {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: Array<{ id: string; name: string }>;
  placeholder: string;
  error?: string;
}) {
  const selectedLabel = value
    ? options.find((option) => option.id === value)?.name
    : undefined;
  return (
    <Field data-invalid={Boolean(error)}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Select
        value={value || null}
        onValueChange={(next) => onChange(next ?? "")}
      >
        <SelectTrigger id={id} aria-invalid={Boolean(error)} className="w-full">
          <SelectValue placeholder={placeholder}>
            {selectedLabel ?? placeholder}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            {options.map((option) => (
              <SelectItem key={option.id} value={option.id}>
                {option.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
      {error ? <FieldError>{error}</FieldError> : null}
    </Field>
  );
}

export function MatchCreateDialog({
  user,
  onClose,
  onCreated,
}: {
  user: SessionUser;
  onClose: () => void;
  onCreated: (match: MatchResponse) => void;
}) {
  const queryClient = useQueryClient();
  const options = useQuery({
    queryKey: ["matches", "create-options"],
    queryFn: async () => {
      const [seasons, teams, competitions, opponents, venues] =
        await Promise.all([
          settingsApi.listSeasons(false),
          settingsApi.listTeams(false),
          settingsApi.listNamed("competitions", false),
          settingsApi.listNamed("opponents", false),
          settingsApi.listNamed("venues", false),
        ]);
      return { seasons, teams, competitions, opponents, venues };
    },
    retry: false,
  });
  const form = useForm<CreateValues>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      seasonId: "",
      competitionId: "",
      teamId: "",
      opponentId: "",
      venueId: "",
      date: "",
      time: "",
      round: "",
      locationType: "HOME",
    },
  });
  const values = useWatch({ control: form.control }) as CreateValues;
  const mutation = useMutation({
    mutationFn: (values: CreateValues) =>
      matchesApi.create({
        seasonId: values.seasonId,
        competitionId: values.competitionId,
        teamId: values.teamId,
        opponentId: values.opponentId,
        venueId: values.venueId || null,
        kickoffAtUtc: toUtc(values.date, values.time),
        round: values.round || null,
        locationType: values.locationType,
      }),
    onSuccess: (match) => {
      void queryClient.invalidateQueries({ queryKey: ["matches"] });
      onCreated(match);
    },
    onError: (error) => {
      if (isApiError(error) && error.validationErrors) {
        Object.entries(error.validationErrors).forEach(([key, messages]) => {
          const field = key.charAt(0).toLowerCase() + key.slice(1);
          if (field in form.getValues()) {
            form.setError(field as keyof CreateValues, {
              message: messages[0],
            });
          }
        });
      }
    },
  });
  const teams = (options.data?.teams ?? []).filter(
    (team) =>
      team.status === "ACTIVE" &&
      (user.primaryRole === "ADMIN" ||
        user.teamScope.type === "ALL" ||
        user.teamScope.selectedTeamIds.includes(team.id)),
  );
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent
        className="max-h-[calc(100dvh-2rem)] grid-rows-[auto_minmax(0,1fr)_auto] sm:max-w-2xl"
        showCloseButton={!mutation.isPending}
      >
        <DialogHeader>
          <DialogTitle>Nova utakmica</DialogTitle>
          <DialogDescription>
            Nova utakmica bit će spremljena sa statusom Zakazana.
          </DialogDescription>
        </DialogHeader>
        <div className="no-scrollbar -mx-4 min-h-0 overflow-y-auto px-4">
          <form
            id="create-match-form"
            className="py-1"
            onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
          >
            <FieldGroup>
              <div className="grid gap-4 sm:grid-cols-2">
                <SelectField
                  id="seasonId"
                  label="Sezona"
                  value={values.seasonId}
                  onChange={(value) =>
                    form.setValue("seasonId", value, { shouldValidate: true })
                  }
                  options={options.data?.seasons ?? []}
                  placeholder="Odaberite sezonu"
                  error={form.formState.errors.seasonId?.message}
                />
                <SelectField
                  id="competitionId"
                  label="Takmičenje"
                  value={values.competitionId}
                  onChange={(value) =>
                    form.setValue("competitionId", value, {
                      shouldValidate: true,
                    })
                  }
                  options={options.data?.competitions ?? []}
                  placeholder="Odaberite takmičenje"
                  error={form.formState.errors.competitionId?.message}
                />
                <SelectField
                  id="teamId"
                  label="Selekcija"
                  value={values.teamId}
                  onChange={(value) =>
                    form.setValue("teamId", value, { shouldValidate: true })
                  }
                  options={teams}
                  placeholder="Odaberite selekciju"
                  error={form.formState.errors.teamId?.message}
                />
                <SelectField
                  id="opponentId"
                  label="Protivnik"
                  value={values.opponentId}
                  onChange={(value) =>
                    form.setValue("opponentId", value, { shouldValidate: true })
                  }
                  options={options.data?.opponents ?? []}
                  placeholder="Odaberite protivnika"
                  error={form.formState.errors.opponentId?.message}
                />
                <SelectField
                  id="venueId"
                  label="Mjesto"
                  value={values.venueId}
                  onChange={(value) => form.setValue("venueId", value)}
                  options={[
                    { id: "", name: "Nije određeno" },
                    ...(options.data?.venues ?? []),
                  ]}
                  placeholder="Odaberite mjesto"
                  error={form.formState.errors.venueId?.message}
                />
                <SelectField
                  id="locationType"
                  label="Lokacija"
                  value={values.locationType}
                  onChange={(value) =>
                    form.setValue(
                      "locationType",
                      value as CreateValues["locationType"],
                      { shouldValidate: true },
                    )
                  }
                  options={Object.entries(locationLabels).map(([id, name]) => ({
                    id,
                    name,
                  }))}
                  placeholder="Odaberite lokaciju"
                  error={form.formState.errors.locationType?.message}
                />
                <Field data-invalid={Boolean(form.formState.errors.date)}>
                  <FieldLabel>Datum početka</FieldLabel>
                  <DatePicker
                    id="create-match-date"
                    value={values.date}
                    onChange={(value) =>
                      form.setValue("date", value, { shouldValidate: true })
                    }
                    aria-invalid={Boolean(form.formState.errors.date)}
                  />
                  {form.formState.errors.date?.message ? (
                    <FieldError>
                      {form.formState.errors.date.message}
                    </FieldError>
                  ) : null}
                </Field>
                <Field data-invalid={Boolean(form.formState.errors.time)}>
                  <FieldLabel htmlFor="create-match-time">
                    Vrijeme početka
                  </FieldLabel>
                  <Input
                    id="create-match-time"
                    step={60}
                    type="time"
                    aria-invalid={Boolean(form.formState.errors.time)}
                    {...form.register("time")}
                  />
                  {form.formState.errors.time?.message ? (
                    <FieldError>
                      {form.formState.errors.time.message}
                    </FieldError>
                  ) : null}
                </Field>
                <Field>
                  <FieldLabel htmlFor="create-match-round">
                    Kolo / faza
                  </FieldLabel>
                  <Input id="create-match-round" {...form.register("round")} />
                  {form.formState.errors.round?.message ? (
                    <FieldError>
                      {form.formState.errors.round.message}
                    </FieldError>
                  ) : null}
                </Field>
              </div>
              {mutation.error ? (
                <FieldError>{errorMessage(mutation.error)}</FieldError>
              ) : null}
            </FieldGroup>
          </form>
        </div>
        <DialogFooter>
          <DialogClose
            render={<Button variant="outline" disabled={mutation.isPending} />}
          >
            Odustani
          </DialogClose>
          <Button
            form="create-match-form"
            disabled={mutation.isPending || options.isLoading}
            type="submit"
          >
            {mutation.isPending ? "Spremanje..." : "Kreiraj utakmicu"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function MatchEditDialog({
  match,
  onClose,
}: {
  match: MatchResponse;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const options = useQuery({
    queryKey: ["matches", "edit-options"],
    queryFn: async () => {
      const [seasons, competitions, opponents, venues] = await Promise.all([
        settingsApi.listSeasons(false),
        settingsApi.listNamed("competitions", false),
        settingsApi.listNamed("opponents", false),
        settingsApi.listNamed("venues", false),
      ]);
      return { seasons, competitions, opponents, venues };
    },
    retry: false,
  });
  const form = useForm<EditValues>({ resolver: zodResolver(editSchema) });
  useEffect(() => {
    const local = localDateTimeParts(match.kickoffAtUtc);
    form.reset({
      seasonId: match.season.id,
      competitionId: match.competition.id,
      opponentId: match.opponent.id,
      venueId: match.venue?.id ?? "",
      date: local.date,
      time: local.time,
      round: match.round ?? "",
      locationType: match.locationType,
      status: match.status,
      teamScore: match.teamScore?.toString() ?? "",
      opponentScore: match.opponentScore?.toString() ?? "",
    });
  }, [form, match]);
  const values = useWatch({ control: form.control }) as EditValues;
  const mutation = useMutation({
    mutationFn: (values: EditValues) =>
      matchesApi.update(match.id, {
        seasonId: values.seasonId,
        competitionId: values.competitionId,
        opponentId: values.opponentId,
        venueId: values.venueId || null,
        kickoffAtUtc: toUtc(values.date, values.time),
        round: values.round || null,
        locationType: values.locationType,
        status: values.status,
        teamScore: values.status === "PLAYED" ? Number(values.teamScore) : null,
        opponentScore:
          values.status === "PLAYED" ? Number(values.opponentScore) : null,
      }),
    onSuccess: () => {
      refreshMatches(queryClient, match.id);
      onClose();
    },
    onError: (error) => {
      if (isApiError(error) && error.validationErrors) {
        Object.entries(error.validationErrors).forEach(([key, messages]) => {
          const field = key.charAt(0).toLowerCase() + key.slice(1);
          if (field in form.getValues())
            form.setError(field as keyof EditValues, { message: messages[0] });
        });
      }
      if (isApiError(error) && error.code === "report_workflow_locked")
        refreshMatches(queryClient, match.id);
    },
  });
  const selectedStatus = values.status;
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent
        className="max-h-[calc(100dvh-2rem)] grid-rows-[auto_minmax(0,1fr)_auto] sm:max-w-2xl"
        showCloseButton={!mutation.isPending}
      >
        <DialogHeader>
          <DialogTitle>Uredi utakmicu</DialogTitle>
          <DialogDescription>
            Selekcija je nepromjenjiva: {match.team.name}.
          </DialogDescription>
        </DialogHeader>
        <div className="no-scrollbar -mx-4 min-h-0 overflow-y-auto px-4">
          <form
            id="edit-match-form"
            className="py-1"
            onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
          >
            <FieldGroup>
              <div className="grid gap-4 sm:grid-cols-2">
                <SelectField
                  id="seasonId"
                  label="Sezona"
                  value={values.seasonId}
                  onChange={(value) =>
                    form.setValue("seasonId", value, { shouldValidate: true })
                  }
                  options={options.data?.seasons ?? []}
                  placeholder="Odaberite sezonu"
                  error={form.formState.errors.seasonId?.message}
                />
                <SelectField
                  id="competitionId"
                  label="Takmičenje"
                  value={values.competitionId}
                  onChange={(value) =>
                    form.setValue("competitionId", value, {
                      shouldValidate: true,
                    })
                  }
                  options={options.data?.competitions ?? []}
                  placeholder="Odaberite takmičenje"
                  error={form.formState.errors.competitionId?.message}
                />
                <SelectField
                  id="opponentId"
                  label="Protivnik"
                  value={values.opponentId}
                  onChange={(value) =>
                    form.setValue("opponentId", value, { shouldValidate: true })
                  }
                  options={options.data?.opponents ?? []}
                  placeholder="Odaberite protivnika"
                  error={form.formState.errors.opponentId?.message}
                />
                <SelectField
                  id="venueId"
                  label="Mjesto"
                  value={values.venueId}
                  onChange={(value) => form.setValue("venueId", value)}
                  options={[
                    { id: "", name: "Nije određeno" },
                    ...(options.data?.venues ?? []),
                  ]}
                  placeholder="Odaberite mjesto"
                  error={form.formState.errors.venueId?.message}
                />
                <Field data-invalid={Boolean(form.formState.errors.date)}>
                  <FieldLabel>Datum početka</FieldLabel>
                  <DatePicker
                    id="match-date"
                    value={values.date}
                    onChange={(value) =>
                      form.setValue("date", value, { shouldValidate: true })
                    }
                    aria-invalid={Boolean(form.formState.errors.date)}
                  />
                  {form.formState.errors.date?.message ? (
                    <FieldError>
                      {form.formState.errors.date.message}
                    </FieldError>
                  ) : null}
                </Field>
                <Field data-invalid={Boolean(form.formState.errors.time)}>
                  <FieldLabel htmlFor="time">Vrijeme početka</FieldLabel>
                  <Input
                    id="time"
                    step={60}
                    type="time"
                    aria-invalid={Boolean(form.formState.errors.time)}
                    {...form.register("time")}
                  />
                  {form.formState.errors.time?.message ? (
                    <FieldError>
                      {form.formState.errors.time.message}
                    </FieldError>
                  ) : null}
                </Field>
                <Field>
                  <FieldLabel htmlFor="round">Kolo / faza</FieldLabel>
                  <Input id="round" {...form.register("round")} />
                  {form.formState.errors.round?.message ? (
                    <FieldError>
                      {form.formState.errors.round.message}
                    </FieldError>
                  ) : null}
                </Field>
                <SelectField
                  id="locationType"
                  label="Lokacija"
                  value={values.locationType}
                  onChange={(value) =>
                    form.setValue(
                      "locationType",
                      value as EditValues["locationType"],
                      { shouldValidate: true },
                    )
                  }
                  options={Object.entries(locationLabels).map(([id, name]) => ({
                    id,
                    name,
                  }))}
                  placeholder="Odaberite lokaciju"
                  error={form.formState.errors.locationType?.message}
                />
                <SelectField
                  id="status"
                  label="Status"
                  value={selectedStatus}
                  onChange={(value) =>
                    form.setValue("status", value as MatchStatus, {
                      shouldValidate: true,
                    })
                  }
                  options={statusesByCurrentStatus[match.status].map((id) => ({
                    id,
                    name: matchStatusLabels[id],
                  }))}
                  placeholder="Odaberite status"
                  error={form.formState.errors.status?.message}
                />
              </div>
              {selectedStatus === "PLAYED" ? (
                <div className="grid gap-4 sm:grid-cols-2">
                  <Field
                    data-invalid={Boolean(form.formState.errors.teamScore)}
                  >
                    <FieldLabel htmlFor="teamScore">
                      FK Velež rezultat
                    </FieldLabel>
                    <Input
                      id="teamScore"
                      inputMode="numeric"
                      min="0"
                      type="number"
                      aria-invalid={Boolean(form.formState.errors.teamScore)}
                      {...form.register("teamScore")}
                    />
                    {form.formState.errors.teamScore?.message ? (
                      <FieldError>
                        {form.formState.errors.teamScore.message}
                      </FieldError>
                    ) : null}
                  </Field>
                  <Field
                    data-invalid={Boolean(form.formState.errors.opponentScore)}
                  >
                    <FieldLabel htmlFor="opponentScore">
                      Rezultat protivnika
                    </FieldLabel>
                    <Input
                      id="opponentScore"
                      inputMode="numeric"
                      min="0"
                      type="number"
                      aria-invalid={Boolean(
                        form.formState.errors.opponentScore,
                      )}
                      {...form.register("opponentScore")}
                    />
                    {form.formState.errors.opponentScore?.message ? (
                      <FieldError>
                        {form.formState.errors.opponentScore.message}
                      </FieldError>
                    ) : null}
                  </Field>
                </div>
              ) : null}
              {mutation.error ? (
                <FieldError>{errorMessage(mutation.error)}</FieldError>
              ) : null}
            </FieldGroup>
          </form>
        </div>
        <DialogFooter>
          <DialogClose
            render={<Button variant="outline" disabled={mutation.isPending} />}
          >
            Odustani
          </DialogClose>
          <Button
            form="edit-match-form"
            disabled={mutation.isPending || options.isLoading}
            type="submit"
          >
            {mutation.isPending ? "Spremanje..." : "Spremi promjene"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function MatchArchiveDialog({
  match,
  onClose,
}: {
  match: MatchResponse;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: () =>
      match.isArchived
        ? matchesApi.restore(match.id)
        : matchesApi.archive(match.id),
    onSuccess: () => {
      refreshMatches(queryClient, match.id);
      onClose();
    },
    onError: (error) => {
      if (isApiError(error) && error.code === "report_workflow_locked")
        refreshMatches(queryClient, match.id);
    },
  });
  const restore = match.isArchived;
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent showCloseButton={!mutation.isPending}>
        <DialogHeader>
          <DialogTitle>
            {restore ? "Vrati utakmicu iz arhive" : "Arhiviraj utakmicu"}
          </DialogTitle>
          <DialogDescription>
            {restore
              ? "Utakmica će zadržati postojeće podatke i status."
              : "Utakmica će biti skrivena s podrazumijevane liste. Neće biti obrisana niti otkazana."}
          </DialogDescription>
        </DialogHeader>
        {mutation.error ? (
          <FieldError>{errorMessage(mutation.error)}</FieldError>
        ) : null}
        <DialogFooter>
          <DialogClose
            render={<Button variant="outline" disabled={mutation.isPending} />}
          >
            Odustani
          </DialogClose>
          <Button
            variant={restore ? "default" : "destructive"}
            disabled={mutation.isPending}
            onClick={() => mutation.mutate()}
          >
            {mutation.isPending
              ? "Obrada..."
              : restore
                ? "Vrati iz arhive"
                : "Arhiviraj"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
