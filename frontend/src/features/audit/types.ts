export interface AuditActor {
  id: string;
  displayName: string | null;
}

export interface AuditItem {
  id: string;
  actor: AuditActor;
  action: string;
  entityType: string;
  entityId: string;
  occurredAtUtc: string;
  previousValues: unknown;
  newValues: unknown;
  metadata: unknown;
}

export interface PagedAuditHistory {
  items: AuditItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AuditFilters {
  action?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  page?: number;
  pageSize?: number;
}

function record(value: unknown): Record<string, unknown> | null {
  return value !== null && typeof value === "object" && !Array.isArray(value)
    ? (value as Record<string, unknown>)
    : null;
}

function string(value: unknown): string | null {
  return typeof value === "string" ? value : null;
}

export function parsePagedAuditHistory(value: unknown): PagedAuditHistory {
  const source = record(value);
  const items = Array.isArray(source?.items) ? source.items : [];
  return {
    items: items.flatMap((entry) => {
      const item = record(entry);
      const actor = record(item?.actor);
      const id = string(item?.id);
      const action = string(item?.action);
      const entityType = string(item?.entityType);
      const entityId = string(item?.entityId);
      const occurredAtUtc = string(item?.occurredAtUtc);
      if (
        !item ||
        !actor ||
        !id ||
        !action ||
        !entityType ||
        !entityId ||
        !occurredAtUtc
      )
        return [];
      return [
        {
          id,
          actor: {
            id: string(actor.id) ?? "",
            displayName: string(actor.displayName),
          },
          action,
          entityType,
          entityId,
          occurredAtUtc,
          previousValues: item.previousValues,
          newValues: item.newValues,
          metadata: item.metadata,
        },
      ];
    }),
    page: typeof source?.page === "number" ? source.page : 1,
    pageSize: typeof source?.pageSize === "number" ? source.pageSize : 25,
    totalCount: typeof source?.totalCount === "number" ? source.totalCount : 0,
    totalPages: typeof source?.totalPages === "number" ? source.totalPages : 0,
  };
}

export function asAuditRecord(value: unknown) {
  return record(value);
}
