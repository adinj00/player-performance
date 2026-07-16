using System.Globalization;
using System.Text;
using System.Text.Json;
using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.Application.Imports;

public enum TabularDetectedDataType
{
    EMPTY,
    TEXT,
    BOOLEAN,
    INTEGER,
    DECIMAL,
    DATE,
    DATETIME,
    MIXED
}
public enum TabularFailureCode
{
    SOURCE_FORMAT_MISMATCH,
    SOURCE_EMPTY,
    SOURCE_ENCODING_UNSUPPORTED,
    CSV_DELIMITER_AMBIGUOUS,
    CSV_MALFORMED, XLSX_INVALID_CONTAINER,
    XLSX_CONTAINER_LIMIT_EXCEEDED,
    XLSX_NO_NON_EMPTY_WORKSHEET,
    XLSX_MULTIPLE_NON_EMPTY_WORKSHEETS,
    HEADER_NOT_FOUND,
    ROW_LIMIT_EXCEEDED,
    COLUMN_LIMIT_EXCEEDED,
    CELL_LENGTH_LIMIT_EXCEEDED,
    SOURCE_READ_FAILED
}
public sealed class TabularReadException(TabularFailureCode code) : Exception(code.ToString())
{
    public TabularFailureCode Code { get; } = code;
}
public sealed record TabularReadOptions(int PreviewRowLimit, int HeaderScanRowLimit, int CsvDetectionRowLimit, int MaxRows, int MaxColumns, int MaxCellLengthCharacters, int MaxXlsxEntryCount, long MaxXlsxUncompressedSizeBytes, double MaxXlsxCompressionRatio, string TemporaryWorkingDirectory, long MaxSourceBytes);
public sealed record TabularColumn(int Ordinal, string SourceHeader, string NormalizedHeader, TabularDetectedDataType DetectedDataType);
public sealed record TabularRow(int SourceRowNumber, IReadOnlyList<object?> Values);
public sealed record TabularReadResult(string ReaderKey, string ReaderVersion, ImportFileFormat DetectedFileFormat, string? EncodingName, string? Delimiter, string? WorksheetName, IReadOnlyList<TabularColumn> Columns, int TotalRowCount, IReadOnlyList<ImportPreviewRowData> PreviewRows, bool PreviewWasTruncated, IReadOnlyList<string> StructuralIssues);
public interface ITabularSourceReader
{
    ImportFileFormat FileFormat { get; }
    Task<TabularReadResult> ReadAsync(Stream source, TabularReadOptions options, CancellationToken ct);
}
public interface ITabularSourceReaderResolver
{
    ITabularSourceReader Resolve(ImportFileFormat format);
}

public static class TabularHeaders
{
    public static IReadOnlyList<(string Source, string Key)> Normalize(IReadOnlyList<string?> headers)
    {
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        var result = new List<(string, string)>();
        for (var index = 0; index < headers.Count; index++)
        {
            var source = string.Join(' ', (headers[index] ?? string.Empty).Normalize(NormalizationForm.FormC).Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            var key = string.Concat(source.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '_')).Trim('_');
            if (key.Length == 0)
                key = $"column_{index + 1}";
            seen.TryGetValue(key, out var count);
            seen[key] = ++count;
            result.Add((source, count == 1 ? key : $"{key}__{count}"));
        }
        return result;
    }
}

public static class TabularValues
{
    public static TabularDetectedDataType DetectCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TabularDetectedDataType.EMPTY;
        var text = value.Trim();
        if (bool.TryParse(text, out _))
            return TabularDetectedDataType.BOOLEAN;
        if (System.Text.RegularExpressions.Regex.IsMatch(text, "^[+-]?[0-9]+$"))
            return TabularDetectedDataType.INTEGER;
        if (System.Text.RegularExpressions.Regex.IsMatch(text, "^[+-]?(?:[0-9]+\\.[0-9]+|[0-9]*\\.[0-9]+)$") && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            return TabularDetectedDataType.DECIMAL;
        if (DateTime.TryParseExact(text, ["yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", "yyyy-MM-ddTHH:mm:ssK"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
            return text.Contains('T') ? TabularDetectedDataType.DATETIME : TabularDetectedDataType.DATE;
        return TabularDetectedDataType.TEXT;
    }
    public static TabularDetectedDataType DetectValue(object? value) => value switch
    {
        null => TabularDetectedDataType.EMPTY,
        bool => TabularDetectedDataType.BOOLEAN,
        byte or short or int or long => TabularDetectedDataType.INTEGER,
        float or double or decimal => TabularDetectedDataType.DECIMAL,
        DateTime or DateTimeOffset => TabularDetectedDataType.DATETIME,
        _ => DetectCsv(Convert.ToString(value, CultureInfo.InvariantCulture))
    };
    public static TabularDetectedDataType Aggregate(IEnumerable<TabularDetectedDataType> values)
    {
        var types = values.Where(x => x != TabularDetectedDataType.EMPTY).Distinct().ToArray();
        return types.Length switch
        {
            0 => TabularDetectedDataType.EMPTY,
            1 => types[0],
            _ => TabularDetectedDataType.MIXED
        };
    }
    public static object? SafeJsonValue(object? value) => value switch
    {
        null => null,
        bool boolean => boolean,
        byte or short or int or long or decimal => value,
        float number when float.IsFinite(number) => number.ToString(CultureInfo.InvariantCulture),
        double number when double.IsFinite(number) => number.ToString(CultureInfo.InvariantCulture),
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset date => date.ToString("O", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };
    public static string ToJson(IReadOnlyList<TabularColumn> columns, IReadOnlyList<object?> values) => JsonSerializer.Serialize(columns.Select((column, index) => new KeyValuePair<string, object?>(column.NormalizedHeader, SafeJsonValue(index < values.Count ? values[index] : null))).ToDictionary());
}
