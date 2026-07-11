using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class PlayerMatchStats : Entity
{
    private PlayerMatchStats() : base(Guid.Empty) { }
    private PlayerMatchStats(Guid id, Guid reportId, Guid appearanceId, DateTime utcNow) : base(id)
    {
        MatchReportId = reportId;
        PlayerMatchAppearanceId = appearanceId;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchReportId { get; private set; }
    public Guid PlayerMatchAppearanceId { get; private set; }
    public int? Goals { get; private set; }
    public int? Assists { get; private set; }
    public int? YellowCards { get; private set; }
    public int? RedCards { get; private set; }
    public int? Shots { get; private set; }
    public int? ShotsOnTarget { get; private set; }
    public int? PassesAttempted { get; private set; }
    public int? PassesCompleted { get; private set; }
    public int? KeyPasses { get; private set; }
    public int? DuelsAttempted { get; private set; }
    public int? DuelsWon { get; private set; }
    public int? FoulsCommitted { get; private set; }
    public int? FoulsWon { get; private set; }
    public int? Offsides { get; private set; }
    public int? BallRecoveries { get; private set; }
    public int? PossessionLosses { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static PlayerMatchStats Create(Guid id, Guid reportId, Guid appearanceId, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(reportId, nameof(reportId));
        Guard.AgainstDefault(appearanceId, nameof(appearanceId));
        return new(id, reportId, appearanceId, utcNow);
    }
    public void Update(PlayerMatchStatsValues values, DateTime utcNow)
    {
        Validate(values);
        (Goals, Assists, YellowCards, RedCards, Shots, ShotsOnTarget, PassesAttempted, PassesCompleted, KeyPasses, DuelsAttempted, DuelsWon, FoulsCommitted, FoulsWon, Offsides, BallRecoveries, PossessionLosses) = values;
        UpdatedAtUtc = utcNow;
    }
    public static void Validate(PlayerMatchStatsValues v)
    {
        foreach (var value in v.Values())
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(v));
        if (v.Shots is not null && v.ShotsOnTarget is not null && v.ShotsOnTarget > v.Shots)
            throw new ArgumentOutOfRangeException(nameof(v));
        if (v.PassesAttempted is not null && v.PassesCompleted is not null && v.PassesCompleted > v.PassesAttempted)
            throw new ArgumentOutOfRangeException(nameof(v));
        if (v.DuelsAttempted is not null && v.DuelsWon is not null && v.DuelsWon > v.DuelsAttempted)
            throw new ArgumentOutOfRangeException(nameof(v));
    }
}

public sealed record PlayerMatchStatsValues(int? Goals, int? Assists, int? YellowCards, int? RedCards, int? Shots, int? ShotsOnTarget, int? PassesAttempted, int? PassesCompleted, int? KeyPasses, int? DuelsAttempted, int? DuelsWon, int? FoulsCommitted, int? FoulsWon, int? Offsides, int? BallRecoveries, int? PossessionLosses)
{
    public IEnumerable<int?> Values() => [Goals, Assists, YellowCards, RedCards, Shots, ShotsOnTarget, PassesAttempted, PassesCompleted, KeyPasses, DuelsAttempted, DuelsWon, FoulsCommitted, FoulsWon, Offsides, BallRecoveries, PossessionLosses];
}
