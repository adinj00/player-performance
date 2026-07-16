using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Imports;
namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class ImportProcessorRegistry(IEnumerable<IImportWorkflowProcessor> processors) : IImportProcessorRegistry
{
    private readonly IReadOnlyList<IImportWorkflowProcessor> processors = processors.ToList();
    public IReadOnlyList<ImportProcessorCapability> Capabilities => processors.Select(x => x.Capability).OrderBy(x => x.ImportType).ThenBy(x => x.SourceSystem).ThenBy(x => x.FileFormat).ToList();
    public IImportWorkflowProcessor? Find(ImportType type, ImportSourceSystem source, ImportFileFormat format) => processors.SingleOrDefault(x => x.Capability is { ImportType: var t, SourceSystem: var s, FileFormat: var f } && t == type && s == source && f == format);
}
