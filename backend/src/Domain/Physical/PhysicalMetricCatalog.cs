namespace PlayerPerformance.Domain.Physical;

public enum PhysicalMetricValueKind
{
    DECIMAL,
    INTEGER,
    DURATION
}
public enum PhysicalMetricUnit
{
    METERS,
    METERS_PER_SECOND,
    METERS_PER_SECOND_SQUARED,
    COUNT,
    SECONDS,
    ARBITRARY_UNITS
}
public enum PhysicalMetricAggregationKind
{
    SUM,
    MAX,
    LATEST
}
public enum ThresholdDirection
{
    ABOVE_OR_EQUAL,
    BELOW_OR_EQUAL
}
public enum ThresholdScope
{
    TEAM,
    PLAYER,
    SOURCE_DEFINED
}
public sealed record PhysicalMetricDefinition(string Code, PhysicalMetricValueKind ValueKind, PhysicalMetricUnit CanonicalUnit, PhysicalMetricAggregationKind AggregationKind, bool RequiresThresholdContext, bool RequiresMethodContext); public interface IPhysicalMetricCatalog { IReadOnlyList<PhysicalMetricDefinition> All { get; } PhysicalMetricDefinition? Find(string code); }
public sealed class PhysicalMetricCatalog : IPhysicalMetricCatalog
{
    private static readonly IReadOnlyList<PhysicalMetricDefinition> D = new PhysicalMetricDefinition[] {
        new("TOTAL_DISTANCE_METERS", PhysicalMetricValueKind.DECIMAL, PhysicalMetricUnit.METERS, PhysicalMetricAggregationKind.SUM, false, false), new("HIGH_SPEED_RUNNING_DISTANCE_METERS", PhysicalMetricValueKind.DECIMAL, PhysicalMetricUnit.METERS, PhysicalMetricAggregationKind.SUM, true, false), new("SPRINT_DISTANCE_METERS", PhysicalMetricValueKind.DECIMAL, PhysicalMetricUnit.METERS, PhysicalMetricAggregationKind.SUM, true, false), new("SPRINT_COUNT", PhysicalMetricValueKind.INTEGER, PhysicalMetricUnit.COUNT, PhysicalMetricAggregationKind.SUM, true, false), new("MAX_SPEED_METERS_PER_SECOND", PhysicalMetricValueKind.DECIMAL, PhysicalMetricUnit.METERS_PER_SECOND, PhysicalMetricAggregationKind.MAX, false, false), new("ACCELERATION_COUNT", PhysicalMetricValueKind.INTEGER, PhysicalMetricUnit.COUNT, PhysicalMetricAggregationKind.SUM, true, false), new("DECELERATION_COUNT", PhysicalMetricValueKind.INTEGER, PhysicalMetricUnit.COUNT, PhysicalMetricAggregationKind.SUM, true, false), new("PLAYER_LOAD_ARBITRARY_UNITS", PhysicalMetricValueKind.DECIMAL, PhysicalMetricUnit.ARBITRARY_UNITS, PhysicalMetricAggregationKind.SUM, false, true), new("SESSION_DURATION_SECONDS", PhysicalMetricValueKind.DURATION, PhysicalMetricUnit.SECONDS, PhysicalMetricAggregationKind.LATEST, false, false)
        };
    public IReadOnlyList<PhysicalMetricDefinition> All => D;
    public PhysicalMetricDefinition? Find(string c) => D.FirstOrDefault(x => x.Code == c);
}
