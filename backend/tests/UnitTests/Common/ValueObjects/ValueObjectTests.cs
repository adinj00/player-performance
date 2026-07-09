using PlayerPerformance.Domain.Common.ValueObjects;

namespace PlayerPerformance.UnitTests.Common.ValueObjects;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenComponentsMatch()
    {
        var first = new TestValueObject("one", 1);
        var second = new TestValueObject("one", 1);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComponentsDiffer()
    {
        var first = new TestValueObject("one", 1);
        var second = new TestValueObject("two", 1);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    private sealed class TestValueObject(string name, int order) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return name;
            yield return order;
        }
    }
}
