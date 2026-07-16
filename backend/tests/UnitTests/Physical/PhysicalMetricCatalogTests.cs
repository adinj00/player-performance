using PlayerPerformance.Domain.Physical;

namespace PlayerPerformance.UnitTests.Physical;

public sealed class PhysicalMetricCatalogTests
{
    [Fact]
    public void Catalogue_HasAllStableDefinitions_InDeterministicOrder()
    {
        var catalog = new PhysicalMetricCatalog();
        Assert.Equal(9, catalog.All.Count);
        Assert.Equal("TOTAL_DISTANCE_METERS", catalog.All[0].Code);
        Assert.Equal(PhysicalMetricUnit.ARBITRARY_UNITS, catalog.Find("PLAYER_LOAD_ARBITRARY_UNITS")!.CanonicalUnit);
        Assert.True(catalog.Find("PLAYER_LOAD_ARBITRARY_UNITS")!.RequiresMethodContext);
        Assert.True(catalog.Find("SPRINT_COUNT")!.RequiresThresholdContext);
        Assert.Null(catalog.Find("unconfirmed_vendor_column"));
    }
}
