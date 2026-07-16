using System.IO;
using PlayerPerformance.Domain.Imports;
namespace PlayerPerformance.Application.Imports;

public static class ImportUploadRules
{
    private static readonly IReadOnlyDictionary<ImportFileFormat, IReadOnlyDictionary<string, string[]>> Accepted = new Dictionary<ImportFileFormat, IReadOnlyDictionary<string, string[]>>
    {
        [ImportFileFormat.CSV] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { [".csv"] = ["text/csv", "application/csv", "application/vnd.ms-excel"] },
        [ImportFileFormat.XLSX] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] }
    };
    public static bool TryGetFormat(string fileName, string? contentType, out ImportFileFormat format)
    {
        format = default;
        if (string.IsNullOrWhiteSpace(contentType))
            return false;
        var extension = Path.GetExtension(fileName);
        foreach (var pair in Accepted)
            if (pair.Value.TryGetValue(extension, out var types) && types.Contains(contentType.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                format = pair.Key;
                return true;
            }
        return false;
    }
    public static IReadOnlyList<ImportFileTypeCapability> Capabilities => Accepted.OrderBy(x => x.Key).Select(x => new ImportFileTypeCapability(x.Key, x.Value.Keys.OrderBy(v => v).ToList(), x.Value.Values.SelectMany(v => v).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList())).ToList();
}
