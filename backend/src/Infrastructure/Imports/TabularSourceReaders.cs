using System.IO.Compression;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class TabularSourceReaderResolver(IEnumerable<ITabularSourceReader> readers) : ITabularSourceReaderResolver
{
    private readonly IReadOnlyDictionary<ImportFileFormat, ITabularSourceReader> readers = readers.ToDictionary(x => x.FileFormat);
    public ITabularSourceReader Resolve(ImportFileFormat format) => readers.TryGetValue(format, out var reader) ? reader : throw new InvalidOperationException("No tabular reader is registered.");
}

internal sealed class CsvTabularSourceReader : ITabularSourceReader
{
    public ImportFileFormat FileFormat => ImportFileFormat.CSV;
    public async Task<TabularReadResult> ReadAsync(Stream source, TabularReadOptions options, CancellationToken ct)
    {
        await using var materialized = await SeekableImportSource.CreateAsync(source, options.TemporaryWorkingDirectory, options.MaxSourceBytes, ct);
        var stream = materialized.Stream;
        var encoding = DetectEncoding(stream);
        var delimiter = await DetectDelimiterAsync(stream, encoding, options, ct);
        stream.Position = 0;
        using var text = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var configuration = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = false,
            BadDataFound = _ => throw new TabularReadException(TabularFailureCode.CSV_MALFORMED),
            MissingFieldFound = null,
            DetectDelimiter = false
        };
        using var parser = new CsvParser(text, configuration);
        var rows = new List<(int Source, string?[] Values)>();
        string?[]? header = null;
        var sourceRow = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            string[]? fields;
            try
            {
                if (!parser.Read())
                    break;
                fields = parser.Record;
            }
            catch (TabularReadException)
            {
                throw;
            }
            catch
            {
                throw new TabularReadException(TabularFailureCode.CSV_MALFORMED);
            }
            if (fields is null)
                break;
            sourceRow++;
            if (fields.Any(x => x?.IndexOf('\0') >= 0))
                throw new TabularReadException(TabularFailureCode.SOURCE_ENCODING_UNSUPPORTED);
            if (fields.Any(x => x?.Length > options.MaxCellLengthCharacters))
                throw new TabularReadException(TabularFailureCode.CELL_LENGTH_LIMIT_EXCEEDED);
            if (header is null)
            {
                if (sourceRow > options.HeaderScanRowLimit)
                    throw new TabularReadException(TabularFailureCode.HEADER_NOT_FOUND);
                if (fields.All(string.IsNullOrWhiteSpace))
                    continue;
                header = fields;
                if (header.Length > options.MaxColumns)
                    throw new TabularReadException(TabularFailureCode.COLUMN_LIMIT_EXCEEDED);
                continue;
            }
            rows.Add((sourceRow, fields));
            if (rows.Count > options.MaxRows)
                throw new TabularReadException(TabularFailureCode.ROW_LIMIT_EXCEEDED);
        }
        if (header is null)
            throw new TabularReadException(TabularFailureCode.HEADER_NOT_FOUND);
        return Build("csv", "1.0.0", ImportFileFormat.CSV, encoding.WebName, delimiter, null, header, rows.Select(x => (x.Source, x.Values.Cast<object?>().ToArray())).ToList(), options);
    }
    private static Encoding DetectEncoding(Stream stream)
    {
        stream.Position = 0;
        Span<byte> bom = stackalloc byte[4];
        var count = stream.Read(bom);
        stream.Position = 0;
        if (count >= 3 && bom[..3].SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }))
            return new UTF8Encoding(false, true);
        if (count >= 2 && bom[..2].SequenceEqual(new byte[] { 0xFF, 0xFE }))
            return new UnicodeEncoding(false, true, true);
        if (count >= 2 && bom[..2].SequenceEqual(new byte[] { 0xFE, 0xFF }))
            return new UnicodeEncoding(true, true, true);
        return new UTF8Encoding(false, true);
    }
    private static async Task<string> DetectDelimiterAsync(Stream stream, Encoding encoding, TabularReadOptions options, CancellationToken ct)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, encoding, true, leaveOpen: true);
        var samples = new List<string>();
        for (var i = 0; i < options.CsvDetectionRowLimit; i++)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (line is null)
                break;
            if (!string.IsNullOrWhiteSpace(line))
                samples.Add(line);
        }
        stream.Position = 0;
        if (samples.Count == 0)
            throw new TabularReadException(TabularFailureCode.SOURCE_EMPTY);
        var candidates = new[] { ",", ";", "\t", "|" }.Select(x => new
        {
            Delimiter = x,
            Score = samples.Sum(line => line.Count(c => c == x[0]))
        }).ToList();
        var best = candidates.MaxBy(x => x.Score)!;
        if (best.Score == 0)
            return ",";
        if (candidates.Count(x => x.Score == best.Score) != 1)
            throw new TabularReadException(TabularFailureCode.CSV_DELIMITER_AMBIGUOUS);
        return best.Delimiter;
    }
    internal static TabularReadResult Build(string key, string version, ImportFileFormat format, string? encoding, string? delimiter, string? worksheet, IReadOnlyList<string?> header, IReadOnlyList<(int Source, object?[] Values)> data, TabularReadOptions options)
    {
        var max = Math.Max(header.Count, data.Count == 0 ? 0 : data.Max(x => x.Values.Length));
        if (max > options.MaxColumns)
            throw new TabularReadException(TabularFailureCode.COLUMN_LIMIT_EXCEEDED);
        var headers = header.Concat(Enumerable.Range(header.Count + 1, max - header.Count).Select(x => $"column_{x}")).ToArray();
        var normalized = TabularHeaders.Normalize(headers);
        var types = Enumerable.Range(0, max).Select(index => TabularValues.Aggregate(data.Select(row => index < row.Values.Length ? TabularValues.DetectValue(row.Values[index]) : TabularDetectedDataType.EMPTY))).ToArray();
        var columns = normalized.Select((x, i) => new TabularColumn(i, x.Source, x.Key, types[i])).ToArray();
        var preview = data.Take(options.PreviewRowLimit).Select(row => new ImportPreviewRowData(row.Source, TabularValues.ToJson(columns, row.Values))).ToArray();
        return new TabularReadResult(key, version, format, encoding, delimiter, worksheet, columns, data.Count, preview, data.Count > preview.Length, []);
    }
}

internal sealed class XlsxTabularSourceReader : ITabularSourceReader
{
    public ImportFileFormat FileFormat => ImportFileFormat.XLSX;
    public async Task<TabularReadResult> ReadAsync(Stream source, TabularReadOptions options, CancellationToken ct)
    {
        await using var materialized = await SeekableImportSource.CreateAsync(source, options.TemporaryWorkingDirectory, options.MaxSourceBytes, ct);
        var stream = materialized.Stream;
        Preflight(stream, options);
        stream.Position = 0;
        using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        var sheets = new List<(string Name, List<(int Source, object?[] Values)> Rows)>();
        do
        {
            var rows = new List<(int, object?[])>();
            var row = 0;
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                row++;
                var values = Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? null : reader.GetValue(i)).ToArray();
                if (values.Any(x => x is string s && s.Length > options.MaxCellLengthCharacters))
                    throw new TabularReadException(TabularFailureCode.CELL_LENGTH_LIMIT_EXCEEDED);
                if (values.Any(x => x is not null))
                    rows.Add((row, values));
            }
            if (rows.Count > 0)
                sheets.Add((reader.Name, rows));
        } while (reader.NextResult());
        if (sheets.Count == 0)
            throw new TabularReadException(TabularFailureCode.XLSX_NO_NON_EMPTY_WORKSHEET);
        if (sheets.Count > 1)
            throw new TabularReadException(TabularFailureCode.XLSX_MULTIPLE_NON_EMPTY_WORKSHEETS);
        var sheet = sheets[0];
        var headerIndex = sheet.Rows.FindIndex(x => x.Source <= options.HeaderScanRowLimit);
        if (headerIndex < 0)
            throw new TabularReadException(TabularFailureCode.HEADER_NOT_FOUND);
        var header = sheet.Rows[headerIndex].Values.Select(x => Convert.ToString(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var data = sheet.Rows.Skip(headerIndex + 1).ToList();
        if (data.Count > options.MaxRows)
            throw new TabularReadException(TabularFailureCode.ROW_LIMIT_EXCEEDED);
        return CsvTabularSourceReader.Build("xlsx", "1.0.0", ImportFileFormat.XLSX, null, null, sheet.Name, header, data, options);
    }
    private static void Preflight(Stream stream, TabularReadOptions options)
    {
        try
        {
            stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read, true);
            if (zip.Entries.Count == 0 || zip.Entries.Count > options.MaxXlsxEntryCount)
                throw new TabularReadException(TabularFailureCode.XLSX_CONTAINER_LIMIT_EXCEEDED);
            long total = 0;
            foreach (var entry in zip.Entries)
            { total = checked(total + entry.Length); if (total > options.MaxXlsxUncompressedSizeBytes || entry.CompressedLength > 0 && (double)entry.Length / entry.CompressedLength > options.MaxXlsxCompressionRatio) throw new TabularReadException(TabularFailureCode.XLSX_CONTAINER_LIMIT_EXCEEDED); }
            if (!zip.Entries.Any(x => x.FullName == "[Content_Types].xml") || !zip.Entries.Any(x => x.FullName.StartsWith("xl/worksheets/", StringComparison.Ordinal)))
                throw new TabularReadException(TabularFailureCode.XLSX_INVALID_CONTAINER);
        }
        catch (TabularReadException)
        {
            throw;
        }
        catch
        {
            throw new TabularReadException(TabularFailureCode.XLSX_INVALID_CONTAINER);
        }
    }
}

internal sealed class SeekableImportSource : IAsyncDisposable
{
    private readonly string? path;
    private readonly bool ownsStream;
    public Stream Stream { get; }
    private SeekableImportSource(Stream stream, string? path, bool ownsStream)
    {
        Stream = stream;
        this.path = path;
        this.ownsStream = ownsStream;
    }
    public static async Task<SeekableImportSource> CreateAsync(Stream source, string temporaryDirectory, long maxSourceBytes, CancellationToken ct)
    {
        if (source.CanSeek)
        { source.Position = 0; return new(source, null, false); }
        Directory.CreateDirectory(temporaryDirectory);
        var path = Path.Combine(temporaryDirectory, $"player-performance-import-{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, ct)) > 0)
                {
                    total += read;
                    if (total > maxSourceBytes)
                        throw new TabularReadException(TabularFailureCode.ROW_LIMIT_EXCEEDED);
                    await destination.WriteAsync(buffer.AsMemory(0, read), ct);
                }
            }
            return new(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous), path, true);
        }
        catch
        {
            if (File.Exists(path))
                File.Delete(path);
            throw;
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (ownsStream)
            await Stream.DisposeAsync();
        if (path is not null && File.Exists(path))
            File.Delete(path);
    }
}
