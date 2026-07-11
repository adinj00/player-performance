namespace PlayerPerformance.Domain.Matches;

public static class MatchParticipationRules
{
    public static void ValidatePlayedSnapshot(
        IReadOnlyCollection<MatchLineupSnapshotEntry> lineup,
        Guid? captainPlayerId,
        IReadOnlyCollection<MatchAppearanceSnapshotEntry> appearances,
        IReadOnlyCollection<MatchSubstitutionSnapshotEntry> substitutions)
    {
        var playerIds = new HashSet<Guid>();
        foreach (var entry in lineup)
        {
            if (entry.PlayerId == Guid.Empty || !Enum.IsDefined(entry.Role) || !playerIds.Add(entry.PlayerId))
                throw new InvalidOperationException("Lineup players must be unique and roles must be valid.");
        }

        var starters = lineup.Where(x => x.Role == MatchLineupRole.STARTER).Select(x => x.PlayerId).ToHashSet();
        if (captainPlayerId.HasValue && !starters.Contains(captainPlayerId.Value))
            throw new InvalidOperationException("The captain must be a starter for a played match.");
        if (starters.Count > 0 && !captainPlayerId.HasValue)
            throw new InvalidOperationException("A played lineup with starters requires a captain.");

        var appearanceIds = new HashSet<Guid>();
        foreach (var appearance in appearances)
        {
            if (appearance.PlayerId == Guid.Empty || appearance.MinutesPlayed < 0 || !playerIds.Contains(appearance.PlayerId) || !appearanceIds.Add(appearance.PlayerId))
                throw new InvalidOperationException("Appearances must be unique lineup players with non-negative minutes.");
        }

        var onField = new HashSet<Guid>(starters);
        var entered = new HashSet<Guid>();
        var sequences = new HashSet<int>();
        foreach (var substitution in substitutions.OrderBy(x => x.Sequence))
        {
            if (substitution.Sequence <= 0 || !sequences.Add(substitution.Sequence) || substitution.Minute < 0 || substitution.StoppageTimeMinute < 0 || substitution.PlayerOutId == Guid.Empty || substitution.PlayerInId == Guid.Empty || substitution.PlayerOutId == substitution.PlayerInId || !playerIds.Contains(substitution.PlayerOutId) || !playerIds.Contains(substitution.PlayerInId) || !onField.Remove(substitution.PlayerOutId) || onField.Contains(substitution.PlayerInId))
                throw new InvalidOperationException("The substitution sequence is not a valid on-field transition.");

            onField.Add(substitution.PlayerInId);
            entered.Add(substitution.PlayerInId);
        }

        var expected = starters.Concat(entered).ToHashSet();
        if (!expected.SetEquals(appearanceIds))
            throw new InvalidOperationException("Appearances must exactly match starters and players who enter.");
    }
}

public sealed record MatchLineupSnapshotEntry(Guid PlayerId, MatchLineupRole Role);
public sealed record MatchAppearanceSnapshotEntry(Guid PlayerId, int MinutesPlayed);
public sealed record MatchSubstitutionSnapshotEntry(Guid PlayerOutId, Guid PlayerInId, int Minute, int? StoppageTimeMinute, int Sequence);
