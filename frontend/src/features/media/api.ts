import { getCsrf } from "@/features/auth/api/auth-api";
import { apiRequest } from "@/lib/api/api-client";
import { getRequiredApiBaseUrl } from "@/lib/env";
import type {
  MediaCapabilities,
  MediaCategory,
  MediaItem,
  MediaLinkTargetType,
  PagedCandidates,
  PagedMedia,
} from "./types";

async function unsafe<T>(
  path: string,
  method: "POST" | "PATCH" | "DELETE",
  json?: unknown,
) {
  const csrf = await getCsrf();
  return apiRequest<T>(path, {
    method,
    json,
    headers: { [csrf.headerName]: csrf.token },
  });
}
export const mediaApi = {
  capabilities: () => apiRequest<MediaCapabilities>("/api/media/capabilities"),
  list: (filters: Record<string, string | number | boolean | null>) => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== "" && value !== false)
        params.set(key, String(value));
    });
    return apiRequest<PagedMedia>(`/api/media?${params}`);
  },
  linked: (targetType: MediaLinkTargetType, targetId: string) =>
    apiRequest<PagedMedia>(
      `/api/media?linkedTargetType=${targetType}&linkedTargetId=${targetId}&page=1&pageSize=25`,
    ),
  get: (id: string) => apiRequest<MediaItem>(`/api/media/${id}`),
  external: (value: {
    teamId: string;
    category: MediaCategory;
    title: string;
    description?: string;
    url: string;
    providerLabel?: string;
  }) => unsafe<MediaItem>("/api/media/external-references", "POST", value),
  update: (
    id: string,
    value: {
      category: MediaCategory;
      title: string;
      description?: string;
      url?: string;
      providerLabel?: string;
    },
  ) => unsafe<MediaItem>(`/api/media/${id}`, "PATCH", value),
  archive: (id: string, action: "archive" | "restore") =>
    unsafe<MediaItem>(`/api/media/${id}/${action}`, "POST"),
  link: (mediaId: string, targetType: MediaLinkTargetType, targetId: string) =>
    unsafe<void>(`/api/media/${mediaId}/${targetType}/${targetId}`, "POST"),
  unlink: (
    mediaId: string,
    targetType: MediaLinkTargetType,
    targetId: string,
  ) =>
    unsafe<void>(`/api/media/${mediaId}/${targetType}/${targetId}`, "DELETE"),
  candidates: (mediaId: string, targetType: MediaLinkTargetType, search = "") =>
    apiRequest<PagedCandidates>(
      `/api/media/${mediaId}/link-candidates?targetType=${targetType}&page=1&pageSize=25${search ? `&search=${encodeURIComponent(search)}` : ""}`,
    ),
  async upload(
    metadata: {
      teamId: string;
      category: MediaCategory;
      title: string;
      description?: string;
    },
    file: File,
    onProgress: (progress: number) => void,
    signal: AbortSignal,
  ) {
    const csrf = await getCsrf();
    return new Promise<MediaItem>((resolve, reject) => {
      const body = new FormData();
      body.append(
        "metadata",
        new Blob([JSON.stringify(metadata)], { type: "application/json" }),
      );
      body.append("file", file, file.name);
      const request = new XMLHttpRequest();
      request.open("POST", `${getRequiredApiBaseUrl()}/api/media/assets`);
      request.withCredentials = true;
      request.setRequestHeader(csrf.headerName, csrf.token);
      request.upload.onprogress = (event) => {
        if (event.lengthComputable)
          onProgress((event.loaded / event.total) * 100);
      };
      request.onerror = () =>
        reject(new Error("Mrežna greška pri učitavanju fajla."));
      request.onabort = () =>
        reject(new DOMException("Upload je prekinut.", "AbortError"));
      request.onload = () => {
        if (request.status >= 200 && request.status < 300) {
          resolve(JSON.parse(request.responseText) as MediaItem);
          return;
        }
        try {
          const problem = JSON.parse(request.responseText) as {
            detail?: string;
            title?: string;
          };
          reject(
            new Error(
              problem.detail ?? problem.title ?? "Fajl nije moguće učitati.",
            ),
          );
        } catch {
          reject(new Error("Fajl nije moguće učitati."));
        }
      };
      signal.addEventListener("abort", () => request.abort(), { once: true });
      request.send(body);
    });
  },
};
