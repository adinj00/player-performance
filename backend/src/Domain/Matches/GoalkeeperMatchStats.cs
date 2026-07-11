using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class GoalkeeperMatchStats : Entity
{
    private GoalkeeperMatchStats() : base(Guid.Empty)
    {
    }

    private GoalkeeperMatchStats(Guid id, Guid reportId, Guid appearanceId, DateTime utcNow) : base(id)
    {
        MatchReportId = reportId;
        PlayerMatchAppearanceId = appearanceId;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
    public Guid MatchReportId { get; private set; }
    public Guid PlayerMatchAppearanceId { get; private set; }
    public int? Saves { get; private set; }
    public int? GoalsConceded { get; private set; }
    public bool? CleanSheet { get; private set; }
    public int? PenaltySaves { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public static GoalkeeperMatchStats Create(Guid id, Guid reportId, Guid appearanceId, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(reportId, nameof(reportId));
        Guard.AgainstDefault(appearanceId, nameof(appearanceId));
        return new(id, reportId, appearanceId, utcNow);
    }
    public void Update(GoalkeeperMatchStatsValues values, DateTime utcNow)
    {
        if (values.Saves < 0 || values.GoalsConceded < 0 || values.PenaltySaves < 0)
            throw new ArgumentOutOfRangeException(nameof(values));
        (Saves, GoalsConceded, CleanSheet, PenaltySaves) = values;
        UpdatedAtUtc = utcNow;
    }
}
public sealed record GoalkeeperMatchStatsValues(int? Saves, int? GoalsConceded, bool? CleanSheet, int? PenaltySaves);
