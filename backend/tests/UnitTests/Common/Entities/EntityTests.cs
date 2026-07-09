using PlayerPerformance.Domain.Common.Entities;

namespace PlayerPerformance.UnitTests.Common.Entities;

public sealed class EntityTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenConcreteTypeAndIdMatch()
    {
        var id = Guid.NewGuid();
        var first = new TestEntity(id);
        var second = new TestEntity(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenIdsDiffer()
    {
        var first = new TestEntity(Guid.NewGuid());
        var second = new TestEntity(Guid.NewGuid());

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenConcreteTypesDiffer()
    {
        var id = Guid.NewGuid();
        var first = new TestEntity(id);
        var second = new DifferentTestEntity(id);

        Assert.False(first.Equals(second));
    }

    private sealed class TestEntity(Guid id) : Entity(id);

    private sealed class DifferentTestEntity(Guid id) : Entity(id);
}
