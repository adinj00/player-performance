using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class GenericPreviewImportProcessor(ITabularSourceReaderResolver readers, Microsoft.Extensions.Options.IOptions<ImportOptions> options, ImportFileFormat format) : IImportWorkflowProcessor
{
    public ImportProcessorCapability Capability { get; } = new(ImportType.PLAYER_ROSTER, ImportSourceSystem.GENERIC, format, true, false, false, format == ImportFileFormat.CSV ? "generic-csv-preview" : "generic-xlsx-preview", "1.0.0");
    public bool IsGenericFallback => true;
    public async Task<ImportPreviewResult> GeneratePreviewAsync(ImportProcessorContext context, Stream source, CancellationToken ct)
    {
        var settings = options.Value;
        var result = await readers.Resolve(context.FileFormat).ReadAsync(source, new TabularReadOptions(settings.PreviewRowLimit, settings.HeaderScanRowLimit, settings.CsvDetectionRowLimit, settings.MaxRowsPerFile, settings.MaxColumnsPerFile, settings.MaxCellLengthCharacters, settings.MaxXlsxEntryCount, settings.MaxXlsxUncompressedSizeBytes, settings.MaxXlsxCompressionRatio, settings.TemporaryWorkingDirectory, settings.MaxUploadSizeBytes), ct);
        return new(result.Columns.Select(x => new ImportPreviewColumnData(x.Ordinal, x.SourceHeader, x.NormalizedHeader, x.DetectedDataType.ToString())).ToArray(), result.PreviewRows, result.TotalRowCount, result.PreviewWasTruncated, System.Text.Json.JsonSerializer.Serialize(new
        {
            result.ReaderKey,
            result.ReaderVersion,
            detectedFileFormat = result.DetectedFileFormat.ToString(),
            result.EncodingName,
            result.Delimiter,
            result.WorksheetName,
            columnCount = result.Columns.Count,
            result.TotalRowCount,
            previewRowCount = result.PreviewRows.Count,
            result.PreviewWasTruncated,
            structuralWarningCount = result.StructuralIssues.Count
        }));
    }
    public Task<ImportValidationResult> ValidateAsync(ImportProcessorContext context, Stream source, CancellationToken ct) => throw new NotSupportedException();
    public Task<ImportConfirmationResult> ConfirmAsync(ImportProcessorContext context, Stream source, CancellationToken ct) => throw new NotSupportedException();
}
