export type ImportType =
  "PLAYER_ROSTER" | "MATCH_PLAYER_STATISTICS" | "MATCH_GPS" | "TRAINING_GPS";
export type ImportSourceSystem = "GENERIC" | "GPEXE" | "ZONE14" | "OTHER";
export type ImportFileFormat = "CSV" | "XLSX";
export type ImportStatus =
  | "UPLOADED"
  | "PARSING"
  | "VALIDATION_FAILED"
  | "READY_TO_CONFIRM"
  | "IMPORTED"
  | "FAILED"
  | "CANCELLED";
export type ImportAction =
  "GENERATE_PREVIEW" | "VALIDATE" | "CONFIRM" | "CANCEL";

export interface ImportFileTypeCapability {
  fileFormat: ImportFileFormat;
  extensions: string[];
  contentTypes: string[];
}
export interface ImportProcessorCapability {
  importType: ImportType;
  sourceSystem: ImportSourceSystem;
  fileFormat: ImportFileFormat;
  canPreview: boolean;
  canValidate: boolean;
  canConfirm: boolean;
  processorKey: string;
  processorVersion: string;
}
export interface ImportCapabilities {
  maxUploadSizeBytes: number;
  previewRowLimit: number;
  processingLeaseTimeoutMinutes: number;
  fileTypes: ImportFileTypeCapability[];
  importTypes: ImportType[];
  sourceSystems: ImportSourceSystem[];
  processorCapabilities: ImportProcessorCapability[];
}
export interface ImportJob {
  id: string;
  teamId: string;
  matchId: string | null;
  importType: ImportType;
  sourceSystem: ImportSourceSystem;
  sourceLabel: string | null;
  fileFormat: ImportFileFormat;
  status: ImportStatus;
  description: string | null;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  configurationRevision: number;
  validatedConfigurationRevision: number | null;
  validatedAtUtc: string | null;
  previewGeneratedAtUtc: string | null;
  validationCompletedAtUtc: string | null;
  totalRowCount: number | null;
  previewRowCount: number | null;
  validRowCount: number | null;
  invalidRowCount: number | null;
  warningCount: number | null;
  failureCode: string | null;
  failureMessage: string | null;
  confirmedAtUtc: string | null;
  cancelledAtUtc: string | null;
  allowedActions: ImportAction[];
}
export interface PagedImports {
  items: ImportJob[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface Preview {
  columns: {
    ordinal: number;
    sourceHeader: string;
    normalizedHeader: string;
    detectedDataType: string | null;
  }[];
  rows: { sourceRowNumber: number; values: Record<string, unknown> }[];
  page: number;
  pageSize: number;
  totalCount: number;
}
export interface ValidationIssues {
  items: {
    severity: "ERROR" | "WARNING";
    code: string;
    message: string;
    sourceRowNumber: number | null;
    columnKey: string | null;
    createdAtUtc: string;
  }[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface ImportFilters {
  search?: string | null;
  teamId?: string | null;
  matchId?: string | null;
  importType?: ImportType | null;
  sourceSystem?: ImportSourceSystem | null;
  fileFormat?: ImportFileFormat | null;
  status?: ImportStatus | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  page?: number;
}
