using PlayerPerformance.Domain.Common.Entities;
namespace PlayerPerformance.Domain.Imports;

public sealed class ImportPreviewRow : Entity
{
    private ImportPreviewRow() : base(Guid.Empty)
    {
        ValuesJson = "{}";
    }
    private ImportPreviewRow(Guid id, Guid jobId, int sourceRowNumber, string valuesJson) : base(id)
    {
        ImportJobId = jobId;
        SourceRowNumber = sourceRowNumber;
        ValuesJson = valuesJson;
    }
    public Guid ImportJobId { get; private set; }
    public int SourceRowNumber { get; private set; }
    public string ValuesJson { get; private set; }
    public static ImportPreviewRow Create(Guid id, Guid jobId, int sourceRowNumber, string valuesJson) => new(id, jobId, sourceRowNumber, valuesJson);
}
