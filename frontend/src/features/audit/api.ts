import { apiRequest } from "@/lib/api/api-client";
import {
  parsePagedAuditHistory,
  type AuditFilters,
  type PagedAuditHistory,
} from "@/features/audit/types";

function query(filters: AuditFilters) {
  const params = new URLSearchParams();
  if (filters.action) params.set("action", filters.action);
  if (filters.dateFrom)
    params.set("dateFrom", `${filters.dateFrom}T00:00:00.000Z`);
  if (filters.dateTo) params.set("dateTo", `${filters.dateTo}T23:59:59.999Z`);
  params.set("page", String(filters.page ?? 1));
  params.set("pageSize", String(filters.pageSize ?? 25));
  return params.toString();
}

async function history(
  path: string,
  filters: AuditFilters,
): Promise<PagedAuditHistory> {
  const response = await apiRequest<unknown>(`${path}?${query(filters)}`);
  return parsePagedAuditHistory(response);
}

export function getMatchReportAudit(reportId: string, filters: AuditFilters) {
  return history(`/api/match-reports/${reportId}/audit`, filters);
}

export function getStaffUserAudit(userId: string, filters: AuditFilters) {
  return history(`/api/users/${userId}/audit`, filters);
}

export function getImportAudit(importJobId: string, filters: AuditFilters) {
  return history(`/api/imports/${importJobId}/audit`, filters);
}
