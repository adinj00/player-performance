using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.UnitTests.Matches;

public sealed class MatchParticipationRulesTests
{
    [Fact]
    public void ValidatePlayedSnapshot_AllowsAnyStarterCountAndZeroMinutes()
    {
        var starter = Guid.NewGuid();
        MatchParticipationRules.ValidatePlayedSnapshot([new(starter, MatchLineupRole.STARTER)], starter, [new(starter, 0)], []);
    }

    [Fact]
    public void ValidatePlayedSnapshot_RejectsDuplicateLineupPlayers()
    {
        var player = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => MatchParticipationRules.ValidatePlayedSnapshot([new(player, MatchLineupRole.STARTER), new(player, MatchLineupRole.SUBSTITUTE)], player, [new(player, 10)], []));
    }

    [Fact]
    public void ValidatePlayedSnapshot_RequiresCaptainStarterAndAllStarterAppearances()
    {
        var starter = Guid.NewGuid();
        var substitute = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => MatchParticipationRules.ValidatePlayedSnapshot([new(starter, MatchLineupRole.STARTER), new(substitute, MatchLineupRole.SUBSTITUTE)], substitute, [new(starter, 10)], []));
        Assert.Throws<InvalidOperationException>(() => MatchParticipationRules.ValidatePlayedSnapshot([new(starter, MatchLineupRole.STARTER)], starter, [], []));
    }

    [Fact]
    public void ValidatePlayedSnapshot_RequiresEachEnteringSubstituteAndAllowsReturn()
    {
        var starter = Guid.NewGuid();
        var substitute = Guid.NewGuid();
        MatchParticipationRules.ValidatePlayedSnapshot(
            [new(starter, MatchLineupRole.STARTER), new(substitute, MatchLineupRole.SUBSTITUTE)], starter,
            [new(starter, 50), new(substitute, 40)],
            [new(starter, substitute, 50, null, 1), new(substitute, starter, 70, null, 2)]);
    }

    [Fact]
    public void ValidatePlayedSnapshot_RejectsInvalidSubstitutionStateAndMinutes()
    {
        var starter = Guid.NewGuid();
        var substitute = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => MatchParticipationRules.ValidatePlayedSnapshot([new(starter, MatchLineupRole.STARTER), new(substitute, MatchLineupRole.SUBSTITUTE)], starter, [new(starter, 10)], [new(substitute, starter, 1, null, 1)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerMatchAppearance.Create(Guid.NewGuid(), Guid.NewGuid(), starter, -1, DateTime.UtcNow));
        Assert.Throws<ArgumentOutOfRangeException>(() => MatchSubstitution.Create(Guid.NewGuid(), Guid.NewGuid(), starter, substitute, -1, null, 1, DateTime.UtcNow));
    }
}
