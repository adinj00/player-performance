import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import { getRequiredApiBaseUrl } from "@/lib/env";
import { normalizeApiError } from "@/lib/api/api-errors";
import type {
  ImportCapabilities,
  ImportFilters,
  ImportJob,
  Preview,
  PagedImports,
  ValidationIssues,
} from "@/features/imports/types";

function query(filters: ImportFilters) {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== "")
      params.set(
        key,
        key === "dateFrom"
          ? `${value}T00:00:00.000Z`
          : key === "dateTo"
            ? `${value}T23:59:59.999Z`
            : String(value),
      );
  });
  params.set("pageSize", "25");
  return params.toString();
}
async function mutate(path: string) {
  const csrf = await getCsrf();
  return apiRequest<void>(path, {
    method: "POST",
    headers: { [csrf.headerName]: csrf.token },
  });
}
export const importQueryKeys = {
  capabilities: ["importCapabilities"] as const,
  list: (filters: ImportFilters) => ["importList", filters] as const,
  detail: (id: string) => ["importDetail", id] as const,
  preview: (id: string, page: number) => ["importPreview", id, page] as const,
  issues: (id: string, page: number, severity: string | null) =>
    ["importValidationIssues", id, page, severity] as const,
  audit: (id: string, page: number) => ["importAudit", id, page] as const,
};
export const importsApi = {
  capabilities: () =>
    apiRequest<ImportCapabilities>("/api/imports/capabilities"),
  list: (filters: ImportFilters) =>
    apiRequest<PagedImports>(`/api/imports?${query(filters)}`),
  get: (id: string) => apiRequest<ImportJob>(`/api/imports/${id}`),
  preview: (id: string, page: number) =>
    apiRequest<Preview>(`/api/imports/${id}/preview?page=${page}&pageSize=25`),
  issues: (id: string, page: number, severity: string | null) =>
    apiRequest<ValidationIssues>(
      `/api/imports/${id}/validation-issues?page=${page}&pageSize=25${severity ? `&severity=${severity}` : ""}`,
    ),
  previewAction: (id: string) => mutate(`/api/imports/${id}/preview`),
  validate: (id: string) => mutate(`/api/imports/${id}/validate`),
  confirm: (id: string) => mutate(`/api/imports/${id}/confirm`),
  cancel: (id: string) => mutate(`/api/imports/${id}/cancel`),
  sourceUrl: (id: string) =>
    `${getRequiredApiBaseUrl()}/api/imports/${id}/source`,
  async upload(
    metadata: object,
    file: File,
    onProgress: (value: number) => void,
    signal: AbortSignal,
  ): Promise<ImportJob> {
    const csrf = await getCsrf();
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      const form = new FormData();
      form.append("metadata", JSON.stringify(metadata));
      form.append("file", file, file.name);
      xhr.open("POST", `${getRequiredApiBaseUrl()}/api/imports`);
      xhr.withCredentials = true;
      xhr.setRequestHeader("Accept", "application/json");
      xhr.setRequestHeader(csrf.headerName, csrf.token);
      xhr.upload.onprogress = (event) => {
        if (event.lengthComputable)
          onProgress(Math.round((event.loaded / event.total) * 100));
      };
      xhr.onerror = () =>
        reject(
          normalizeApiError(0, null, "Mrežna greška pri učitavanju fajla."),
        );
      xhr.onload = () => {
        let body: unknown;
        try {
          body = xhr.responseText ? JSON.parse(xhr.responseText) : null;
        } catch {
          body = xhr.responseText;
        }
        if (xhr.status >= 200 && xhr.status < 300) resolve(body as ImportJob);
        else reject(normalizeApiError(xhr.status, body));
      };
      signal.addEventListener("abort", () => xhr.abort(), { once: true });
      xhr.send(form);
    });
  },
};
