import { getRequiredApiBaseUrl } from "@/lib/env";
import type {
  MediaCapabilities,
  MediaCategory,
  MediaSourceType,
} from "./types";

export const categoryLabels: Record<MediaCategory, string> = {
  VIDEO: "Video",
  IMAGE: "Slika",
  DOCUMENT: "Dokument",
  OTHER: "Ostalo",
};
export const sourceLabels: Record<MediaSourceType, string> = {
  UPLOADED_FILE: "Učitani fajl",
  EXTERNAL_REFERENCE: "Vanjska referenca",
};
export function formatBytes(bytes: number | null) {
  if (bytes === null) return "—";
  const units = ["B", "KB", "MB", "GB"];
  const index = Math.min(
    Math.floor(Math.log(Math.max(bytes, 1)) / Math.log(1024)),
    units.length - 1,
  );
  return `${new Intl.NumberFormat("bs-BA", { maximumFractionDigits: 1 }).format(bytes / 1024 ** index)} ${units[index]}`;
}
export function contentUrl(mediaId: string, download = false) {
  return `${getRequiredApiBaseUrl()}/api/media/${mediaId}/content${download ? "?download=true" : ""}`;
}
export function safeHost(value: string | null) {
  try {
    return value ? new URL(value).host : "—";
  } catch {
    return "—";
  }
}
export function acceptFor(
  capabilities: MediaCapabilities | undefined,
  category: MediaCategory,
) {
  return (
    capabilities?.uploadedFileTypes
      .find((item) => item.category === category)
      ?.extensions.join(",") ?? ""
  );
}
