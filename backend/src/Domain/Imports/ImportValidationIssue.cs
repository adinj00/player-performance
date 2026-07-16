using PlayerPerformance.Domain.Common.Entities;
namespace PlayerPerformance.Domain.Imports;

public sealed class ImportValidationIssue : Entity
{
    private ImportValidationIssue() : base(Guid.Empty)
    {
        Code = string.Empty;
        Message = string.Empty;
        MetadataJson = "{}";
    }
    private ImportValidationIssue(Guid id, Guid jobId, ImportValidationSeverity severity, string code, string message, int? sourceRowNumber, string? columnKey, string metadataJson, DateTime createdAtUtc) : base(id)
    {
        ImportJobId = jobId;
        Severity = severity;
        Code = code;
        Message = message;
        SourceRowNumber = sourceRowNumber;
        ColumnKey = columnKey;
        MetadataJson = metadataJson;
        CreatedAtUtc = createdAtUtc;
    }
    public Guid ImportJobId { get; private set; }
    public ImportValidationSeverity Severity { get; private set; }
    public string Code { get; private set; }
    public string Message { get; private set; }
    public int? SourceRowNumber { get; private set; }
    public string? ColumnKey { get; private set; }
    public string MetadataJson { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static ImportValidationIssue Create(Guid id, Guid jobId, ImportValidationSeverity severity, string code, string message, int? sourceRowNumber, string? columnKey, string metadataJson, DateTime createdAtUtc) => new(id, jobId, severity, code, message, sourceRowNumber, columnKey, metadataJson, createdAtUtc);
}
