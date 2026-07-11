import { zodResolver } from "@hookform/resolvers/zod";
import { Archive, ArrowDown, ArrowUp, RotateCcw } from "lucide-react";
import {
  NavLink,
  Outlet,
  useNavigate,
  useSearchParams,
} from "react-router-dom";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { routePaths } from "@/app/route-paths";
import { ErrorState } from "@/components/common/error-state";
import { FormErrorSummary } from "@/components/common/form-error-summary";
import { PageHeader } from "@/components/common/page-header";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
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
import { useSession } from "@/features/auth/hooks/use-session";
import { isApiError } from "@/lib/api/api-client";
import { cn } from "@/lib/utils";

import { settingsNavigation } from "./settings-navigation";
import {
  trackingLevelHelp,
  trackingLevelLabels,
  type TeamTrackingLevel,
} from "./types";

export function SettingsLayout() {
  const { user } = useSession();
  const navigate = useNavigate();
  if (user?.primaryRole !== "ADMIN") {
    return (
      <ErrorState
        title="Pristup nije dozvoljen"
        description="Ovu stranicu mogu otvoriti samo administratori."
        action={
          <Button onClick={() => navigate(routePaths.dashboard)}>
            Na kontrolnu ploču
          </Button>
        }
      />
    );
  }
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Postavke"
        description="Upravljajte osnovnim podacima koji se koriste kroz sistem."
      />
      <nav
        aria-label="Navigacija postavki"
        className="border-border flex gap-2 overflow-x-auto border-b pb-3"
      >
        {settingsNavigation.map(([label, path]) => (
          <NavLink
            key={path}
            to={path}
            className={({ isActive }) =>
              cn(
                "focus-visible:ring-ring/50 rounded-lg px-3 py-2 text-sm whitespace-nowrap focus-visible:ring-3 focus-visible:outline-none",
                isActive
                  ? "bg-primary text-primary-foreground font-medium"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground",
              )
            }
          >
            {label}
          </NavLink>
        ))}
      </nav>
      <Outlet />
    </div>
  );
}

export function ArchivedFilter() {
  const [params, setParams] = useSearchParams();
  const includeArchived = params.get("archived") === "include";
  return (
    <label className="flex items-center gap-2 text-sm">
      <Checkbox
        id="include-archived"
        checked={includeArchived}
        onCheckedChange={(checked) => {
          const next = new URLSearchParams(params);
          if (checked === true) next.set("archived", "include");
          else next.delete("archived");
          setParams(next, { replace: true });
        }}
      />
      <span>Prikaži arhivirane</span>
    </label>
  );
}

function safeError(error: unknown) {
  if (isApiError(error)) {
    if (error.status === 409)
      return "Zapis sa ovim nazivom već postoji ili promjena više nije moguća.";
    if (error.status === 404)
      return "Zapis više nije dostupan. Lista je osvježena.";
    if (error.status === 403) return "Nemate ovlaštenje za ovu radnju.";
  }
  return "Promjene nije moguće sačuvati. Provjerite unesene podatke.";
}

const nameSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Unesite naziv.")
    .max(120, "Naziv može imati najviše 120 znakova."),
});
const seasonSchema = nameSchema
  .extend({
    startDate: z.string().min(1, "Unesite datum početka."),
    endDate: z.string().min(1, "Unesite datum završetka."),
  })
  .refine((value) => value.startDate <= value.endDate, {
    path: ["endDate"],
    message: "Datum završetka ne može biti prije datuma početka.",
  });
const teamSchema = nameSchema.extend({
  trackingLevel: z.enum(["BASIC", "STANDARD", "FULL"]),
});

type DialogKind = "name" | "season" | "team";
type FormValues = z.infer<typeof nameSchema> &
  Partial<z.infer<typeof seasonSchema> & z.infer<typeof teamSchema>>;

export function SettingFormDialog({
  open,
  onClose,
  kind,
  title,
  initial,
  pending,
  error,
  onSubmit,
}: {
  open: boolean;
  onClose: () => void;
  kind: DialogKind;
  title: string;
  initial?: FormValues;
  pending: boolean;
  error: unknown;
  onSubmit: (values: FormValues) => void;
}) {
  const schema =
    kind === "season"
      ? seasonSchema
      : kind === "team"
        ? teamSchema
        : nameSchema;
  const form = useForm<FormValues>({
    resolver: zodResolver(schema) as never,
    defaultValues: initial ?? {
      name: "",
      startDate: "",
      endDate: "",
      trackingLevel: "BASIC",
    },
  });
  useEffect(() => {
    if (open) {
      form.reset(
        initial ?? {
          name: "",
          startDate: "",
          endDate: "",
          trackingLevel: "BASIC",
        },
      );
    }
  }, [form, initial, open]);
  const field = (name: keyof FormValues) =>
    form.formState.errors[name]?.message as string | undefined;
  return (
    <Dialog
      open={open}
      onOpenChange={(value) => !value && !pending && onClose()}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            Popunite obavezna polja i sačuvajte promjene.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="flex flex-col gap-5"
        >
          <FormErrorSummary errors={error ? safeError(error) : null} />
          <FieldGroup>
            <Field data-invalid={Boolean(field("name"))}>
              <FieldLabel htmlFor="setting-name">Naziv</FieldLabel>
              <Input
                id="setting-name"
                autoFocus
                aria-invalid={Boolean(field("name"))}
                {...form.register("name")}
              />
              <FieldError>{field("name")}</FieldError>
            </Field>
            {kind === "season" ? (
              <>
                <Field data-invalid={Boolean(field("startDate"))}>
                  <FieldLabel htmlFor="start-date">Datum početka</FieldLabel>
                  <Input
                    id="start-date"
                    type="date"
                    aria-invalid={Boolean(field("startDate"))}
                    {...form.register("startDate")}
                  />
                  <FieldError>{field("startDate")}</FieldError>
                </Field>
                <Field data-invalid={Boolean(field("endDate"))}>
                  <FieldLabel htmlFor="end-date">Datum završetka</FieldLabel>
                  <Input
                    id="end-date"
                    type="date"
                    aria-invalid={Boolean(field("endDate"))}
                    {...form.register("endDate")}
                  />
                  <FieldError>{field("endDate")}</FieldError>
                </Field>
              </>
            ) : null}
            {kind === "team" ? (
              <Field data-invalid={Boolean(field("trackingLevel"))}>
                <FieldLabel>Nivo praćenja</FieldLabel>
                <Select
                  value={form.watch("trackingLevel") as TeamTrackingLevel}
                  onValueChange={(value) =>
                    form.setValue("trackingLevel", value as TeamTrackingLevel)
                  }
                >
                  <SelectTrigger className="w-full">
                    <SelectValue>
                      {
                        trackingLevelLabels[
                          form.watch("trackingLevel") as TeamTrackingLevel
                        ]
                      }
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      {(
                        Object.keys(trackingLevelLabels) as TeamTrackingLevel[]
                      ).map((level) => (
                        <SelectItem key={level} value={level}>
                          {trackingLevelLabels[level]}
                        </SelectItem>
                      ))}
                    </SelectGroup>
                  </SelectContent>
                </Select>
                <FieldDescription>
                  {
                    trackingLevelHelp[
                      form.watch("trackingLevel") as TeamTrackingLevel
                    ]
                  }{" "}
                  Nivo određuje budući obim dostupnih podataka i tokova rada.
                </FieldDescription>
              </Field>
            ) : null}
          </FieldGroup>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              disabled={pending}
              onClick={onClose}
            >
              Odustani
            </Button>
            <Button type="submit" disabled={pending}>
              {pending ? "Čuvanje..." : "Sačuvaj"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function ConfirmDialog({
  target,
  action,
  pending,
  error,
  onClose,
  onConfirm,
}: {
  target: string;
  action: "archive" | "restore" | "activate" | "deactivate";
  pending: boolean;
  error: unknown;
  onClose: () => void;
  onConfirm: () => void;
}) {
  const archive = action === "archive";
  const title = archive
    ? "Arhiviraj zapis"
    : action === "restore"
      ? "Vrati zapis"
      : action === "activate"
        ? "Aktiviraj selekciju"
        : "Deaktiviraj selekciju";
  return (
    <Dialog open onOpenChange={(value) => !value && !pending && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            {archive ? (
              <>Da li ste sigurni da želite arhivirati „{target}“?</>
            ) : (
              <>Potvrdite promjenu statusa za „{target}“.</>
            )}
          </DialogDescription>
        </DialogHeader>
        <FormErrorSummary errors={error ? safeError(error) : null} />
        <DialogFooter>
          <Button variant="outline" disabled={pending} onClick={onClose}>
            Odustani
          </Button>
          <Button
            variant={archive ? "destructive" : "default"}
            disabled={pending}
            onClick={onConfirm}
          >
            {pending ? "Obrada..." : "Potvrdi"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function LifecycleButtons({
  archived,
  status,
  onEdit,
  onAction,
  ordering,
  moveUp,
  moveDown,
  reorderPending,
}: {
  archived?: boolean;
  status?: string;
  onEdit?: () => void;
  onAction: (action: "archive" | "restore" | "activate" | "deactivate") => void;
  ordering?: boolean;
  moveUp?: () => void;
  moveDown?: () => void;
  reorderPending?: boolean;
}) {
  if (archived || status === "ARCHIVED")
    return (
      <Button size="sm" variant="outline" onClick={() => onAction("restore")}>
        <RotateCcw data-icon="inline-start" />
        Vrati
      </Button>
    );
  return (
    <div className="flex flex-wrap gap-2">
      {ordering ? (
        <>
          <Button
            size="icon-sm"
            variant="outline"
            aria-label="Pomjeri gore"
            disabled={reorderPending || !moveUp}
            onClick={moveUp}
          >
            <ArrowUp />
          </Button>
          <Button
            size="icon-sm"
            variant="outline"
            aria-label="Pomjeri dolje"
            disabled={reorderPending || !moveDown}
            onClick={moveDown}
          >
            <ArrowDown />
          </Button>
        </>
      ) : null}
      {status === "ACTIVE" ? (
        <Button
          size="sm"
          variant="outline"
          onClick={() => onAction("deactivate")}
        >
          Deaktiviraj
        </Button>
      ) : status === "INACTIVE" ? (
        <Button
          size="sm"
          variant="outline"
          onClick={() => onAction("activate")}
        >
          Aktiviraj
        </Button>
      ) : null}
      {onEdit ? (
        <Button size="sm" variant="outline" onClick={onEdit}>
          Uredi
        </Button>
      ) : null}
      <Button
        size="sm"
        variant="destructive"
        onClick={() => onAction("archive")}
      >
        <Archive data-icon="inline-start" />
        Arhiviraj
      </Button>
    </div>
  );
}
