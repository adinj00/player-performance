import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { DatePicker } from "@/components/common/date-picker";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { formatUtcDateTime } from "@/lib/date-format";
import {
  asAuditRecord,
  type AuditItem,
  type PagedAuditHistory,
} from "@/features/audit/types";

export type AuditLabels = Record<string, string>;
export type AuditFieldLabels = Record<string, string>;

const sensitive = /password|token|secret|securitystamp|credential/i;

function auditValue(value: unknown, depth = 0): string {
  if (value === null || value === undefined) return "Nije postavljeno";
  if (typeof value === "boolean") return value ? "Da" : "Ne";
  if (typeof value === "string" || typeof value === "number")
    return String(value);
  if (depth >= 2) return "Složena vrijednost";
  if (Array.isArray(value))
    return value.length
      ? value.map((item) => auditValue(item, depth + 1)).join(", ")
      : "Nije postavljeno";
  const source = asAuditRecord(value);
  if (!source) return "Nepodržana vrijednost";
  return Object.entries(source)
    .slice(0, 12)
    .map(
      ([key, item]) =>
        `${key}: ${sensitive.test(key) ? "Skriveno" : auditValue(item, depth + 1)}`,
    )
    .join("; ");
}

function ChangeRows({
  item,
  fields,
  formatValue,
}: {
  item: AuditItem;
  fields: AuditFieldLabels;
  formatValue?: (key: string, value: unknown) => string | undefined;
}) {
  const before = asAuditRecord(item.previousValues) ?? {};
  const after = asAuditRecord(item.newValues) ?? {};
  const keys = [
    ...new Set([...Object.keys(before), ...Object.keys(after)]),
  ].filter((key) => !sensitive.test(key));
  if (!keys.length) return null;
  return (
    <dl className="grid gap-3 text-sm">
      {keys.map((key) => (
        <div key={key} className="grid gap-2 border-t pt-3 sm:grid-cols-3">
          <dt className="font-medium">{fields[key] ?? key}</dt>
          <dd>
            <span className="text-muted-foreground">Prije: </span>
            {formatValue?.(key, before[key]) ?? auditValue(before[key])}
          </dd>
          <dd>
            <span className="text-muted-foreground">Poslije: </span>
            {formatValue?.(key, after[key]) ?? auditValue(after[key])}
          </dd>
        </div>
      ))}
    </dl>
  );
}

export function AuditHistory({
  data,
  labels,
  fields,
  expectedEntityType,
  formatValue,
}: {
  data: PagedAuditHistory | undefined;
  labels: AuditLabels;
  fields: AuditFieldLabels;
  expectedEntityType: string;
  formatValue?: (key: string, value: unknown) => string | undefined;
}) {
  if (!data?.items.length)
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>Još nema zabilježenih promjena</EmptyTitle>
          <EmptyDescription>
            Historija je dostupna od uvođenja audit evidencije.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  return (
    <div className="flex flex-col gap-4">
      {data.items.map((item) => (
        <article
          key={item.id}
          className="flex flex-col gap-4 rounded-xl border p-4"
        >
          <header className="flex flex-wrap items-start justify-between gap-3">
            <div className="flex flex-col gap-1">
              <h3 className="font-medium">
                {labels[item.action] ??
                  `Nepoznata audit radnja: ${item.action}`}
              </h3>
              <p className="text-muted-foreground text-sm">
                {item.actor.displayName || "Nepoznat korisnik"} ·{" "}
                {formatUtcDateTime(item.occurredAtUtc)}
              </p>
            </div>
            <Badge variant="secondary">{item.action}</Badge>
          </header>
          {item.entityType !== expectedEntityType ? (
            <Alert variant="destructive">
              <AlertTitle>Neočekivana vrsta zapisa</AlertTitle>
              <AlertDescription>
                Ovaj događaj nije prikazan kao promjena očekivanog entiteta.
              </AlertDescription>
            </Alert>
          ) : (
            <ChangeRows item={item} fields={fields} formatValue={formatValue} />
          )}
          <Collapsible>
            <CollapsibleTrigger render={<Button variant="ghost" size="sm" />}>
              Tehnički detalji
            </CollapsibleTrigger>
            <CollapsibleContent className="text-muted-foreground pt-3 text-sm">
              {auditValue(item.metadata)}
            </CollapsibleContent>
          </Collapsible>
        </article>
      ))}
    </div>
  );
}

export function AuditLoading() {
  return (
    <div className="flex flex-col gap-3">
      <Skeleton className="h-32 w-full" />
      <Skeleton className="h-32 w-full" />
    </div>
  );
}

export function AuditError({ retry }: { retry: () => void }) {
  return (
    <Alert variant="destructive">
      <AlertTitle>Historiju promjena nije moguće učitati</AlertTitle>
      <AlertDescription className="flex flex-wrap items-center gap-3">
        Pokušajte ponovo.
        <Button variant="outline" onClick={retry}>
          Pokušaj ponovo
        </Button>
      </AlertDescription>
    </Alert>
  );
}

export function AuditPagination({
  data,
  onPage,
}: {
  data: PagedAuditHistory;
  onPage: (page: number) => void;
}) {
  if (data.totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-between gap-3">
      <p className="text-muted-foreground text-sm">
        Stranica {data.page} od {data.totalPages} · {data.totalCount} zapisa
      </p>
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={data.page <= 1}
          onClick={() => onPage(data.page - 1)}
        >
          Prethodna
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={data.page >= data.totalPages}
          onClick={() => onPage(data.page + 1)}
        >
          Sljedeća
        </Button>
      </div>
    </div>
  );
}

export function AuditFilterBar({
  actions,
  value,
  onChange,
  compact = false,
}: {
  actions: AuditLabels;
  value: {
    action: string | null;
    dateFrom: string | null;
    dateTo: string | null;
  };
  onChange: (
    next: Partial<{
      action: string | null;
      dateFrom: string | null;
      dateTo: string | null;
    }>,
  ) => void;
  compact?: boolean;
}) {
  const hasFilters = Boolean(value.action || value.dateFrom || value.dateTo);
  return (
    <section className="rounded-xl border p-4">
      <FieldGroup
        className={
          compact
            ? "flex flex-col gap-3"
            : "grid gap-3 md:grid-cols-2 xl:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)_minmax(0,1fr)_auto] xl:items-end"
        }
      >
        <Field>
          <FieldLabel>Radnja</FieldLabel>
          <Select
            value={value.action ?? "all"}
            onValueChange={(action) =>
              onChange({ action: action === "all" ? null : action })
            }
          >
            <SelectTrigger className="w-full">
              <SelectValue>
                {value.action
                  ? (actions[value.action] ?? value.action)
                  : "Sve radnje"}
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectItem value="all">Sve radnje</SelectItem>
                {Object.entries(actions).map(([code, label]) => (
                  <SelectItem key={code} value={code}>
                    {label}
                  </SelectItem>
                ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </Field>
        <Field>
          <FieldLabel htmlFor="audit-date-from">Datum od</FieldLabel>
          <DatePicker
            id="audit-date-from"
            value={value.dateFrom ?? undefined}
            onChange={(dateFrom) => onChange({ dateFrom })}
            placeholder="Odaberite datum"
          />
        </Field>
        <Field>
          <FieldLabel htmlFor="audit-date-to">Datum do</FieldLabel>
          <DatePicker
            id="audit-date-to"
            value={value.dateTo ?? undefined}
            onChange={(dateTo) => onChange({ dateTo })}
            placeholder="Odaberite datum"
          />
        </Field>
        {hasFilters ? (
          <Button
            className={compact ? "w-full" : undefined}
            variant="outline"
            onClick={() =>
              onChange({ action: null, dateFrom: null, dateTo: null })
            }
          >
            Očisti filtere
          </Button>
        ) : null}
      </FieldGroup>
    </section>
  );
}
