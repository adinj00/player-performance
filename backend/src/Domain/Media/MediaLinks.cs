using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Media;

public abstract class MediaLink : Entity
{
    protected MediaLink(Guid id, Guid mediaItemId, Guid actorUserId, DateTime linkedAtUtc) : base(id)
    {
        Guard.AgainstDefault(mediaItemId, nameof(mediaItemId));
        Guard.AgainstDefault(actorUserId, nameof(actorUserId));
        MediaItemId = mediaItemId;
        LinkedByUserId = actorUserId;
        LinkedAtUtc = linkedAtUtc;
    }

    protected MediaLink() : base(Guid.Empty) { }
    public Guid MediaItemId { get; private set; }
    public Guid LinkedByUserId { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid? UnlinkedByUserId { get; private set; }
    public DateTime? UnlinkedAtUtc { get; private set; }
    public bool IsActive => UnlinkedAtUtc is null;
    public void Unlink(Guid actorUserId, DateTime utcNow)
    {
        Guard.AgainstDefault(actorUserId, nameof(actorUserId));
        if (!IsActive)
            throw new InvalidOperationException("The media link is already inactive.");
        UnlinkedByUserId = actorUserId;
        UnlinkedAtUtc = utcNow;
    }
}

public sealed class MediaMatchLink : MediaLink
{
    private MediaMatchLink() { }
    private MediaMatchLink(Guid id, Guid mediaItemId, Guid matchId, Guid actorUserId, DateTime linkedAtUtc) : base(id, mediaItemId, actorUserId, linkedAtUtc) => MatchId = matchId;
    public Guid MatchId { get; private set; }
    public static MediaMatchLink Create(Guid id, Guid mediaItemId, Guid matchId, Guid actorUserId, DateTime linkedAtUtc)
    {
        Guard.AgainstDefault(matchId, nameof(matchId));
        return new(id, mediaItemId, matchId, actorUserId, linkedAtUtc);
    }
}
public sealed class MediaMatchReportLink : MediaLink
{
    private MediaMatchReportLink() { }
    private MediaMatchReportLink(Guid id, Guid mediaItemId, Guid matchReportId, Guid actorUserId, DateTime linkedAtUtc) : base(id, mediaItemId, actorUserId, linkedAtUtc) => MatchReportId = matchReportId;
    public Guid MatchReportId { get; private set; }
    public static MediaMatchReportLink Create(Guid id, Guid mediaItemId, Guid reportId, Guid actorUserId, DateTime linkedAtUtc)
    {
        Guard.AgainstDefault(reportId, nameof(reportId));
        return new(id, mediaItemId, reportId, actorUserId, linkedAtUtc);
    }
}
public sealed class MediaPlayerLink : MediaLink
{
    private MediaPlayerLink() { }
    private MediaPlayerLink(Guid id, Guid mediaItemId, Guid playerId, Guid actorUserId, DateTime linkedAtUtc) : base(id, mediaItemId, actorUserId, linkedAtUtc) => PlayerId = playerId;
    public Guid PlayerId { get; private set; }
    public static MediaPlayerLink Create(Guid id, Guid mediaItemId, Guid playerId, Guid actorUserId, DateTime linkedAtUtc)
    {
        Guard.AgainstDefault(playerId, nameof(playerId));
        return new(id, mediaItemId, playerId, actorUserId, linkedAtUtc);
    }
}
