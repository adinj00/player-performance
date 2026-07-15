export type MediaSourceType = "UPLOADED_FILE" | "EXTERNAL_REFERENCE";
export type MediaCategory = "VIDEO" | "IMAGE" | "DOCUMENT" | "OTHER";
export type MediaLinkTargetType = "MATCH" | "MATCH_REPORT" | "PLAYER";

export interface MediaItem {
  id: string;
  teamId: string;
  teamName: string;
  sourceType: MediaSourceType;
  category: MediaCategory;
  title: string;
  description: string | null;
  source: {
    originalFileName: string | null;
    contentType: string | null;
    sizeBytes: number | null;
    url: string | null;
    providerLabel: string | null;
  };
  createdAtUtc: string;
  isArchived: boolean;
}
export interface PagedMedia {
  items: MediaItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface MediaCapabilities {
  maxUploadSizeBytes: number;
  uploadedFileTypes: {
    category: MediaCategory;
    extensions: string[];
    contentTypes: string[];
  }[];
  externalReferenceCategories: MediaCategory[];
}
export interface MediaCandidate {
  id: string;
  targetType: MediaLinkTargetType;
  primaryLabel: string;
  secondaryLabel: string;
  status: string;
}
export interface PagedCandidates {
  items: MediaCandidate[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
