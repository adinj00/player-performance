using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Imports;
namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class ImportProcessorRegistry(IEnumerable<IImportWorkflowProcessor> processors) : IImportProcessorRegistry
{
    private readonly IReadOnlyList<IImportWorkflowProcessor> processors = processors.ToList();
    private readonly IReadOnlyDictionary<ImportFileFormat, IImportWorkflowProcessor> fallbacks = BuildFallbacks(processors);
    public IReadOnlyList<ImportProcessorCapability> Capabilities => BuildCapabilities();
    public IImportWorkflowProcessor? Find(ImportType type, ImportSourceSystem source, ImportFileFormat format) => processors.SingleOrDefault(x => !x.IsGenericFallback && x.Capability is { ImportType: var t, SourceSystem: var s, FileFormat: var f } && t == type && s == source && f == format) ?? (fallbacks.TryGetValue(format, out var fallback) ? fallback : null);
    private static IReadOnlyDictionary<ImportFileFormat, IImportWorkflowProcessor> BuildFallbacks(IEnumerable<IImportWorkflowProcessor> values)
    {
        var groups = values.Where(x => x.IsGenericFallback).GroupBy(x => x.Capability.FileFormat).ToArray();
        if (groups.Any(x => x.Count() != 1) || values.Where(x => !x.IsGenericFallback).GroupBy(x => (x.Capability.ImportType, x.Capability.SourceSystem, x.Capability.FileFormat)).Any(x => x.Count() != 1))
            throw new InvalidOperationException("Import processor registrations are ambiguous.");
        return groups.ToDictionary(x => x.Key, x => x.Single());
    }
    private IReadOnlyList<ImportProcessorCapability> BuildCapabilities() => Enum.GetValues<ImportType>().SelectMany(type => Enum.GetValues<ImportSourceSystem>().SelectMany(source => Enum.GetValues<ImportFileFormat>().Select(format =>
    {
        var processor = Find(type, source, format);
        return processor is null ? null : processor.Capability with { ImportType = type, SourceSystem = source, FileFormat = format };
    }))).Where(x => x is not null).Select(x => x!).OrderBy(x => x.ImportType).ThenBy(x => x.SourceSystem).ThenBy(x => x.FileFormat).ToArray();
}
