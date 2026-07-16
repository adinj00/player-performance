using PlayerPerformance.Domain.Common.Entities;
namespace PlayerPerformance.Domain.Imports;

public sealed class ImportPreviewColumn : Entity
{
    private ImportPreviewColumn() : base(Guid.Empty)
    {
        SourceHeader = string.Empty;
        NormalizedHeader = string.Empty;
    }
    private ImportPreviewColumn(Guid id, Guid jobId, int ordinal, string sourceHeader, string normalizedHeader, string? detectedDataType) : base(id)
    {
        ImportJobId = jobId;
        Ordinal = ordinal;
        SourceHeader = sourceHeader;
        NormalizedHeader = normalizedHeader;
        DetectedDataType = detectedDataType;
    }
    public Guid ImportJobId { get; private set; }
    public int Ordinal { get; private set; }
    public string SourceHeader { get; private set; }
    public string NormalizedHeader { get; private set; }
    public string? DetectedDataType { get; private set; }
    public static ImportPreviewColumn Create(Guid id, Guid jobId, int ordinal, string sourceHeader, string normalizedHeader, string? detectedDataType) => new(id, jobId, ordinal, sourceHeader, normalizedHeader, detectedDataType);
}
