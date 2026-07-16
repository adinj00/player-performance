using PlayerPerformance.Application.Imports;

namespace PlayerPerformance.UnitTests;

public sealed class TabularContractsTests
{
    [Fact]
    public void Normalize_ShouldCollapseWhitespace_NormalizeUnicode_AndSuffixDuplicates()
    {
        var headers = TabularHeaders.Normalize([" Player   Name ", "Player-Name", "", "Žuti karton", "Player Name"]);

        Assert.Collection(headers,
            header => Assert.Equal(("Player Name", "player_name"), header),
            header => Assert.Equal(("Player-Name", "player_name__2"), header),
            header => Assert.Equal(("", "column_3"), header),
            header => Assert.Equal(("Žuti karton", "žuti_karton"), header),
            header => Assert.Equal(("Player Name", "player_name__3"), header));
    }

    [Theory]
    [InlineData(null, TabularDetectedDataType.EMPTY)]
    [InlineData("  ", TabularDetectedDataType.EMPTY)]
    [InlineData("TRUE", TabularDetectedDataType.BOOLEAN)]
    [InlineData("-12", TabularDetectedDataType.INTEGER)]
    [InlineData("10.25", TabularDetectedDataType.DECIMAL)]
    [InlineData("2026-07-16", TabularDetectedDataType.DATE)]
    [InlineData("2026-07-16T12:00:00Z", TabularDetectedDataType.DATETIME)]
    [InlineData("10,25", TabularDetectedDataType.TEXT)]
    [InlineData("16/07/2026", TabularDetectedDataType.TEXT)]
    public void DetectCsv_ShouldUseOnlyConservativeInvariantForms(string? value, TabularDetectedDataType expected)
    {
        Assert.Equal(expected, TabularValues.DetectCsv(value));
    }

    [Fact]
    public void ToJson_ShouldPreserveNullZeroBooleanAndInvariantDates()
    {
        var columns = new[]
        {
            new TabularColumn(0, "Null", "null", TabularDetectedDataType.EMPTY),
            new TabularColumn(1, "Zero", "zero", TabularDetectedDataType.INTEGER),
            new TabularColumn(2, "Value", "value", TabularDetectedDataType.BOOLEAN),
            new TabularColumn(3, "Date", "date", TabularDetectedDataType.DATETIME)
        };

        var json = TabularValues.ToJson(columns, [null, 0L, true, new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc)]);
        using var document = System.Text.Json.JsonDocument.Parse(json);

        Assert.Equal(System.Text.Json.JsonValueKind.Null, document.RootElement.GetProperty("null").ValueKind);
        Assert.Equal(0, document.RootElement.GetProperty("zero").GetInt64());
        Assert.True(document.RootElement.GetProperty("value").GetBoolean());
        Assert.Equal("2026-07-16T12:00:00.0000000Z", document.RootElement.GetProperty("date").GetString());
    }
}
