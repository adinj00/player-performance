using System.Text.Json;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.Application.Imports;

public sealed record CreateImportRequest(Guid TeamId, Guid? MatchId, ImportType ImportType, ImportSourceSystem SourceSystem, string? SourceLabel, string? Description, string OriginalFileName, string? ContentType, long? DeclaredLength, Stream Content);
public sealed record ImportListQuery(Guid? TeamId = null, Guid? MatchId = null, ImportType? ImportType = null, ImportSourceSystem? SourceSystem = null, ImportFileFormat? FileFormat = null, ImportJobStatus? Status = null, string? Search = null, DateTime? DateFrom = null, DateTime? DateTo = null, int Page = 1, int PageSize = 25);
public sealed record ImportJobResponse(Guid Id, Guid TeamId, Guid? MatchId, ImportType ImportType, ImportSourceSystem SourceSystem, string? SourceLabel, ImportFileFormat FileFormat, ImportJobStatus Status, string? Description, string OriginalFileName, string ContentType, long SizeBytes, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, int ConfigurationRevision, int? ValidatedConfigurationRevision, DateTime? ValidatedAtUtc, DateTime? PreviewGeneratedAtUtc, DateTime? ValidationCompletedAtUtc, int? TotalRowCount, int? PreviewRowCount, int? ValidRowCount, int? InvalidRowCount, int? WarningCount, string? FailureCode, string? FailureMessage, DateTime? ConfirmedAtUtc, DateTime? CancelledAtUtc, IReadOnlyList<ImportAllowedAction> AllowedActions);
public sealed record PagedImportJobsResponse(IReadOnlyList<ImportJobResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed record ImportSourceResponse(Stream Content, string ContentType, string OriginalFileName);
public sealed record ImportPreviewResponse(IReadOnlyList<ImportPreviewColumnResponse> Columns, IReadOnlyList<ImportPreviewRowResponse> Rows, int Page, int PageSize, int TotalCount);
public sealed record ImportPreviewColumnResponse(int Ordinal, string SourceHeader, string NormalizedHeader, string? DetectedDataType);
public sealed record ImportPreviewRowResponse(int SourceRowNumber, JsonElement Values);
public sealed record ImportValidationIssueResponse(ImportValidationSeverity Severity, string Code, string Message, int? SourceRowNumber, string? ColumnKey, JsonElement Metadata, DateTime CreatedAtUtc);
public sealed record PagedImportValidationIssuesResponse(IReadOnlyList<ImportValidationIssueResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed record ImportProcessorCapability(ImportType ImportType, ImportSourceSystem SourceSystem, ImportFileFormat FileFormat, bool CanPreview, bool CanValidate, bool CanConfirm, string ProcessorKey, string ProcessorVersion);
public sealed record ImportCapabilitiesResponse(long MaxUploadSizeBytes, int PreviewRowLimit, int ProcessingLeaseTimeoutMinutes, IReadOnlyList<ImportFileTypeCapability> FileTypes, IReadOnlyList<ImportType> ImportTypes, IReadOnlyList<ImportSourceSystem> SourceSystems, IReadOnlyList<ImportProcessorCapability> ProcessorCapabilities);
public sealed record ImportFileTypeCapability(ImportFileFormat FileFormat, IReadOnlyList<string> Extensions,
IReadOnlyList<string> ContentTypes);
public interface IImportService
{
    Task<Result<ImportCapabilitiesResponse>> GetCapabilitiesAsync(CancellationToken ct);
    Task<Result<ImportJobResponse>> CreateAsync(CreateImportRequest request, CancellationToken ct);
    Task<Result<PagedImportJobsResponse>> ListAsync(ImportListQuery query, CancellationToken ct);
    Task<ImportJobResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<Result<ImportSourceResponse>> OpenSourceAsync(Guid id, CancellationToken ct);
    Task<Result<ImportPreviewResponse>> GetPreviewAsync(Guid id, int page, int pageSize, CancellationToken ct);
    Task<Result<PagedImportValidationIssuesResponse>> GetIssuesAsync(Guid id, ImportValidationSeverity? severity, int page, int pageSize, CancellationToken ct);
    Task<Result> ExecuteAsync(Guid id, ImportProcessingOperation operation, CancellationToken ct);
    Task<Result> CancelAsync(Guid id, CancellationToken ct);
}
public sealed record ImportProcessorContext(Guid ImportJobId, Guid TeamId, Guid? MatchId, ImportType ImportType, ImportSourceSystem SourceSystem, ImportFileFormat FileFormat, int ConfigurationRevision);
public interface IImportWorkflowProcessor
{
    ImportProcessorCapability Capability { get; }
    Task<ImportPreviewResult> GeneratePreviewAsync(ImportProcessorContext context, Stream source, CancellationToken ct);
    Task<ImportValidationResult> ValidateAsync(ImportProcessorContext context, Stream source, CancellationToken ct);
    Task<ImportConfirmationResult> ConfirmAsync(ImportProcessorContext context, Stream source, CancellationToken ct);
}
public interface IImportProcessorRegistry
{
    IImportWorkflowProcessor? Find(ImportType type, ImportSourceSystem source, ImportFileFormat format);
    IReadOnlyList<ImportProcessorCapability> Capabilities { get; }
}
public sealed record ImportPreviewResult(IReadOnlyList<ImportPreviewColumnData> Columns, IReadOnlyList<ImportPreviewRowData> Rows);
public sealed record ImportPreviewColumnData(int Ordinal, string SourceHeader, string NormalizedHeader, string? DetectedDataType);
public sealed record ImportPreviewRowData(int SourceRowNumber, string ValuesJson);
public sealed record ImportValidationResult(IReadOnlyList<ImportValidationIssueData> Issues, int? TotalRowCount, int? ValidRowCount, int? InvalidRowCount, int? WarningCount);
public sealed record ImportValidationIssueData(ImportValidationSeverity Severity, string Code, string Message, int? SourceRowNumber, string? ColumnKey, string MetadataJson);
public sealed record ImportConfirmationResult(string ResultSummaryJson);
